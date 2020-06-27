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
			: this(info.Filename, info.BaseSize, info.TintColor, logger)
		{
		}

		public SkiaSharpSvgTools(string filename, Size? baseSize, Color? tintColor, ILogger logger)
			: base(filename, baseSize, tintColor, logger)
		{
			var sw = new Stopwatch();
			sw.Start();

			svg = new SKSvg();
			svg.Load(filename);

			sw.Stop();
			Logger?.Log($"Open SVG took {sw.ElapsedMilliseconds}ms ({filename})");
		}

		public override SKSize GetOriginalSize() =>
			svg.Picture.CullRect.Size;

		public override void DrawUnscaled(SKCanvas canvas) =>
			canvas.DrawPicture(svg.Picture, Paint);

		public void Dispose()
		{
			svg?.Dispose();
			svg = null;
		}
	}
}
