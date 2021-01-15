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

		public bool IsVector
			=> Path.GetExtension(Filename)?.TrimStart('.')?.ToLowerInvariant()?.Equals("svg") ?? false;

		public bool IsAppIcon { get; set; }

		public string ForegroundFilename { get; set; }

		public bool ForegroundIsVector
			=> Path.GetExtension(ForegroundFilename)?.TrimStart('.')?.ToLowerInvariant()?.Equals("svg") ?? false;

		public double ForegroundScale { get; set; } = 1.0;
	}
}
