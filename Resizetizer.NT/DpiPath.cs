//using SixLabors.ImageSharp;
//using SixLabors.ImageSharp.Processing;

namespace Resizetizer
{
	internal class DpiPath
	{
		public DpiPath(string path, decimal scale, string suffix = null)
		{
			Path = path;
			Scale = scale;
			FileSuffix = suffix;
		}

		public string Path { get; set; }
		public decimal Scale { get; set; }
		public string FileSuffix { get; set; }

		public bool Optimize { get; set; } = true;

		
		public static DpiPath[] Android
			=> new []
			{
				new DpiPath("drawable-mdpi", 1.0m),
				new DpiPath("drawable-hdpi", 1.5m),
				new DpiPath("drawable-xhdpi", 2.0m),
				new DpiPath("drawable-xxhdpi", 3.0m),
				new DpiPath("drawable-xxxhdpi", 4.0m),
			};

		static DpiPath AndroidOriginal => new DpiPath("drawable", 1.0m);


		public static DpiPath[] Tizen
			=> new[]
			{
				new DpiPath("MDPI", 1.0m),
				new DpiPath("HDPI", 1.5m),
				new DpiPath("XHDPI", 2.0m),
				new DpiPath("XXHDPI", 3.0m),
				new DpiPath("XXXHDPI", 4.0m),
			};

		static DpiPath TizenOriginal => new DpiPath("MDPI", 1.0m);

		public static DpiPath[] Ios
			=> new []
			{
				new DpiPath("", 1.0m),
				new DpiPath("", 2.0m, "@2x"),
				new DpiPath("", 3.0m, "@3x"),
			};

		static DpiPath IosOriginal => new DpiPath("Resources", 1.0m);


		public static DpiPath[] Uwp
			=> new []
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
				case "tizen":
					return DpiPath.TizenOriginal;
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
				case "tizen":
					return DpiPath.Tizen;
			}

			return null;
		}
	}
}
