using Xunit;

namespace Resizetizer.NT.Tests
{
	public class ResizerTests
	{
		private readonly DpiPath _dpiPath =  new DpiPath("", 1);

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("blue_")]
		public void PrependImageNamePrefix(string prefix)
		{
			var info = new SharedImageInfo
			{
				Filename = "image.svg",
				ImageNamePrefix = prefix
			};
			
			var fixture = CreateClass(info);
			var result = fixture.GetFileDestination(_dpiPath);
			
			Assert.EndsWith($"{prefix}image.svg", result);
		}
		
		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData("_blue")]
		public void AppendImageNamePrefix(string postfix)
		{
			var info = new SharedImageInfo
			{
				Filename = "image.svg",
				ImageNamePostfix = postfix
			};
			
			var fixture = CreateClass(info);
			var result = fixture.GetFileDestination(_dpiPath);
			
			Assert.EndsWith($"image{postfix}.svg", result);
		}

		private static Resizer CreateClass(SharedImageInfo sharedImageInfo)
		{
			return new Resizer(sharedImageInfo, "ResizerTests", new TestLogger());
		}
	}
}
