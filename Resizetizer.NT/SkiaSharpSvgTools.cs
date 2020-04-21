using SkiaSharp;
using Svg.Skia;
using System;
using System.Diagnostics;
using System.Drawing;

namespace Resizetizer
{
	internal class SkiaSharpSvgTools : SkiaSharpTools, IDisposable
	{
		SKSvg svg;
		SKPaint paint;

		public SkiaSharpSvgTools(SharedImageInfo info, ILogger logger)
			: base(info, logger)
		{
			if (Info.TintColor is Color tint)
			{
				var color = new SKColor(unchecked((uint)tint.ToArgb()));
				Logger?.Log($"Detected a tint color of {color}");

				paint = new SKPaint
				{
					ColorFilter = SKColorFilter.CreateBlendMode(color, SKBlendMode.SrcIn)
				};
			}
			
			var sw = new Stopwatch();
			sw.Start();

			svg = new SKSvg();
			svg.Load(Info.Filename);

			sw.Stop();
			Logger?.Log($"Open SVG took {sw.ElapsedMilliseconds}ms");
		}

		protected override SKSize GetOriginalSize() =>
			svg.Picture.CullRect.Size;

		protected override void DrawUnscaled(SKCanvas canvas) =>
			canvas.DrawPicture(svg.Picture, paint);

		public void Dispose()
		{
			svg?.Dispose();
			svg = null;
		}
	}
}
