using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;

namespace Resizetizer
{
	internal class SharedImageInfo
	{
		public string Alias { get; set; }

		public string Filename { get; set; }

		public string OutputName =>
			string.IsNullOrWhiteSpace(Alias)
				? Path.GetFileNameWithoutExtension(Filename)
				: Path.GetFileNameWithoutExtension(Alias);

		public string OutputExtension =>
			string.IsNullOrWhiteSpace(Alias) || !Path.HasExtension(Alias)
				? Path.GetExtension(Filename)
				: Path.GetExtension(Alias);

		public Size? BaseSize { get; set; }

		public bool Resize { get; set; } = true;

		public Color? TintColor { get; set; }

		public bool IsVector => IsVectorFilename(Filename);

		public bool IsAppIcon { get; set; }

		public string ForegroundFilename { get; set; }

		public bool ForegroundIsVector => IsVectorFilename(ForegroundFilename);

		public double ForegroundScale { get; set; } = 1.0;

		private static bool IsVectorFilename(string filename)
			=> Path.GetExtension(filename)?.Equals(".svg", StringComparison.OrdinalIgnoreCase) ?? false;

		static readonly Regex rxFilename
			= new Regex(@"^[a-z]+[a-z0-9_]{0,}[^_]$", RegexOptions.Singleline);

		public bool IsValidOutputName
			=> rxFilename.IsMatch(OutputName);
	}
}
