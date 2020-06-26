using System;
using System.Collections.Generic;

namespace Resizetizer
{
	internal class AppleIconAssetsGenerator
	{
		public AppleIconAssetsGenerator(SharedImageInfo info, string intermediateOutputPath, ILogger logger)
		{
			Info = info;
			Logger = logger;
			IntermediateOutputPath = intermediateOutputPath;
		}

		public SharedImageInfo Info { get; private set; }
		public string IntermediateOutputPath { get; private set; }
		public ILogger Logger { get; private set; }

		public IEnumerable<ResizedImageInfo> Generate()
		{


			return new List<ResizedImageInfo>();
		}
	}
}
