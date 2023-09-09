using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

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

		public string IsMacEnabled { get; set; }

		public ILogger Logger => this;

		public override bool Execute()
		{
			System.Threading.Tasks.Task.Run(async () =>
			{
				try
				{
					Log.LogMessage(MessageImportance.Low, $"Hello from Resizetizer.NT");
					await DoExecute();
				}
				// suggestion related a better logging by @r2d2rigo:
				// https://github.com/Redth/ResizetizerNT/issues/69#issuecomment-979908021
				// https://github.com/rotorsoft-ltd/ResizetizerNT/commit/029476d44975677af5b002ae89863a04fb597b93
				catch (AggregateException aggregate)
				{
					foreach (var innerException in aggregate.InnerExceptions)
					{
						Log.LogErrorFromException(innerException);
					}

					Log.LogWarning($"Additional log details to the previously logged exception: PlatformType=[{PlatformType}],IntermediateOutputPath=[{IntermediateOutputPath}],InputsFile=[{InputsFile}],IsMacEnabled=[{IsMacEnabled}]");
				}
				catch (Exception ex)
				{
					Log.LogErrorFromException(ex, true, true, GetFileNameToLogError());
					Log.LogWarning($"Additional log details to the previously logged exception: PlatformType=[{PlatformType}],IntermediateOutputPath=[{IntermediateOutputPath}],InputsFile=[{InputsFile}],IsMacEnabled=[{IsMacEnabled}]");
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
			// do not exist after packages were updated
			// Svg.SvgDocument.SkipGdiPlusCapabilityCheck = true;

			var images = ParseImageTaskItems(SharedImages);

			var dpis = DpiPath.GetDpis(PlatformType);

			if (dpis == null || dpis.Length <= 0)
				return System.Threading.Tasks.Task.CompletedTask;

			var originalScaleDpi = DpiPath.GetOriginal(PlatformType);

			var resizedImages = new ConcurrentBag<ResizedImageInfo>();

			System.Threading.Tasks.Parallel.ForEach(images, img =>
			{
				try
				{
					var opStopwatch = new Stopwatch();
					opStopwatch.Start();

					string op;

					if (img.IsAppIcon)
					{
						// App icons are special
						ProcessAppIcon(img, resizedImages);

						op = "App Icon";
					}
					else
					{
						// By default we resize, but let's make sure
						if (img.Resize)
						{
							ProcessImageResize(img, dpis, resizedImages);

							op = "Resize";
						}
						else
						{
							// Otherwise just copy the thing over to the 1.0 scale
							ProcessImageCopy(img, originalScaleDpi, resizedImages);

							op = "Copy";
						}
					}

					opStopwatch.Stop();

					Log.LogMessage(MessageImportance.Low, $"{op} took {opStopwatch.ElapsedMilliseconds}ms");
				}
				// suggestion related a better logging by @r2d2rigo:
				// https://github.com/Redth/ResizetizerNT/issues/69#issuecomment-979908021
				// https://github.com/rotorsoft-ltd/ResizetizerNT/commit/029476d44975677af5b002ae89863a04fb597b93
				catch (AggregateException aggregate)
				{
					foreach (var innerException in aggregate.InnerExceptions)
					{
						Log.LogErrorFromException(innerException);
					}

					Log.LogWarning(
						$"Additional log details to the previously logged exception for SharedImageInfo: Alias=[{img.Alias}] BaseSize=[{img.BaseSize?.ToString()}] Filename=[{img.Filename}] ForegroundFilename=[{img.ForegroundFilename}] ForegroundIsVector=[{img.ForegroundIsVector}] ForegroundScale=[{img.ForegroundScale}] OutputName=[{img.OutputName}] OutputExtension=[{img.OutputExtension}] OutputFormat=[{img.OutputFormat}] Resize=[{img.Resize}] TintColor=[{img.TintColor}]");
				}
				catch (Exception ex)
				{
					Log.LogErrorFromException(ex, true, true, GetFileNameToLogError());
					Log.LogWarning(
						$"Additional log details to the previously logged exception for SharedImageInfo: Alias=[{img.Alias}] BaseSize=[{img.BaseSize?.ToString()}] Filename=[{img.Filename}] ForegroundFilename=[{img.ForegroundFilename}] ForegroundIsVector=[{img.ForegroundIsVector}] ForegroundScale=[{img.ForegroundScale}] OutputName=[{img.OutputName}] OutputExtension=[{img.OutputExtension}] OutputFormat=[{img.OutputFormat}] Resize=[{img.Resize}] TintColor=[{img.TintColor}]");
					throw;
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

		private string GetFileNameToLogError([CallerFilePath] string filePath = "",
			[CallerLineNumber] int lineNumber = 0) => $"{filePath}:{lineNumber}";

		void ProcessAppIcon(SharedImageInfo img, ConcurrentBag<ResizedImageInfo> resizedImages)
		{
			var appIconName = img.OutputName;

			// Generate the actual bitmap app icons themselves
			var appIconDpis = DpiPath.GetAppIconDpis(PlatformType, appIconName);

			Log.LogMessage(MessageImportance.Low, $"App Icon");

			// Apple and Android have special additional files to generate for app icons
			if (PlatformType == "android")
			{
				Log.LogMessage(MessageImportance.Low, $"Android Adaptive Icon Generator");

				appIconName = appIconName.ToLowerInvariant();

				var adaptiveIconGen = new AndroidAdaptiveIconGenerator(img, appIconName, IntermediateOutputPath, this);
				var iconsGenerated = adaptiveIconGen.Generate();

				foreach (var iconGenerated in iconsGenerated)
					resizedImages.Add(iconGenerated);
			}
			else if (PlatformType == "ios")
			{
				Log.LogMessage(MessageImportance.Low, $"iOS Icon Assets Generator");

				var appleAssetGen = new AppleIconAssetsGenerator(img, appIconName, IntermediateOutputPath, appIconDpis, this);

				var assetsGenerated = appleAssetGen.Generate();

				foreach (var assetGenerated in assetsGenerated)
					resizedImages.Add(assetGenerated);
			}

			Log.LogMessage(MessageImportance.Low, $"Generating App Icon Bitmaps for DPIs");

			var appTool = new SkiaSharpAppIconTools(img, this);

			Log.LogMessage(MessageImportance.Low, $"App Icon: Intermediate Path " + IntermediateOutputPath);

			foreach (var dpi in appIconDpis)
			{
				Log.LogMessage(MessageImportance.Low, $"App Icon: " + dpi);

				var destination = Resizer.GetFileDestination(img, dpi, IntermediateOutputPath)
					.Replace("{name}", appIconName);

				Log.LogMessage(MessageImportance.Low, $"App Icon Destination: " + destination);

				appTool.Resize(dpi, Path.ChangeExtension(destination, ".png"));
			}
		}

		void ProcessImageResize(SharedImageInfo img, DpiPath[] dpis, ConcurrentBag<ResizedImageInfo> resizedImages)
		{
			var resizer = new Resizer(img, IntermediateOutputPath, this);

			foreach (var dpi in dpis)
			{
				Log.LogMessage(MessageImportance.Low, $"Resizing {img.Filename}");

				var r = resizer.Resize(dpi, InputsFile);
				resizedImages.Add(r);

				Log.LogMessage(MessageImportance.Low, $"Resized {img.Filename}");
			}
		}

		void ProcessImageCopy(SharedImageInfo img, DpiPath originalScaleDpi, ConcurrentBag<ResizedImageInfo> resizedImages)
		{
			var resizer = new Resizer(img, IntermediateOutputPath, this);

			Log.LogMessage(MessageImportance.Low, $"Copying {img.Filename}");

			var r = resizer.CopyFile(originalScaleDpi, InputsFile, PlatformType.ToLower().Equals("android"));
			resizedImages.Add(r);

			Log.LogMessage(MessageImportance.Low, $"Copied {img.Filename}");
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

				var fileInfo = new FileInfo(image.GetMetadata("FullPath"));

				info.Filename = fileInfo.FullName;

				info.Alias = image.GetMetadata("Link");

				info.OutputFormat = Utils.ParseFormatString(image.GetMetadata("Format"));

				info.BaseSize = Utils.ParseSizeString(image.GetMetadata("BaseSize"));

				if (bool.TryParse(image.GetMetadata("Resize"), out var rz))
					info.Resize = rz;

				info.TintColor = Utils.ParseColorString(image.GetMetadata("TintColor"));

				if (bool.TryParse(image.GetMetadata("IsAppIcon"), out var iai))
					info.IsAppIcon = iai;

				if (float.TryParse(image.GetMetadata("ForegroundScale"), out var fsc))
					info.ForegroundScale = fsc;

				var fgFile = image.GetMetadata("ForegroundFile");
				if (!string.IsNullOrEmpty(fgFile))
				{
					var bgFileInfo = new FileInfo(info.Filename);

					if (!Path.IsPathRooted(fgFile))
						fgFile = Path.Combine(bgFileInfo.Directory.FullName, fgFile);
					else
						fgFile = Path.GetFullPath(fgFile);

					Logger.Log($"AppIcon Foreground: " + fgFile);

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
}
