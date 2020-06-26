using SkiaSharp;
using System;
using System.Diagnostics;
using System.Drawing;

namespace Resizetizer
{
	internal class SkiaSharpBitmapTools : SkiaSharpTools, IDisposable
	{
		SKBitmap bmp;

		public SkiaSharpBitmapTools(string filename, Size? baseSize, Color? tintColor, ILogger logger)
			: base(filename, baseSize, tintColor, logger)
		{
			var sw = new Stopwatch();
			sw.Start();

			bmp = SKBitmap.Decode(filename);

			sw.Stop();
			Logger?.Log($"Open RASTER took {sw.ElapsedMilliseconds}ms");
		}

		public override SKSize GetOriginalSize() =>
			bmp.Info.Size;

		public override void DrawUnscaled(SKCanvas canvas) =>
			canvas.DrawBitmap(bmp, 0, 0, Paint);

		public void Dispose()
		{
			bmp?.Dispose();
			bmp = null;
		}
	}
}
