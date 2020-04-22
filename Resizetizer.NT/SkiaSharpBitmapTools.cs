using SkiaSharp;
using System;
using System.Diagnostics;

namespace Resizetizer
{
	internal class SkiaSharpBitmapTools : SkiaSharpTools, IDisposable
	{
		SKBitmap bmp;

		public SkiaSharpBitmapTools(SharedImageInfo info, ILogger logger)
			: base(info, logger)
		{
			var sw = new Stopwatch();
			sw.Start();

			bmp = SKBitmap.Decode(info.Filename);

			sw.Stop();
			Logger?.Log($"Open RASTER took {sw.ElapsedMilliseconds}ms");
		}

		protected override SKSize GetOriginalSize() =>
			bmp.Info.Size;

		protected override void DrawUnscaled(SKCanvas canvas) =>
			canvas.DrawBitmap(bmp, 0, 0, Paint);

		public void Dispose()
		{
			bmp?.Dispose();
			bmp = null;
		}
	}
}
