//using SixLabors.ImageSharp;
//using SixLabors.ImageSharp.Processing;
using System;
using System.IO;

namespace Resizetizer
{
	internal class Resizer
	{
		public Resizer(SharedImageInfo info, string intermediateOutputPath, ILogger logger)
		{
			Info = info;
			Logger = logger;
			IntermediateOutputPath = intermediateOutputPath;
		}

		public ILogger Logger { get; private set; }
		public string IntermediateOutputPath { get; private set; }

		public SharedImageInfo Info { get; private set; }

		SkiaSharpBitmapTools bmpTools;
		SkiaSharpSvgTools svgTools;

		public string GetFileDestination(DpiPath dpi)
			=> GetFileDestination(Info, dpi, IntermediateOutputPath);

		public static string GetFileDestination(SharedImageInfo info, DpiPath dpi, string intermediateOutputPath)
		{
			var name = Path.GetFileNameWithoutExtension(info.Filename);
			var ext = Path.GetExtension(info.Filename);

			var fullIntermediateOutputPath = new DirectoryInfo(intermediateOutputPath);

			var destination = Path.Combine(fullIntermediateOutputPath.FullName, dpi.Path, name + dpi.FileSuffix + ext);

			var fileInfo = new FileInfo(destination);
			if (!fileInfo.Directory.Exists)
				fileInfo.Directory.Create();

			return destination;
		}

		public static string CopyFile(SharedImageInfo info, DpiPath dpi, string intermediateOutputPath, string inputsFile, ILogger logger, bool isAndroid = false)
		{
			var destination = Resizer.GetFileDestination(info, dpi, intermediateOutputPath);
			var androidVector = false;

			if (isAndroid && info.IsVector && !info.Resize)
			{
				// TODO: Turn SVG into Vector Drawable format
				// Update destination to be .xml file
				destination = Path.ChangeExtension(info.Filename, ".xml");
				androidVector = true;
			}

			if (IsUpToDate(info.Filename, destination, inputsFile, logger))
				return destination;
			
			if (androidVector)
			{
				// TODO: Don't just copy, let's transform to android vector
				File.Copy(info.Filename, destination, true);
			}
			else
			{
				// Otherwise just copy it straight
				File.Copy(info.Filename, destination, true);
			}

			return destination;
		}

		static bool IsUpToDate(string inputFile, string outputFile, string inputsFile, ILogger logger)
		{
			var fileIn = new FileInfo(inputFile);
			var fileOut = new FileInfo(outputFile);
			var fileInputs = new FileInfo(inputsFile);

			if (fileIn.Exists && fileOut.Exists && fileInputs.Exists
				&& fileIn.LastWriteTimeUtc <= fileOut.LastWriteTimeUtc
				&& fileInputs.LastWriteTimeUtc <= fileOut.LastWriteTimeUtc)
			{
				logger.Log($"Skipping '{inputFile}' as output '{outputFile}' is already up to date.");
				return true;
			}

			return false;
		}

		public string Resize(DpiPath dpi, string inputsFile)
		{
			Logger?.Log($"Resizing: {Info.Filename}");

			var destination = GetFileDestination(dpi);

			if (Info.IsVector)
				destination = Path.ChangeExtension(destination, ".png");

			if (IsUpToDate(Info.Filename, destination, inputsFile, Logger))
				return destination;

			if (Info.IsVector)
			{
				if (svgTools == null)
					svgTools = new SkiaSharpSvgTools(Info, Logger);

				svgTools.Resize(dpi, destination);
			}
			else
			{
				if (bmpTools == null)
					bmpTools = new SkiaSharpBitmapTools(Info, Logger);

				bmpTools.Resize(dpi, destination);
			}

			return destination;
		}
	}
}
