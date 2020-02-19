using SkiaSharp;
using Svg;
using Svg.Skia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Resizetizer
{
	internal class SkiaSharpSvgTools
	{
		public SkiaSharpSvgTools(SharedImageInfo info, ILogger logger)
		{
			Info = info;
			Logger = logger;

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

		public SharedImageInfo Info { get; private set; }

		public ILogger Logger { get; private set; }

		SKSvg svg;

		public void Resize(DpiPath dpi, string destination)
		{
			int sourceNominalWidth = Info.BaseSize?.Width ?? (int)svg.Picture.CullRect.Width;
			int sourceNominalHeight = Info.BaseSize?.Height ?? (int)svg.Picture.CullRect.Height;
			var resizeRatio = dpi.Scale;

			// Find the actual size of the SVG 
			var sourceActualWidth = svg.Picture.CullRect.Width;
			var sourceActualHeight = svg.Picture.CullRect.Height;

			// Figure out what the ratio to convert the actual image size to the nominal size is
			var nominalRatio = Math.Max((double)sourceNominalWidth / (double)sourceActualWidth, (double)sourceNominalHeight / (double)sourceActualHeight);

			// Multiply nominal ratio by the resize ratio to get our final ratio we actually adjust by
			var adjustRatio = nominalRatio * (double)resizeRatio;

			// Figure out our scaled width and height to make a new canvas for
			//var scaledWidth = sourceActualWidth * adjustRatio;
			//var scaledHeight = sourceActualHeight * adjustRatio;

			var sw = new Stopwatch();
			sw.Start();

			using (var stream = File.OpenWrite(destination))
			{
				svg.Picture.ToImage(stream, SKColors.Empty, SKEncodedImageFormat.Png, 100, (float)adjustRatio, (float)adjustRatio, SKColorType.Argb4444, SKAlphaType.Premul);
			}

			sw.Stop();
			Logger?.Log($"Save Image took {sw.ElapsedMilliseconds}ms");

		}
	}
}
