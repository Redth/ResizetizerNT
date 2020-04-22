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

		public SkiaSharpSvgTools(SharedImageInfo info, ILogger logger)
			: base(info, logger)
		{
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
			canvas.DrawPicture(svg.Picture, Paint);

		public void Dispose()
		{
			svg?.Dispose();
			svg = null;
		}
	}
}
