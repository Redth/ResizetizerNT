using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Resizetizer.NT.Tests")]

namespace Resizetizer
{
	internal class Utils
	{
		static readonly Regex rxResourceFilenameValidation
			= new Regex(@"^[a-z]+[a-z0-9_]{0,}[^_]$", RegexOptions.Singleline);

		public static bool IsValidResourceFilename(string filename)
			=> rxResourceFilenameValidation.IsMatch(Path.GetFileNameWithoutExtension(filename));

		public static Color? ParseColorString(string tint)
		{
			if (string.IsNullOrEmpty(tint))
				return null;

			if (int.TryParse(tint.Trim('#'), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value))
				return Color.FromArgb(value);

			try
			{
				return Color.FromName(tint);
			}
			catch
			{
			}

			return null;
		}

		public static Size? ParseSizeString(string size)
		{
			if (string.IsNullOrEmpty(size))
				return null;

			var parts = size.Split(new char[] { ',', ';' }, 2);

			if (parts.Length > 0 && int.TryParse(parts[0], out var width))
			{
				if (parts.Length > 1 && int.TryParse(parts[1], out var height))
					return new Size(width, height);
				else
					return new Size(width, width);
			}

			return null;
		}


		public static ImageFormat ParseFormatString(string input)
		{
			ImageFormat Parse(string format)
			{
				if (string.IsNullOrEmpty(format))
					return new ImageFormat();

				var parts = format.Split(new char[] { ',', ';' }, 2);


				if (parts.Length > 0 && Enum.TryParse(parts[0], out ImageFormat.Formats imgFormat))
				{
					if (parts.Length > 1 && int.TryParse(parts[1], out var compression))
						return new ImageFormat()
						{
							Format = imgFormat,
							Quality = compression
						};
					else
						return new ImageFormat()
						{
							Format = imgFormat
						};
				}

				return new ImageFormat();
			}

			var res = Parse(input);

			switch (res.Format)
			{
				case ImageFormat.Formats.Default:
					break;
				case ImageFormat.Formats.Png:
					if (res.Quality == -1)
						res.Quality = 100;
					break;
				case ImageFormat.Formats.Jpeg:
					if (res.Quality == -1)
						res.Quality = 80;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return res;
		}
	}
}
