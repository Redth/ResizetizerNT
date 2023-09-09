# What's changed?

This is the fork of the https://github.com/Redth/ResizetizerNT/tree/master repo with additionaly added features.
- All the libraries updated to the most recent versions;
- Added support for attribute `Format='Jpeg,Quality'` for `<SharedImage/>` and logic behind;
- Improved error logging, included additional details into error messages;
- Package Id changed Resizetizer.NT -> Resizetizer.NT.2023, to be able to publish it to the NuGet.

# Where is full ReadMe?
To read the full readme file, please follow this link https://github.com/Redth/ResizetizerNT
## NuGet
NuGet package is listed under a new name `Resizetizer.NT.2023` here - https://www.nuget.org/packages/Resizetizer.NT.2023

## Jpeg output format and Jpeg Quality
If you want to use PNG source image, but you don't want to include PNG into your mobile application project, now you can set the output image format as JPEG, plus set the Jpeg Quality.
This is very useful when you was proveded with PNG based screen backgrounds for your application and you don't want to spend any time in the image editing software.

```xml
<ItemGroup>
  <SharedImage Include="SharedImages\app_background_screen.png" BaseSize="480,800" Format="Jpeg,70" />
</ItemGroup>
```

## Overriding included images

TBD
