using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
//using SixLabors.ImageSharp;
//using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Resizetizer
{
	public class ResizetizeSharedImages : Task
	{
		[Required]
		public string PlatformType { get; set; } = "android";

		[Required]
		public string IntermediateOutputPath { get; set; }

		public ITaskItem[] SharedImages { get; set; }

		[Output]
		public ITaskItem[] CopiedResources { get; set; }

		public override bool Execute()
		{
			var images = ParseImageTaskItems(SharedImages);

			var dpis = DpiPath.GetDpis(PlatformType);

			if (dpis == null || dpis.Length <= 0)
				return false;

			var originalScaleDpi = DpiPath.GetOriginal(PlatformType);

			var fileWrites = new List<string>();

			foreach (var img in images)
			{
				// By default we resize, but let's make sure
				if (img.Resize)
				{
					foreach (var dpi in dpis)
					{
						var r = Resize(img, dpi);
						fileWrites.Add(r);
					}
				}
				else
				{
					// Otherwise just copy the thing over to the 1.0 scale
					var dest = CopyFile(img, originalScaleDpi);
					fileWrites.Add(dest);
				}
			}

			// Need to output filewrites back as resources
            if (PlatformType.Equals("ios", StringComparison.OrdinalIgnoreCase))
				CopiedResources = fileWrites.Select(s => new TaskItem(Path.GetFullPath(s),
                    new Dictionary<string, string> { { "LogicalName", Path.GetFileName(s) } })).ToArray();
			else
				CopiedResources = fileWrites.Select(s => new TaskItem(s)).ToArray();

			return true;
		}

		string CopyFile(SharedImageInfo info, DpiPath dpi)
		{
			var destination = GetFileDestination(info, dpi);

			if (PlatformType.ToLowerInvariant() == "android" && info.IsVector && !info.Resize)
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

		string Resize(SharedImageInfo info, DpiPath dpi)
		{
			Log.LogMessage("Resizing: {0}", info.Filename);

			var destination = GetFileDestination(info, dpi);

			if (info.IsVector)
			{
				//// Load image
				//var svg = Svg.SvgDocument.Open(info.Filename);
				//var sourceActualWidth = svg.Width;
				//var sourceActualHeight = svg.Height;
				//var sourceNominalWidth = info.BaseSize?.Width ?? svg.Width;
				//var sourceNominalHeight = info.BaseSize?.Height ?? svg.Height;

				//var nominalRatio = Math.Max((decimal)sourceNominalWidth.Value / (decimal)sourceActualWidth.Value, (decimal)sourceNominalHeight.Value / (decimal)sourceActualHeight.Value);

				//var adjustRatio = nominalRatio * dpi.Scale;
				//var bitmap = svg.Draw((int)((decimal)svg.Width.Value * adjustRatio), 0);


				//bitmap.Save(destination, System.Drawing.Imaging.ImageFormat.Png);
				destination = Path.ChangeExtension(destination, ".png");

				var skrz = new SkiaSharpSvgTools();
				skrz.Resize(info, dpi, destination);
			}
			else
			{
				//using (var img = SixLabors.ImageSharp.Image.Load(info.Filename))
				//{
				//	var sourceActualWidth = img.Width;
				//	var sourceActualHeight = img.Height;
				//	var sourceNominalWidth = info.BaseSize?.Width ?? img.Width;
				//	var sourceNominalHeight = info.BaseSize?.Height ?? img.Height;

				//	var nominalRatio = Math.Max((decimal)sourceNominalWidth / (decimal)sourceActualWidth, (decimal)sourceNominalHeight / (decimal)sourceActualHeight);

				//	var adjustRatio = nominalRatio * dpi.Scale;

				//	img.Mutate(x => x
				//		 .Resize((int)(sourceActualWidth * adjustRatio), (int)(sourceActualHeight * adjustRatio)));
				//	img.Save(destination); // Automatic encoder selected based on extension.
				//}

				var skrz = new SkiaSharpBitmapTools();
				skrz.Resize(info, dpi, destination, msg => Log.LogMessage(msg));
			}

			return destination;
		}

		string GetFileDestination(SharedImageInfo info, DpiPath dpi)
		{
			var name = Path.GetFileNameWithoutExtension(info.Filename);
			var ext = Path.GetExtension(info.Filename);

			var fullIntermediateOutputPath = new DirectoryInfo(IntermediateOutputPath);

			var destination = Path.Combine(fullIntermediateOutputPath.FullName, dpi.Path, name + dpi.FileSuffix + ext);

			var fileInfo = new FileInfo(destination);
			if (!fileInfo.Directory.Exists)
				fileInfo.Directory.Create();

			return destination;
		}

		List<SharedImageInfo> ParseImageTaskItems(ITaskItem[] images)
		{
			var r = new List<SharedImageInfo>();

			if (images == null)
				return r;

			foreach (var image in images)
			{
				var info = new SharedImageInfo();

				info.Filename = image.GetMetadata("FullPath");

				var size = image.GetMetadata("BaseSize");
				if (!string.IsNullOrWhiteSpace(size))
				{
					var parts = size.Split(new char[] { ',', ';' }, 2);

					if (parts.Length > 0 && int.TryParse(parts[0], out var width))
					{
						if (parts.Length > 1 && int.TryParse(parts[1], out var height))
							info.BaseSize = new Size(width, height);
						else
							info.BaseSize = new Size(width, width);
					}
				}

				if (bool.TryParse(image.GetMetadata("Resize"), out var rz))
					info.Resize= rz;

				info.FillColor = image.GetMetadata("FillColor");

				// TODO:
				// - Parse out custom DPI's
				// - Parse out fill/bg color

				r.Add(info);
			}

			return r;
		}
	}

	class DpiPath
	{
		public DpiPath(string path, decimal scale, string suffix = null)
		{
			Path = path;
			Scale = scale;
			FileSuffix = suffix;
		}

		public string Path { get; set; }
		public decimal Scale { get; set; }
		public string FileSuffix { get; set; }

		public bool Optimize { get; set; } = true;

		
		public static DpiPath[] Android
			=> new []
			{
				new DpiPath("drawable-mdpi", 1.0m),
				new DpiPath("drawable-hdpi", 1.5m),
				new DpiPath("drawable-xhdpi", 2.0m),
				new DpiPath("drawable-xxhdpi", 3.0m),
				new DpiPath("drawable-xxxhdpi", 4.0m),
			};

		static DpiPath AndroidOriginal => new DpiPath("drawable", 1.0m);
		public static DpiPath[] Ios
			=> new []
			{
				new DpiPath("", 1.0m),
				new DpiPath("", 2.0m, "@2x"),
				new DpiPath("", 3.0m, "@3x"),
			};

		static DpiPath IosOriginal => new DpiPath("Resources", 1.0m);

		public static DpiPath GetOriginal(string platform)
		{
			switch (platform.ToLowerInvariant())
			{
				case "ios":
					return DpiPath.IosOriginal;
				case "android":
					return DpiPath.AndroidOriginal;
			}

			return null;
		}

		public static DpiPath[] GetDpis(string platform)
		{
			switch (platform.ToLowerInvariant())
			{
				case "ios":
					return DpiPath.Ios;
				case "android":
					return DpiPath.Android;
			}

			return null;
		}
	}

	internal class SharedImageInfo
	{
		public string Filename { get; set; }

		public Size? BaseSize { get; set; }

		public bool Resize { get; set; } = true;

		public string FillColor { get; set; }

		public bool IsVector
			=> Path.GetExtension(Filename)?.TrimStart('.')?.ToLowerInvariant()?.Equals("svg") ?? false;
	}

	public enum SharedImageType
	{
		Vector,
		Bitmap
	}
}
