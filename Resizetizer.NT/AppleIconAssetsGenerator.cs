﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Resizetizer
{
	internal class AppleIconAssetsGenerator
	{
		public AppleIconAssetsGenerator(SharedImageInfo info, string appIconName, string intermediateOutputPath, DpiPath[] dpis, ILogger logger)
		{
			Info = info;
			Logger = logger;
			IntermediateOutputPath = intermediateOutputPath;
			AppIconName = appIconName;
			Dpis = dpis;
		}

		public string AppIconName { get; }

		public DpiPath[] Dpis { get; }

		public SharedImageInfo Info { get; private set; }
		public string IntermediateOutputPath { get; private set; }
		public ILogger Logger { get; private set; }

		public IEnumerable<ResizedImageInfo> Generate()
		{
			var outputAppIconSetDir = Path.Combine(IntermediateOutputPath, DpiPath.IosAppIconPath.Replace("{name}", AppIconName));
			var outputAssetsDir = Path.Combine(outputAppIconSetDir, "..");

			Logger.Log("iOS App Icon Set Directory: " + outputAppIconSetDir);

			Directory.CreateDirectory(outputAppIconSetDir);

			var assetContentsFile = Path.Combine(outputAssetsDir, "Contents.json");
			var appIconSetContentsFile = Path.Combine(outputAppIconSetDir, "Contents.json");

			var infoJsonProp = new JObject(
				new JProperty("info", new JObject(
					new JProperty("version", 1),
					new JProperty("author", "xcode"))));

			var appIconImagesJson = new List<JObject>();

			foreach(var dpi in Dpis)
			{
				foreach (var idiom in dpi.Idioms)
				{
					appIconImagesJson.Add(new JObject(
						new JProperty("idiom", idiom),
						new JProperty("size", $"{dpi.Size.Value.Width}x{dpi.Size.Value.Height}"),
						new JProperty("scale", dpi.Scale.ToString("0") + "x"),
						new JProperty("filename", AppIconName + dpi.FileSuffix + ".png")));
				}
			}

			var appIconContentsJson = new JObject(
				new JProperty("images", appIconImagesJson.ToArray()),
				new JProperty("properties", new JObject()),
				new JProperty("info", new JObject(
					new JProperty("version", 1),
					new JProperty("author", "xcode"))));

			//File.WriteAllText(assetContentsFile, infoJsonProp.ToString());
			File.WriteAllText(appIconSetContentsFile, appIconContentsJson.ToString());

			return new List<ResizedImageInfo> {
				//new ResizedImageInfo { Dpi = new DpiPath("", 1), Filename = assetContentsFile },
				new ResizedImageInfo { Dpi = new DpiPath("", 1), Filename = appIconSetContentsFile }
			};
		}
	}
}
