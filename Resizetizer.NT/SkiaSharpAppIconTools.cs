using System;
using System.Diagnostics;
using System.IO;
using SkiaSharp;

namespace Resizetizer
{
	internal class SkiaSharpAppIconTools
	{
		public SkiaSharpAppIconTools(SharedImageInfo info, ILogger logger)
		{
			Info = info;
			Logger = logger;

			hasForeground = File.Exists(info.ForegroundFilename);

			if (hasForeground)
				foregroundTools = SkiaSharpTools.Create(info.ForegroundIsVector, info.ForegroundFilename, null, info.TintColor, logger);

			backgroundTools = SkiaSharpTools.Create(info.IsVector, info.Filename, null, null, logger);

			backgroundOriginalSize = backgroundTools.GetOriginalSize();

			if (hasForeground)
				foregroundOriginalSize = foregroundTools.GetOriginalSize();
		}

		bool hasForeground = false;

		SkiaSharpTools backgroundTools;
		SkiaSharpTools foregroundTools;

		SKSize foregroundOriginalSize;
		SKSize backgroundOriginalSize;

		public SharedImageInfo Info { get; }
		public ILogger Logger { get; }

		public ResizedImageInfo Resize(DpiPath dpi, string destination)
		{
			var sw = new Stopwatch();
			sw.Start();

			var (bgScaledSize, bgScale) = backgroundTools.GetScaledSize(backgroundOriginalSize, dpi);

			// Allocate
			using (var tempBitmap = new SKBitmap(bgScaledSize.Width, bgScaledSize.Height))
			{
				// Draw (copy)
				using (var canvas = new SKCanvas(tempBitmap))
				{
					canvas.Clear(SKColors.Transparent);
					canvas.Save();
					canvas.Scale(bgScale, bgScale);

					backgroundTools.DrawUnscaled(canvas);

					var (fgScaledSize, fgScale) = foregroundTools.GetScaledSize(foregroundOriginalSize, dpi);

					// Multiply by user input scale
					fgScale *= (float)Info.ForegroundScale;

					// Foreground 
					canvas.Scale(fgScale, fgScale, bgScaledSize.Width / 2, bgScaledSize.Height / 2);

					foregroundTools.DrawUnscaled(canvas);
				}

				// Save (encode)
				using (var pixmap = tempBitmap.PeekPixels())
				using (var wrapper = new SKFileWStream(destination))
				{
					pixmap.Encode(wrapper, SKPngEncoderOptions.Default);
				}
			}

			sw.Stop();
			Logger?.Log($"Save Image took {sw.ElapsedMilliseconds}ms");

			return new ResizedImageInfo { Dpi = dpi, Filename = destination };
		}
	}
}
