//using SixLabors.ImageSharp;
//using SixLabors.ImageSharp.Processing;
using System.Drawing;
using System.IO;

namespace Resizetizer
{
	internal class SharedImageInfo
	{
		public string Filename { get; set; }

		public Size? BaseSize { get; set; }

		public bool Resize { get; set; } = true;

		public Color? TintColor { get; set; }
		
		public string ImageNamePrefix { get; set; }
		
		public string ImageNamePostfix { get; set; }

		public bool IsVector
			=> Path.GetExtension(Filename)?.TrimStart('.')?.ToLowerInvariant()?.Equals("svg") ?? false;
	}
}
