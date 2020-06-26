//using SixLabors.ImageSharp;
//using SixLabors.ImageSharp.Processing;

using System.Drawing;

namespace Resizetizer
{
	internal class DpiPath
	{
		public DpiPath(string path, decimal scale, string suffix = null, SizeF? size = null, string[] idioms = null)
		{
			Path = path;
			Scale = scale;
			FileSuffix = suffix;
			Size = size;
			Idioms = idioms;
		}

		public string Path { get; set; }
		public decimal Scale { get; set; }
		public string FileSuffix { get; set; }

		public SizeF? Size { get; set; }

		public bool Optimize { get; set; } = true;

		public string[] Idioms { get; set; }

		
		public static DpiPath[] Android
			=> new []
			{
				new DpiPath("drawable-mdpi", 1.0m),
				new DpiPath("drawable-hdpi", 1.5m),
				new DpiPath("drawable-xhdpi", 2.0m),
				new DpiPath("drawable-xxhdpi", 3.0m),
				new DpiPath("drawable-xxxhdpi", 4.0m),
			};

		public static DpiPath[] AndroidAppIcon
			=> new[]
			{
				new DpiPath("mipmap-mdpi", 1.0m, size: new SizeF(48, 48)),
				new DpiPath("mipmap-hdpi", 1.5m, size: new SizeF(48, 48)),
				new DpiPath("mipmap-xhdpi", 2.0m, size: new SizeF(48, 48)),
				new DpiPath("mipmap-xxhdpi", 3.0m, size: new SizeF(48, 48)),
				new DpiPath("mipmap-xxxhdpi", 4.0m, size: new SizeF(48, 48)),
			};

		static DpiPath AndroidOriginal => new DpiPath("drawable", 1.0m);

		public static DpiPath[] Ios
			=> new []
			{
				new DpiPath("", 1.0m),
				new DpiPath("", 2.0m, "@2x"),
				new DpiPath("", 3.0m, "@3x"),
			};

		public static DpiPath[] IosAppIcon
			=> new[]
			{
				new DpiPath("", 1.0m, "20x20@1x", new SizeF(20, 20), new [] { "iphone", "ipad" }),
				new DpiPath("", 2.0m, "20x20@2x", new SizeF(20, 20), new [] { "iphone", "ipad" }),
				new DpiPath("", 3.0m, "20x20@3x", new SizeF(20, 20), new [] { "iphone" }),

				new DpiPath("", 1.0m, "29x29@1x", new SizeF(29, 29), new [] { "iphone" }),
				new DpiPath("", 2.0m, "29x29@2x", new SizeF(29, 29), new [] { "iphone", "ipad" }),
				new DpiPath("", 3.0m, "29x29@3x", new SizeF(29, 29), new [] { "iphone", "ipad" }),

				new DpiPath("", 1.0m, "40x40@1x", new SizeF(40, 40), new [] { "ipad" }),
				new DpiPath("", 2.0m, "40x40@2x", new SizeF(40, 40), new [] { "iphone", "ipad" }),
				new DpiPath("", 3.0m, "40x40@3x", new SizeF(40, 40), new [] { "iphone" }),

				new DpiPath("", 2.0m, "60x60@2x", new SizeF(60, 60), new [] { "iphone" }),
				new DpiPath("", 3.0m, "60x60@3x", new SizeF(60, 60), new [] { "iphone" }),

				new DpiPath("", 2.0m, "76x76@1x", new SizeF(76, 76), new [] { "ipad" }),
				new DpiPath("", 2.0m, "76x76@2x", new SizeF(76, 76), new [] { "iphone", "ipad" }),

				new DpiPath("", 2.0m, "83.5x83.5@1x", new SizeF(83.5f, 83.5f), new [] { "ipad" }),
				
				new DpiPath("", 2.0m, "ItunesArtwork@2x", new SizeF(512, 512), new [] { "ios-marketing" }),
				
			};

		static DpiPath IosOriginal => new DpiPath("Resources", 1.0m);


		public static DpiPath[] Uwp
			=> new []
			{
				new DpiPath("Assets", 1.0m, ".scale-100"),
				new DpiPath("Assets", 2.0m, ".scale-200"),
				new DpiPath("Assets", 4.0m, ".scale-400"),
			};

		public static DpiPath[] UwpAppIcon
			=> new[]
			{
				new DpiPath("Assets", 1.0m, ".scale-100"),
				new DpiPath("Assets", 2.0m, ".scale-200"),
				new DpiPath("Assets", 4.0m, ".scale-400"),
			};

		public static DpiPath[] Wpf
			=> new[]
			{
				new DpiPath("", 4.0m, ""),
			};

		public static DpiPath[] WpfAppIcon
			=> new[]
			{
				new DpiPath("", 4.0m, ""),
			};

		static DpiPath WpfOriginal => new DpiPath("", 4.0m, "");

		static DpiPath UwpOriginal => new DpiPath("Assets", 1.0m, ".scale-100");

		public static DpiPath GetOriginal(string platform)
		{
			switch (platform.ToLowerInvariant())
			{
				case "ios":
					return DpiPath.IosOriginal;
				case "android":
					return DpiPath.AndroidOriginal;
				case "uwp":
					return DpiPath.UwpOriginal;
				case "wpf":
					return DpiPath.WpfOriginal;
			}

			return null;
		}

		public static DpiPath[] GetDpis(string platform)
		{
			switch (platform.ToLowerInvariant())
			{
				case "ios":
					return DpiPath.Ios;
				case "android":
					return DpiPath.Android;
				case "uwp":
					return DpiPath.Uwp;
				case "wpf":
					return DpiPath.Wpf;
			}

			return null;
		}

		public static DpiPath[] GetAppIconDpis(string platform)
		{
			switch (platform.ToLowerInvariant())
			{
				case "ios":
					return DpiPath.IosAppIcon;
				case "android":
					return DpiPath.AndroidAppIcon;
				case "uwp":
					return DpiPath.UwpAppIcon;
				case "wpf":
					return DpiPath.WpfAppIcon;
			}

			return null;
		}
	}
}
