using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
//using SixLabors.ImageSharp;
//using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Resizetizer
{
	public class ResizetizeSharedImages : AsyncTask, ILogger
	{
		[Required]
		public string PlatformType { get; set; } = "android";

		[Required]
		public string IntermediateOutputPath { get; set; }

		public string InputsFile { get; set; }

		public ITaskItem[] SharedImages { get; set; }

		[Output]
		public ITaskItem[] CopiedResources { get; set; }

		public string IsMacEnabled { get;set; }

		public override bool Execute()
		{
			System.Threading.Tasks.Task.Run(async () =>
			{
				try
				{
					await DoExecute();
				}
				catch (Exception ex)
				{
					Log.LogErrorFromException(ex);
				}
				finally
				{
					Complete();
				}

			});

			return base.Execute();
		}

		System.Threading.Tasks.Task DoExecute()
		{
			Svg.SvgDocument.SkipGdiPlusCapabilityCheck = true;

			var images = ParseImageTaskItems(SharedImages);

			var dpis = DpiPath.GetDpis(PlatformType);

			if (dpis == null || dpis.Length <= 0)
				return System.Threading.Tasks.Task.CompletedTask;

			var originalScaleDpi = DpiPath.GetOriginal(PlatformType);

			var resizedImages = new ConcurrentBag<ResizedImageInfo>();

			System.Threading.Tasks.Parallel.ForEach(images, img =>
			{
				if (img.IsAppIcon)
				{
					// Apple and Android have special additional files to generate for app icons
					if (PlatformType == "android")
					{
						var adaptiveIconGen = new AndroidAdaptiveIconGenerator(img, IntermediateOutputPath, this);
						var iconsGenerated = adaptiveIconGen.Generate();

						resizedImages.AddRange(iconsGenerated);
					}
					else if (PlatformType == "ios")
					{
						var appleAssetGen = new AppleIconAssetsGenerator(img, IntermediateOutputPath, this);

						var assetsGenerated = appleAssetGen.Generate();

						resizedImages.AddRange(assetsGenerated);
					}
					
					// Generate the actual bitmap app icons themselves
					var appIconDpis = DpiPath.GetAppIconDpis(PlatformType);

					var appTool = new SkiaSharpAppIconTools(img, this);

					foreach (var dpi in appIconDpis)
					{
						appTool.Resize(dpi, Resizer.GetFileDestination(img, dpi, IntermediateOutputPath));
					}
				}
				else
				{
					var opStopwatch = new Stopwatch();
					opStopwatch.Start();

					var op = "Resize";

					// By default we resize, but let's make sure
					if (img.Resize)
					{
						var resizer = new Resizer(img, IntermediateOutputPath, this);

						foreach (var dpi in dpis)
						{
							var r = resizer.Resize(dpi, InputsFile);
							resizedImages.Add(r);
						}
					}
					else
					{
						op = "Copy";
						// Otherwise just copy the thing over to the 1.0 scale
						var r = Resizer.CopyFile(img, originalScaleDpi, IntermediateOutputPath, InputsFile, this, PlatformType.ToLower().Equals("android"));
						resizedImages.Add(r);
					}

					opStopwatch.Stop();

					Log.LogMessage(MessageImportance.Low, $"{op} took {opStopwatch.ElapsedMilliseconds}ms");
				}
			});
			
			var copiedResources = new List<TaskItem>();

			foreach (var img in resizedImages)
			{
				var attr = new Dictionary<string, string>();
				string itemSpec = Path.GetFullPath(img.Filename);

				// Fix the item spec to be relative for mac
				if (bool.TryParse(IsMacEnabled, out bool isMac) && isMac)
					itemSpec = img.Filename;

				// Add DPI info to the itemspec so we can use it in the targets
				attr.Add("_ResizetizerDpiPath", img.Dpi.Path);
				attr.Add("_ResizetizerDpiScale", img.Dpi.Scale.ToString());

				copiedResources.Add(new TaskItem(itemSpec, attr));
			}

			CopiedResources = copiedResources.ToArray();

			return System.Threading.Tasks.Task.CompletedTask;
		}

		void ILogger.Log(string message)
		{
			Log?.LogMessage(message);
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

				info.BaseSize = Utils.ParseSizeString(image.GetMetadata("BaseSize"));

				if (bool.TryParse(image.GetMetadata("Resize"), out var rz))
					info.Resize= rz;

				info.TintColor = Utils.ParseColorString(image.GetMetadata("TintColor"));

				if (bool.TryParse(image.GetMetadata("IsAppIcon"), out var iai))
					info.IsAppIcon = iai;

				if (float.TryParse(image.GetMetadata("ForegroundScale"), out var fsc))
					info.ForegroundScale = fsc;

				var fgFile = image.GetMetadata("Foreground");
				if (!string.IsNullOrEmpty(fgFile))
				{
					fgFile = Path.GetFullPath(fgFile);
					if (File.Exists(fgFile))
						info.ForegroundFilename = fgFile;
				}

				// TODO:
				// - Parse out custom DPI's

				r.Add(info);
			}

			return r;
		}
	}

	public interface ILogger
	{
		void Log(string message);
	}
}
