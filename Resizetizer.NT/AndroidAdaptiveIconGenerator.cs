using System;
using System.Collections.Generic;
using System.IO;

namespace Resizetizer
{
	internal class AndroidAdaptiveIconGenerator
	{
		public AndroidAdaptiveIconGenerator(SharedImageInfo info, string intermediateOutputPath, ILogger logger)
		{
			Info = info;
			Logger = logger;
			IntermediateOutputPath = intermediateOutputPath;
		}

		public SharedImageInfo Info { get; private set; }
		public string IntermediateOutputPath { get; private set; }
		public ILogger Logger { get; private set; }

		const string AdaptiveIconDrawableXml =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<adaptive-icon xmlns:android=""http://schemas.android.com/apk/res/android"">
	<background android:drawable=""@drawable/{name}_background""/>
	<foreground android:drawable=""@drawable/{name}_foreground""/>
</adaptive-icon>";

		const string EmptyVectorDrawable =
@"<vector xmlns:android=""http://schemas.android.com/apk/res/android"" xmlns:aapt=""http://schemas.android.com/aapt""
    android:viewportWidth=""1024""
    android:viewportHeight=""1024""
    android:width=""1024dp""
    android:height=""1024dp"" />
";

		public IEnumerable<ResizedImageInfo> Generate()
		{
			var results = new List<ResizedImageInfo>();

			var name = Path.GetFileNameWithoutExtension(Info.Filename);
			var fullIntermediateOutputPath = new DirectoryInfo(IntermediateOutputPath);

			var backgroundFile = Info.Filename;
			var backgroundIsVector = Info.IsVector;

			var foregroundFile = Info.ForegroundFilename;
			var foregroundIsVector = Info.ForegroundIsVector;
			var foregroundExists = File.Exists(foregroundFile);

			// If we have vectors we can emit an adaptive icon
			if (backgroundIsVector && (foregroundIsVector || !foregroundExists))
			{
				var backgroundDestination = Path.Combine(fullIntermediateOutputPath.FullName, "drawable-v24", name + "_background.xml");
				var fileInfo = new FileInfo(backgroundDestination);
				if (!fileInfo.Directory.Exists)
					fileInfo.Directory.Create();

				Svg2VectorDrawable.Svg2Vector.Convert(backgroundFile, backgroundDestination);

				var foregroundDestination = Path.Combine(fullIntermediateOutputPath.FullName, "drawable", name + "_foreground.xml");
				fileInfo = new FileInfo(foregroundDestination);
				if (!fileInfo.Directory.Exists)
					fileInfo.Directory.Create();

				// Convert to android vector drawable, or use a blank one if it doesn't exist
				if (foregroundExists)
					Svg2VectorDrawable.Svg2Vector.Convert(foregroundFile, foregroundDestination);
				else
					File.WriteAllText(foregroundDestination, EmptyVectorDrawable);

				var adaptiveIconXmlStr = AdaptiveIconDrawableXml.Replace("{name}", name);

				var adaptiveIconDestination = Path.Combine(fullIntermediateOutputPath.FullName, "mipmap-anydpi-v26", name + ".xml");
				var adaptiveIconRoundDestination = Path.Combine(fullIntermediateOutputPath.FullName, "mipmap-anydpi-v26", name + "_round.xml");

				fileInfo = new FileInfo(adaptiveIconDestination);
				if (!fileInfo.Directory.Exists)
					fileInfo.Directory.Create();

				// Write out the adaptive icon xml drawables
				File.WriteAllText(adaptiveIconDestination, adaptiveIconXmlStr);
				File.WriteAllText(adaptiveIconRoundDestination, adaptiveIconXmlStr);

				results.Add(new ResizedImageInfo { Dpi = new DpiPath("drawable-v24", 1, "_background"), Filename = backgroundDestination });
				results.Add(new ResizedImageInfo { Dpi = new DpiPath("drawable", 1, "_foreground"), Filename = foregroundDestination });
				results.Add(new ResizedImageInfo { Dpi = new DpiPath("mipmap-anydpi-v26", 1), Filename = adaptiveIconDestination });
				results.Add(new ResizedImageInfo { Dpi = new DpiPath("mipmap-anydpi-v26", 1, "_round"), Filename = adaptiveIconRoundDestination });
			}

			return results;
		}
	}
}
