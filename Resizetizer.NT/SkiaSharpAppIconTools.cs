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
					canvas.Restore();

					if (hasForeground)
					{
						var (fgScaledSize, fgScale) = foregroundTools.GetScaledSize(foregroundOriginalSize, dpi);

						var userFgScale = 1.0f; // (float)Info.ForegroundScale;

						Logger.Log("dpi.Size: " + dpi.Size);
						Logger.Log("dpi.Scale: " + dpi.Scale);
						Logger.Log("bgScaledSize: " + bgScaledSize);
						Logger.Log("bgScale: " + bgScale);
						Logger.Log("foregroundOriginalSize: " + foregroundOriginalSize);
						Logger.Log("fgScaledSize: " + fgScaledSize);
						Logger.Log("fgScale: " + fgScale);
						Logger.Log("userFgScale: " + userFgScale);

						var fgScaledSizeCenterX = fgScaledSize.Width / 2;
						var fgScaledSizeCenterY = fgScaledSize.Height / 2;

						var finalScale = userFgScale * fgScale;

						Logger.Log("fgScaledSizeCenterX: " + fgScaledSizeCenterX);
						Logger.Log("finalScale: " + finalScale);

						canvas.Scale(finalScale); //, finalScale, fgScaledSizeCenterX, fgScaledSizeCenterY);

						foregroundTools.DrawUnscaled(canvas);
					}
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
