using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
//using SixLabors.ImageSharp;
//using SixLabors.ImageSharp.Processing;
using System;
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

			var resizedImages = new List<string>();

			System.Threading.Tasks.Parallel.ForEach(images, img =>
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
					var dest = Resizer.CopyFile(img, originalScaleDpi, IntermediateOutputPath, InputsFile, this, PlatformType.ToLower().Equals("android"));
					resizedImages.Add(dest);
				}

				opStopwatch.Stop();

				Log.LogMessage(MessageImportance.Low, $"{op} took {opStopwatch.ElapsedMilliseconds}ms");
			});
			
			var copiedResources = new List<TaskItem>();

			foreach (var f in resizedImages)
			{
				var attr = new Dictionary<string, string>();
				string itemSpec = Path.GetFullPath(f);

				if (bool.TryParse(IsMacEnabled, out bool isMac) && isMac)
					itemSpec = f;

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

				var tint = image.GetMetadata("TintColor");

				if (!string.IsNullOrWhiteSpace(tint))
				{
					try
					{
						var hx = "0x" + tint.Trim('#');
						var c = Color.FromArgb(int.Parse(hx));

						info.TintColor = c;
					}
					catch
					{
						try { info.TintColor = Color.FromName(tint); }
						catch { }
					}
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
