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

			if (Info.IsVector)
				svgTools = new SkiaSharpSvgTools(Info, Logger);
			else
				bmpTools = new SkiaSharpBitmapTools(Info, Logger);
		}

		public ILogger Logger { get; private set; }
		public string IntermediateOutputPath { get; private set; }

		public SharedImageInfo Info { get; private set; }

		readonly SkiaSharpBitmapTools bmpTools;
		readonly SkiaSharpSvgTools svgTools;

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

		public static string CopyFile(SharedImageInfo info, DpiPath dpi, string intermediateOutputPath, bool isAndroid = false)
		{
			var destination = Resizer.GetFileDestination(info, dpi, intermediateOutputPath);

			if (isAndroid && info.IsVector && !info.Resize)
			{
				// TODO: Turn SVG into Vector Drawable format
				// Update destination to be .xml file
				destination = Path.ChangeExtension(info.Filename, ".xml");

				File.Copy(info.Filename, destination, true);
			}
			else
			{
				// Otherwise just copy it straight
				File.Copy(info.Filename, destination, true);
			}

			return destination;
		}

		public string Resize(DpiPath dpi)
		{
			Logger?.Log($"Resizing: {Info.Filename}");

			var destination = GetFileDestination(dpi);

			if (Info.IsVector)
			{
				destination = Path.ChangeExtension(destination, ".png");

				svgTools.Resize(dpi, destination);
			}
			else
			{
				bmpTools.Resize(dpi, destination);
			}

			return destination;
		}
	}
}
