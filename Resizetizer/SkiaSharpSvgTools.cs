using SkiaSharp;
using SkiaSharp.Extended.Svg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using SKSvg = SkiaSharp.Extended.Svg.SKSvg;

namespace Resizetizer
{
	internal class SkiaSharpSvgTools
	{
		static readonly string[] rxFillPatterns = new[] {
			@"fill\s?=\s?""(?<fill>.*?)""",
			@"style\s?=\s?""fill:(?<fill>.*?)""",
		};

		public void Resize(MobileImageInfo image, DpiPath dpi, string destination)
		{
			var fillColor = image.FillColor;

			var svg = new SKSvg();

			// For SVG's we can optionally change the fill color on all paths
			if (!string.IsNullOrEmpty(fillColor))
			{
				var svgText = File.ReadAllText(image.Filename);

				foreach (var rxPattern in rxFillPatterns)
				{
					var matches = Regex.Matches(svgText, rxPattern);

					foreach (Match match in matches)
					{
						var fillGroup = match.Groups?["fill"];

						if (fillGroup != null)
						{
							// Replace the matched rx group with our override fill color
							var a = svgText.Substring(0, fillGroup.Index);
							var b = svgText.Substring(fillGroup.Index + fillGroup.Length);
							svgText = a + fillColor.TrimEnd(';') + ";" + b;
						}
					}
				}

				using (var ms = new MemoryStream())
				using (var sw = new StreamWriter(ms))
				{
					sw.Write(svgText);
					sw.Flush();

					svg.Load(ms);
				}
			}
			else
			{
				svg.Load(image.Filename);
			}

			int sourceNominalWidth = image.OriginalSize?.Width ?? (int)svg.Picture.CullRect.Width;
			int sourceNominalHeight = image.OriginalSize?.Height ?? (int)svg.Picture.CullRect.Height;
			var resizeRatio = dpi.Scale;

			// Find the actual size of the SVG 
			var sourceActualWidth = svg.Picture.CullRect.Width;
			var sourceActualHeight = svg.Picture.CullRect.Height;

			// Figure out what the ratio to convert the actual image size to the nominal size is
			var nominalRatio = Math.Max((double)sourceNominalWidth / (double)sourceActualWidth, (double)sourceNominalHeight / (double)sourceActualHeight);

			// Multiply nominal ratio by the resize ratio to get our final ratio we actually adjust by
			var adjustRatio = nominalRatio * (double)resizeRatio;

			// Figure out our scaled width and height to make a new canvas for
			var scaledWidth = sourceActualWidth * adjustRatio;
			var scaledHeight = sourceActualHeight * adjustRatio;

			// Make a canvas of the target size to draw the svg onto
			var bmp = new SKBitmap((int)Math.Floor(scaledWidth), (int)Math.Floor(scaledHeight));
			var canvas = new SKCanvas(bmp);

			// Make a matrix to scale the SVG
			var matrix = SKMatrix.MakeScale((float)adjustRatio, (float)adjustRatio);

			canvas.Clear(SKColors.Transparent);

			// Draw the svg onto the canvas with our scaled matrix
			canvas.DrawPicture(svg.Picture, ref matrix);

			// Save the op
			canvas.Save();

			// Export the canvas
			var img = SKImage.FromBitmap(bmp);
			var data = img.Encode(SKEncodedImageFormat.Png, 100);
			using (var fs = File.Open(destination, FileMode.Create))
			{
				data.SaveTo(fs);
			}
		}
	}
}
