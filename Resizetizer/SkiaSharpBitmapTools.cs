using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Resizetizer
{
	internal class SkiaSharpBitmapTools
	{
		public void Resize(SharedImageInfo image, DpiPath dpi, string destination, Action<string> logMessage)
		{
			using (var bmp = SKBitmap.Decode(image.Filename))
			{

				logMessage?.Invoke($"BMP: {image.Filename}, W:{bmp.Width}");

				int sourceNominalWidth = image.BaseSize?.Width ?? bmp.Width;
				int sourceNominalHeight = image.BaseSize?.Height ?? bmp.Height;
				var resizeRatio = dpi.Scale;

				var sourceActualWidth = bmp.Width;
				var sourceActualHeight = bmp.Height;

				var nominalRatio = Math.Max((double)sourceNominalWidth / (double)sourceActualWidth, (double)sourceNominalHeight / (double)sourceActualHeight);

				var adjustRatio = nominalRatio * Convert.ToDouble(resizeRatio);

				logMessage?.Invoke($"Ratio: {adjustRatio}");

				var newWidth = (int)Math.Floor(bmp.Width * adjustRatio);
				var newHeight = (int)Math.Floor(bmp.Height * adjustRatio);

				logMessage?.Invoke($"NewSize: {newWidth} * {newHeight}");

				using (var rzBitmap = bmp.Resize(new SKImageInfo(newWidth, newHeight), SKBitmapResizeMethod.Lanczos3))
				using (var img = SKImage.FromBitmap(rzBitmap))
				using (var data = img.Encode(SKEncodedImageFormat.Png, 100))
				using (var fs = File.Open(destination, FileMode.Create))
				{
					data.SaveTo(fs);
				}
			}
		}
	}
}
