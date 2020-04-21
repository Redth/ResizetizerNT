using SkiaSharp;
using Svg;
using Svg.Skia;
using System;
using System.Diagnostics;

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

			var svgDoc = SKSvg.Open(Info.Filename);

			void ChangeFill(SvgElement element)
			{
				if (element is SvgPath ePath && ePath.Fill is SvgColourServer eFill)
				{
					Logger?.Log($"Found Fill: {eFill.Colour.ToString()}");

					if (!eFill.Colour.IsEmpty)
					{
						ePath.Fill = new SvgColourServer(Info.TintColor.Value);

						Logger?.Log($"Changing Fill: {Info.TintColor.ToString()}");
					}
				}

				if (element.Children.Count > 0)
				{
					foreach (var item in element.Children)
						ChangeFill(item);
				}
			}

			if (Info.TintColor.HasValue)
			{
				Logger?.Log($"Changing Tint: {Info.TintColor.Value.ToString()}");

				foreach (var elem in svgDoc.Children)
					ChangeFill(elem);
			}

			Svg.SvgDocument.SkipGdiPlusCapabilityCheck = true;


			//svgDoc.Write(Info.Filename + ".mod.svg");
			sw.Stop();
			Logger?.Log($"Open SVG took {sw.ElapsedMilliseconds}ms");
			sw.Reset();
			sw.Start();


			svg = new SKSvg();
			sw.Stop();
			Logger?.Log($"new SKSvg() took {sw.ElapsedMilliseconds}ms");
			sw.Reset();
			sw.Start();

			svg.FromSvgDocument(svgDoc);
			sw.Stop();
			Logger?.Log($"svg.FromSvgDocument took {sw.ElapsedMilliseconds}ms");

		}

		protected override SKSize GetOriginalSize() =>
			svg.Picture.CullRect.Size;

		protected override void DrawUnscaled(SKCanvas canvas) =>
			canvas.DrawPicture(svg.Picture);

		public void Dispose()
		{
			svg?.Dispose();
			svg = null;
		}
	}
}
