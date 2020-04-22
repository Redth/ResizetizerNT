using SkiaSharp;
using System;
using System.Diagnostics;
using System.Drawing;

namespace Resizetizer
{
	internal abstract class SkiaSharpTools
	{
		public SkiaSharpTools(SharedImageInfo info, ILogger logger)
		{
			Info = info;
			Logger = logger;

			if (Info.TintColor is Color tint)
			{
				var color = new SKColor(unchecked((uint)tint.ToArgb()));
				Logger?.Log($"Detected a tint color of {color}");

				Paint = new SKPaint
				{
					ColorFilter = SKColorFilter.CreateBlendMode(color, SKBlendMode.SrcIn)
				};
			}
		}

		public SharedImageInfo Info { get; }

		public ILogger Logger { get; }

		public SKPaint Paint { get; }

		public void Resize(DpiPath dpi, string destination)
		{
			var originalSize = GetOriginalSize();
			var (scaledSize, scale) = GetScaledSize(originalSize, dpi.Scale);

			var sw = new Stopwatch();
			sw.Start();

			// Allocate
			using (var tempBitmap = new SKBitmap(scaledSize.Width, scaledSize.Height))
			{
				// Draw (copy)
				using (var canvas = new SKCanvas(tempBitmap))
				{
					canvas.Clear(SKColors.Transparent);
					canvas.Save();
					canvas.Scale(scale, scale);
					DrawUnscaled(canvas);
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
		}

		protected abstract SKSize GetOriginalSize();

		protected abstract void DrawUnscaled(SKCanvas canvas);

		protected (SKSizeI, float) GetScaledSize(SKSize originalSize, decimal resizeRatio)
		{
			int sourceNominalWidth = Info.BaseSize?.Width ?? (int)originalSize.Width;
			int sourceNominalHeight = Info.BaseSize?.Height ?? (int)originalSize.Height;

			// Find the actual size of the image
			var sourceActualWidth = originalSize.Width;
			var sourceActualHeight = originalSize.Height;

			// Figure out what the ratio to convert the actual image size to the nominal size is
			var nominalRatio = Math.Max(sourceNominalWidth / sourceActualWidth, sourceNominalHeight / sourceActualHeight);

			// Multiply nominal ratio by the resize ratio to get our final ratio we actually adjust by
			var adjustRatio = nominalRatio * (float)resizeRatio;

			// Figure out our scaled width and height to make a new canvas for
			var scaledWidth = sourceActualWidth * adjustRatio;
			var scaledHeight = sourceActualHeight * adjustRatio;

			return (new SKSizeI((int)scaledWidth, (int)scaledHeight), adjustRatio);
		}
	}
}
