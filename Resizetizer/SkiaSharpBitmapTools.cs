using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Resizetizer
{
	internal class SkiaSharpBitmapTools : IDisposable
	{
		public SkiaSharpBitmapTools(SharedImageInfo info, ILogger logger)
		{
			Info = info;
			Logger = logger;
			bmp = SKBitmap.Decode(info.Filename);
		}

		public SharedImageInfo Info { get; private set; }
		public ILogger Logger { get; private set; }

		SKBitmap bmp;

		public void Resize(DpiPath dpi, string destination)
		{
			int sourceNominalWidth = Info.BaseSize?.Width ?? bmp.Width;
			int sourceNominalHeight = Info.BaseSize?.Height ?? bmp.Height;
			var resizeRatio = dpi.Scale;

			var sourceActualWidth = bmp.Width;
			var sourceActualHeight = bmp.Height;

			var nominalRatio = Math.Max((double)sourceNominalWidth / (double)sourceActualWidth, (double)sourceNominalHeight / (double)sourceActualHeight);

			var adjustRatio = nominalRatio * Convert.ToDouble(resizeRatio);

			var newWidth = (int)Math.Floor(bmp.Width * adjustRatio);
			var newHeight = (int)Math.Floor(bmp.Height * adjustRatio);

			using (var rzBitmap = bmp.Resize(new SKImageInfo(newWidth, newHeight), SKFilterQuality.High))
			using (var img = SKImage.FromBitmap(rzBitmap))
			using (var data = img.Encode(SKEncodedImageFormat.Png, 100))
			using (var fs = File.Open(destination, FileMode.Create))
			{
				data.SaveTo(fs);
			}
		}

		public void Dispose()
		{
			bmp?.Dispose();
		}
	}
}
