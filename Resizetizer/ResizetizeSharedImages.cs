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
	public class ResizetizeSharedImages : Task, ILogger
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
					var resizer = new Resizer(img, IntermediateOutputPath, this);

					foreach (var dpi in dpis)
					{
						var r = resizer.Resize(dpi);
						fileWrites.Add(r);
					}
				}
				else
				{
					// Otherwise just copy the thing over to the 1.0 scale
					var dest = Resizer.CopyFile(img, originalScaleDpi, IntermediateOutputPath, PlatformType.ToLower().Equals("android"));
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
