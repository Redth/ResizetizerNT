using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Resizetizer.NT.Tests
{
	public class ResizetizeSharedImagesTests
	{
		public abstract class ExecuteForApp : IDisposable, IBuildEngine
		{
			protected readonly string DestinationDirectory;
			protected readonly TestLogger Logger;

			protected List<BuildErrorEventArgs> LogErrorEvents = new List<BuildErrorEventArgs>();
			protected List<BuildMessageEventArgs> LogMessageEvents = new List<BuildMessageEventArgs>();
			protected List<CustomBuildEventArgs> LogCustomEvents = new List<CustomBuildEventArgs>();
			protected List<BuildWarningEventArgs> LogWarningEvents = new List<BuildWarningEventArgs>();

			public ExecuteForApp(string type)
			{
				DestinationDirectory = Path.Combine(Path.GetTempPath(), "ResizetizeSharedImagesTests", type, Path.GetRandomFileName());
			}

			protected ResizetizeSharedImages GetNewTask(string type, params ITaskItem[] items) =>
				new ResizetizeSharedImages
				{
					PlatformType = type,
					IntermediateOutputPath = DestinationDirectory,
					InputsFile = "resizetizer.inputs",
					SharedImages = items,
					BuildEngine = this,
				};

			protected ITaskItem GetCopiedResource(ResizetizeSharedImages task, string path) =>
				task.CopiedResources.Single(c => c.ItemSpec.Replace("\\", "/").EndsWith(path));

			protected void AssertFileSize(string file, int width, int height)
			{
				file = Path.Combine(DestinationDirectory, file);

				Assert.True(File.Exists(file), $"File did not exist: {file}");

				using var codec = SKCodec.Create(file);
				Assert.Equal(width, codec.Info.Width);
				Assert.Equal(height, codec.Info.Height);
			}

			protected void AssertFileExists(string file)
			{
				file = Path.Combine(DestinationDirectory, file);

				Assert.True(File.Exists(file), $"File did not exist: {file}");
			}

			protected void AssertFileContains(string file, params string[] snippet)
			{
				file = Path.Combine(DestinationDirectory, file);

				var content = File.ReadAllText(file);

				foreach (var snip in snippet)
					Assert.Contains(snip, content);
			}

			void IDisposable.Dispose()
			{
				if (Directory.Exists(DestinationDirectory))
					Directory.Delete(DestinationDirectory, true);
			}

			// IBuildEngine

			bool IBuildEngine.ContinueOnError => false;

			int IBuildEngine.LineNumberOfTaskNode => 0;

			int IBuildEngine.ColumnNumberOfTaskNode => 0;

			string IBuildEngine.ProjectFileOfTaskNode => "FakeProject.proj";

			bool IBuildEngine.BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs) => throw new NotImplementedException();

			void IBuildEngine.LogCustomEvent(CustomBuildEventArgs e) => LogCustomEvents.Add(e);

			void IBuildEngine.LogErrorEvent(BuildErrorEventArgs e) => LogErrorEvents.Add(e);

			void IBuildEngine.LogMessageEvent(BuildMessageEventArgs e) => LogMessageEvents.Add(e);

			void IBuildEngine.LogWarningEvent(BuildWarningEventArgs e) => LogWarningEvents.Add(e);
		}

		public class ExecuteForAndroid : ExecuteForApp
		{
			public ExecuteForAndroid()
				: base("Android")
			{
			}

			ResizetizeSharedImages GetNewTask(params ITaskItem[] items) =>
				GetNewTask("android", items);

			[Fact]
			public void NoItemsSucceed()
			{
				var task = GetNewTask();

				var success = task.Execute();

				Assert.True(success);
			}

			[Fact]
			public void NonExistantFileFails()
			{
				var items = new[]
				{
					new TaskItem("non-existant.png"),
				};

				var task = GetNewTask(items);

				var success = task.Execute();

				Assert.False(success);
			}

			[Fact]
			public void ValidFileSucceeds()
			{
				var items = new[]
				{
					new TaskItem("images/camera.png"),
				};

				var task = GetNewTask(items);

				var success = task.Execute();

				Assert.True(success);
			}

			[Fact]
			public void SingleImageWithOnlyPathSucceeds()
			{
				var items = new[]
				{
					new TaskItem("images/camera.png"),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success);

				AssertFileSize("drawable-mdpi/camera.png", 1792, 1792);  // 1x
				AssertFileSize("drawable-xhdpi/camera.png", 3584, 3584); // 2x
			}

			[Fact]
			public void TwoImagesWithOnlyPathSucceed()
			{
				var items = new[]
				{
					new TaskItem("images/camera.png"),
					new TaskItem("images/camera_color.png"),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success);

				AssertFileSize("drawable-mdpi/camera.png", 1792, 1792);
				AssertFileSize("drawable-mdpi/camera_color.png", 256, 256);

				AssertFileSize("drawable-xhdpi/camera.png", 3584, 3584);
				AssertFileSize("drawable-xhdpi/camera_color.png", 512, 512);
			}

			[Fact]
			public void ImageWithOnlyPathHasMetadata()
			{
				var items = new[]
				{
					new TaskItem("images/camera.png"),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success);

				var copied = task.CopiedResources;
				Assert.Equal(items.Length * DpiPath.Android.Length, copied.Length);

				var mdpi = GetCopiedResource(task, "drawable-mdpi/camera.png");
				Assert.Equal("drawable-mdpi", mdpi.GetMetadata("_ResizetizerDpiPath"));
				Assert.Equal("1.0", mdpi.GetMetadata("_ResizetizerDpiScale"));

				var xhdpi = GetCopiedResource(task, "drawable-xhdpi/camera.png");
				Assert.Equal("drawable-xhdpi", xhdpi.GetMetadata("_ResizetizerDpiPath"));
				Assert.Equal("2.0", xhdpi.GetMetadata("_ResizetizerDpiScale"));
			}

			[Fact]
			public void TwoImagesWithOnlyPathHasMetadata()
			{
				var items = new[]
				{
					new TaskItem("images/camera.png"),
					new TaskItem("images/camera_color.png"),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success);

				var copied = task.CopiedResources;
				Assert.Equal(items.Length * DpiPath.Android.Length, copied.Length);

				var mdpi = GetCopiedResource(task, "drawable-mdpi/camera.png");
				Assert.Equal("drawable-mdpi", mdpi.GetMetadata("_ResizetizerDpiPath"));
				Assert.Equal("1.0", mdpi.GetMetadata("_ResizetizerDpiScale"));

				var xhdpi = GetCopiedResource(task, "drawable-xhdpi/camera.png");
				Assert.Equal("drawable-xhdpi", xhdpi.GetMetadata("_ResizetizerDpiPath"));
				Assert.Equal("2.0", xhdpi.GetMetadata("_ResizetizerDpiScale"));

				mdpi = GetCopiedResource(task, "drawable-mdpi/camera_color.png");
				Assert.Equal("drawable-mdpi", mdpi.GetMetadata("_ResizetizerDpiPath"));
				Assert.Equal("1.0", mdpi.GetMetadata("_ResizetizerDpiScale"));

				xhdpi = GetCopiedResource(task, "drawable-xhdpi/camera_color.png");
				Assert.Equal("drawable-xhdpi", xhdpi.GetMetadata("_ResizetizerDpiPath"));
				Assert.Equal("2.0", xhdpi.GetMetadata("_ResizetizerDpiScale"));
			}

			[Fact]
			public void SingleImageWithBaseSizeSucceeds()
			{
				var items = new[]
				{
					new TaskItem("images/camera.png", new Dictionary<string, string>
					{
						["BaseSize"] = "44"
					}),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success);

				AssertFileSize("drawable-mdpi/camera.png", 44, 44);
				AssertFileSize("drawable-xhdpi/camera.png", 88, 88);
			}

			[Theory]
			[InlineData("camera")]
			[InlineData("camera_color")]
			public void SingleRasterAppIconWithOnlyPathSucceedsWithoutVectors(string name)
			{
				var items = new[]
				{
					new TaskItem($"images/{name}.png", new Dictionary<string, string>
					{
						["IsAppIcon"] = bool.TrueString
					}),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success);

				AssertFileSize($"mipmap-mdpi/{name}.png", 48, 48);
				AssertFileSize($"mipmap-xhdpi/{name}.png", 96, 96);

				var vectors = Directory.GetFiles(DestinationDirectory, "*.xml", SearchOption.AllDirectories);
				Assert.Empty(vectors);
			}

			[Theory]
			[InlineData("appicon")]
			[InlineData("camera")]
			public void SingleVectorAppIconWithOnlyPathSucceedsWithVectors(string name)
			{
				var items = new[]
				{
					new TaskItem($"images/{name}.svg", new Dictionary<string, string>
					{
						["IsAppIcon"] = bool.TrueString
					}),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success);

				AssertFileSize($"mipmap-mdpi/{name}.png", 48, 48);
				AssertFileSize($"mipmap-xhdpi/{name}.png", 96, 96);

				AssertFileExists($"drawable/{name}_foreground.xml");
				AssertFileExists($"drawable-v24/{name}_background.xml");

				AssertFileExists($"mipmap-anydpi-v26/{name}.xml");
				AssertFileExists($"mipmap-anydpi-v26/{name}_round.xml");

				AssertFileContains($"mipmap-anydpi-v26/{name}.xml",
					$"<foreground android:drawable=\"@drawable/{name}_foreground\"/>",
					$"<background android:drawable=\"@drawable/{name}_background\"/>");

				AssertFileContains($"mipmap-anydpi-v26/{name}_round.xml",
					$"<foreground android:drawable=\"@drawable/{name}_foreground\"/>",
					$"<background android:drawable=\"@drawable/{name}_background\"/>");
			}
		}

		public class ExecuteForiOS : ExecuteForApp
		{
			public ExecuteForiOS()
				: base("iOS")
			{
			}

			ResizetizeSharedImages GetNewTask(params ITaskItem[] items) =>
				GetNewTask("ios", items);

			[Fact]
			public void NoItemsSucceed()
			{
				var task = GetNewTask();

				var success = task.Execute();

				Assert.True(success);
			}

			[Fact]
			public void NonExistantFileFails()
			{
				var items = new[]
				{
					new TaskItem("non-existant.png"),
				};

				var task = GetNewTask(items);

				var success = task.Execute();

				Assert.False(success);
			}

			[Fact]
			public void ValidFileSucceeds()
			{
				var items = new[]
				{
					new TaskItem("images/camera.png"),
				};

				var task = GetNewTask(items);

				var success = task.Execute();

				Assert.True(success);
			}

			[Fact]
			public void SingleImageWithOnlyPathSucceeds()
			{
				var items = new[]
				{
					new TaskItem("images/camera.png"),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success);

				AssertFileSize("camera.png", 1792, 1792);
				AssertFileSize("camera@2x.png", 3584, 3584);
			}

			[Fact]
			public void TwoImagesWithOnlyPathSucceed()
			{
				var items = new[]
				{
					new TaskItem("images/camera.png"),
					new TaskItem("images/camera_color.png"),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success);

				AssertFileSize("camera.png", 1792, 1792);
				AssertFileSize("camera_color.png", 256, 256);

				AssertFileSize("camera@2x.png", 3584, 3584);
				AssertFileSize("camera_color@2x.png", 512, 512);
			}

			[Fact]
			public void ImageWithOnlyPathHasMetadata()
			{
				var items = new[]
				{
					new TaskItem("images/camera.png"),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success);

				var copied = task.CopiedResources;
				Assert.Equal(items.Length * DpiPath.Ios.Length, copied.Length);

				var mdpi = GetCopiedResource(task, "camera.png");
				Assert.Equal("", mdpi.GetMetadata("_ResizetizerDpiPath"));
				Assert.Equal("1.0", mdpi.GetMetadata("_ResizetizerDpiScale"));

				var xhdpi = GetCopiedResource(task, "camera@2x.png");
				Assert.Equal("", xhdpi.GetMetadata("_ResizetizerDpiPath"));
				Assert.Equal("2.0", xhdpi.GetMetadata("_ResizetizerDpiScale"));
			}

			[Fact]
			public void TwoImagesWithOnlyPathHasMetadata()
			{
				var items = new[]
				{
					new TaskItem("images/camera.png"),
					new TaskItem("images/camera_color.png"),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success);

				var copied = task.CopiedResources;
				Assert.Equal(items.Length * DpiPath.Ios.Length, copied.Length);

				var mdpi = GetCopiedResource(task, "camera.png");
				Assert.Equal("", mdpi.GetMetadata("_ResizetizerDpiPath"));
				Assert.Equal("1.0", mdpi.GetMetadata("_ResizetizerDpiScale"));

				var xhdpi = GetCopiedResource(task, "camera@2x.png");
				Assert.Equal("", xhdpi.GetMetadata("_ResizetizerDpiPath"));
				Assert.Equal("2.0", xhdpi.GetMetadata("_ResizetizerDpiScale"));

				mdpi = GetCopiedResource(task, "camera_color.png");
				Assert.Equal("", mdpi.GetMetadata("_ResizetizerDpiPath"));
				Assert.Equal("1.0", mdpi.GetMetadata("_ResizetizerDpiScale"));

				xhdpi = GetCopiedResource(task, "camera_color@2x.png");
				Assert.Equal("", xhdpi.GetMetadata("_ResizetizerDpiPath"));
				Assert.Equal("2.0", xhdpi.GetMetadata("_ResizetizerDpiScale"));
			}

			[Fact]
			public void SingleImageWithBaseSizeSucceeds()
			{
				var items = new[]
				{
					new TaskItem("images/camera.png", new Dictionary<string, string>
					{
						["BaseSize"] = "44"
					}),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success);

				AssertFileSize("camera.png", 44, 44);
				AssertFileSize("camera@2x.png", 88, 88);
			}

			[Theory]
			[InlineData("camera")]
			[InlineData("camera_color")]
			public void SingleRasterAppIconWithOnlyPathSucceedsWithoutVectors(string name)
			{
				var items = new[]
				{
					new TaskItem($"images/{name}.png", new Dictionary<string, string>
					{
						["IsAppIcon"] = bool.TrueString
					}),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success);

				AssertFileSize($"Assets.xcassets/{name}.appiconset/{name}20x20@2x.png", 40, 40);
				AssertFileSize($"Assets.xcassets/{name}.appiconset/{name}20x20@3x.png", 60, 60);
				AssertFileSize($"Assets.xcassets/{name}.appiconset/{name}60x60@2x.png", 120, 120);
				AssertFileSize($"Assets.xcassets/{name}.appiconset/{name}60x60@3x.png", 180, 180);
				AssertFileSize($"Assets.xcassets/{name}.appiconset/{name}ItunesArtwork.png", 1024, 1024);

				AssertFileExists($"Assets.xcassets/{name}.appiconset/Contents.json");

				AssertFileContains($"Assets.xcassets/{name}.appiconset/Contents.json",
					$"\"filename\": \"{name}20x20@2x.png\"",
					$"\"size\": \"20x20\",");
			}

			[Theory]
			[InlineData("appicon")]
			[InlineData("camera")]
			[InlineData("camera_color")]
			public void SingleVectorAppIconWithOnlyPathSucceedsWithVectors(string name)
			{
				var items = new[]
				{
					new TaskItem($"images/{name}.svg", new Dictionary<string, string>
					{
						["IsAppIcon"] = bool.TrueString
					}),
				};

				var task = GetNewTask(items);
				var success = task.Execute();
				Assert.True(success);

				AssertFileSize($"Assets.xcassets/{name}.appiconset/{name}20x20@2x.png", 40, 40);
				AssertFileSize($"Assets.xcassets/{name}.appiconset/{name}20x20@3x.png", 60, 60);
				AssertFileSize($"Assets.xcassets/{name}.appiconset/{name}60x60@2x.png", 120, 120);
				AssertFileSize($"Assets.xcassets/{name}.appiconset/{name}60x60@3x.png", 180, 180);
				AssertFileSize($"Assets.xcassets/{name}.appiconset/{name}ItunesArtwork.png", 1024, 1024);

				AssertFileExists($"Assets.xcassets/{name}.appiconset/Contents.json");

				AssertFileContains($"Assets.xcassets/{name}.appiconset/Contents.json",
					$"\"filename\": \"{name}20x20@2x.png\"",
					$"\"size\": \"20x20\",");
			}
		}
	}
}
