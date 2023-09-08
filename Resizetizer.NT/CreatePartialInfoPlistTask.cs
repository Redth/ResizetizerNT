﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Resizetizer
{
	public class CreatePartialInfoPlistTask : AsyncTask
	{
		public ITaskItem[] CustomFonts { get; set; }

		[Required]
		public string IntermediateOutputPath { get; set; }

		public string PlistName { get; set; }

		[Output]
		public ITaskItem[] PlistFiles { get; set; }

		const string plistHeader =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
<dict>";
		const string plistFooter = @"
</dict>
</plist>";

		public override bool Execute()
		{
			System.Threading.Tasks.Task.Run(async () =>
			{
				try
				{
					var plistFilename = Path.Combine(IntermediateOutputPath, PlistName ?? "PartialInfo.plist");

					using (var f = File.CreateText(plistFilename))
					{
						f.WriteLine(plistHeader);

						f.WriteLine("  <key>UIAppFonts</key>");
						f.WriteLine("  <array>");

						foreach (var font in CustomFonts)
						{
							var fontFile = new FileInfo(font.ItemSpec);

							f.WriteLine("    <string>" + fontFile.Name + "</string>");
						}

						f.WriteLine("  </array>");
						f.WriteLine(plistFooter);
					}

					PlistFiles = new[] { new TaskItem(plistFilename) };
				}
				catch (Exception ex)
				{
					Log.LogErrorFromException(ex);
				}
				finally
				{
					Complete();
				}

			});

			return base.Execute();
		}
	}
}