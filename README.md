# Shared Images in Xamarin

# Rationale

Currently using images in Xamarin and Xamarin.Forms projects requires adding them to each individual app head project. It also often involves resizing images for multiple display densities. Basically, image management is currently tedious and very unintuitive in Xamarin apps.

# Proposed Solution

Images can be added to shared projects one time and used in the app head projects without manually copying them or resizing them to multiple display densities.

Images should be added to the project with a `SharedImage` build action (item group). 


    <ItemGroup>
      <SharedImage 
        Include="img.png"
        BaseSize="44,44" />
    </ItemGroup>

During builds of the app head projects, these items are discovered and copied to the app head intermediate output paths and includes as an appropriate resource type during the build. 

The user should never see the images in any of the app head projects. 

Both bitmap (png, jpeg) and vector (svg) formats should be supported. 

## Bitmaps

By default if no `BaseSize` attribute is specified on the `SharedImage` item, the image is not resized at all and is simply included as a standard image in the app head project (`Resources/drawable` on android, etc.).

If an `BaseSize` is specified, then the resizing rules apply to the image and different copies of the image are made for the various display densities of each app head project.

## Vectors

Vectors provide the best results for resizing an image to the display densities of each device. 

By default if no `BaseSize` attribute is specified on the `SharedImage` item, the vector is not resized at all and is simply included as a standard vector image in the app head project (`Resources/drawable` on android, etc.).  


> On Android the vector will also be converted to a Vector Drawable format resource. 

Since by its very nature a vector’s size is unknown, if it is to be resized, a `BaseSize` must be included to support resizing to different display densities. Resize rules apply here. 

# Resize Rules

When an image needs to be resized for various display densities we need to know it's baseline or standard 1.0 device display density scale or factor in order to derive other image sizes to resize for other densities. 

For the purpose of adding images to a project this is being called the `BaseSize`, and is specified in the project’s `SharedImage` item.

When no `BaseSize` is specified, on android this maps to `drawable-mdpi`, on iOS this is the equivalent of `@1x`, and on UWP it is `scale-100`. 

When an `BaseSize` is specified this is considered the 1.0 scale factor for the images. All other density sizes are derived from this. 

The original size is typically the expected density independent pixel size you intend to use the image in. For example this is the size you would intend to set for the `RequestedWidth` and `RequestedHeight` of a Xamarin.Forms Image control.

There are some default densities depending on target platform:

## Android
- 1.0 `drawable-mdpi`
- 1.5 `drawable-hdpi`
- 2.0 `drawable-xhdpi`
- 3.0 `drawable-xxhdpi`
- 4.0 `drawable-xxxhdpi`
## iOS
- 1.0 `@1x`
- 2.0 `@2x`
- 3.0 `@3x`
## UWP 
- 1.0 `scale-100`
- 2.0 `scale-200`
- 3.0 `scale-300` ?
- 4.0 `scale-400` ?

Not sure if 3.0 and 4.0 scales are used in UWP or common.  Need to decide if we should generate these or not.
Interestingly UWP also allows for high vs low contrast images.  Might be something to think about down the road.

## Other Platforms

Tizen? MacOS? WPF?

# IDE Integration

The proposed solution does not require but may benefit from some IDE customization or integration. 

Adding items to a csproj manually is becoming more normal in sdk style projects.

## Project Intellisense 

It appears to be possible for us to add to the visual studio XSD for generating Intellisense inside project files so that attributes like `BaseSize` will show up as possible options. 

## Property Editor

You can already add images to a project which default to be in the  `None` item group. You will be able to select `SharedImage` as a build action. In addition it may be possible to add properties to the property editor for things like `BaseSize` to be accessible from the IDE UI.

# Other Features

There is an opportunity with things like vectors to provide some other interesting features:

- TintColor for vectors could look something like this, where all paths would be set to fill with the specified color:
    <SharedImage Include="my.svg" Size="10,10" TintColor="#123456" />
- Optimize/Crunch output images (eg: pngcrunch)
    - iOS does this as a build step already don’t think android does, not sure if this fits into this step or if we should look at baking png optimization into android core build
- Custom DPI outputs: This would be a power user feature which could allow the project item to specify the densities which will be generated for the image including the path and file suffix on a per platform basis, which might be helpful for things like app icons.  This could look something like:
    <SharedImage Include="my.svg" Size="10,10">
      <!-- Maybe Apple and Android add 5.0 scale support before we do? -->
      <OutputSize Scale="5.0" PathPrefix="drawable-xxxxhdpi" />
      <OutputSize Scale="5.0" FileNamePostfix="@5x" />
    </SharedImage>
- AppIcons - on iOS these are asset catalogs? Maybe there should be an attribute or a new build action specifically for app icons which generates mipmap drawables on android and asset catalogs on iOS since this is a pretty specific use case
# Image Manipulation Libraries

The most difficult part of the implementation is turning out to be finding a reliable, appropriately licensed library to use for resizing the images and even more, finding one that can deal with SVG’s.

- ImageSharp - Works great for Bitmap resizing
- Svg - Has a dependency on Fizzler which is GPL - not sure if this is a problem if we just redistribute the library as is?
- SkiaSharp - can do both SVG and Bitmap resizing
    - Has very limited and rudimentary SVG support
    - Issues with current version in resizing bitmaps, and SkiaSharp.SVG doesn’t support latest SkiaSharp core library
- librsvg - Idea: Maybe we can bind just enough of this for our own needs here?
- svg2ad - android drawables are a subset of svg. Basically just paths with different fills (gradients are supported). There are a couple of older open source efforts in converting svg to android drawables, we could use them as a starting point. Android studio also has an implementation of this and it appears to be open source. We could port this or just use the java code: [https://android.googlesource.com/platform/tools/base/+/refs/heads/mirror-goog-studio-master-dev/sdk-common/src/main/java/com/android/ide/common/vectordrawable/](https://android.googlesource.com/platform/tools/base/+/refs/heads/mirror-goog-studio-master-dev/sdk-common/src/main/java/com/android/ide/common/vectordrawable/)
# Questions & Considerations
- How does this affect build times?
    - If we generate images in the app head a rebuild will cause them to be regenerated which could be slow.
    - Should we use a more global cache location to make rebuilds where source images haven't changed faster?
# Progress
## Completed
- Android msbuild targets are working correctly. 
- iOS msbuild targets are working correctly.
- Image resizing
    - SVG→Bitmap scaling works with Skia.Svg
    - Bitmap→Bitmap scaling works with SkiaSharp 
## In Progress 
- Add `TintColor` support for SVG
- Need to add svg→android vector processing support
- UWP
- Restructure repo to make samples better
- Package nuget



