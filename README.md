# Resizetizer NT (for Workgroups and fun individuals)

Take SVG's and PNG's and automagically have them resized to all the different resolutions and included in your Xamarin Android, iOS, and UWP projects!

## History
A few years back I created a thing I called [Resizetizer](https://github.com/Redth/Resizetizer) which helped solve the pain of resizing images to every single target density/resolution for iOS, Android, and UWP.  

The original incarnation required that you define a .yaml file with all of your inputs and desired outputs.  If you referenced this yaml file in your project and built it, the images would all be resized according to your config.  You also had to specify all the various resolutions you wanted to output in the config file.

## NT (New Technology)

This was great, but we could do better yet!  The Resizetizer NT (New Technology) improves the story in a few ways:
1. Add your images to your shared (netstandard2) projects with the `SharedImage` build action
2. The config file is no longer necessary (or even a thing at all)
3. Ideal resolutions for each platform are automatically created
4. Resized images are automatically included in your Xamarin iOS, Android and UWP app head projects

## How to use it

First you need to add the `ResizetizerNT` NuGet package to your shared code (netstandard2.0) project, as well as your Xamarin iOS/Android/UWP app projects.  It's important to install it in _all_ of your projects for this to work.

Next, add the images to your shared code (netstandard2.0) project as `SharedImage` build actions:

```xml
<ItemGroup>
  <SharedImage 
    Include="hamburger.svg"
    BaseSize="40,20" />
</ItemGroup>
```

These images can be `.svg` or `.png` types.

## BaseSize
Notice the `BaseSize` attribute which you will need to manually add.  This is required for the resizer to know the baseline or nominal, or original (whatever you want to call it) density size.  This is the size you would use in your code to specify the image size (eg: `<Image Source="hamburger.png" WidthRequest="40" HeightRequest="20" />`).  In Android this is the `drawable-mdpi` resolution, on iOS you can consider it the `@1x` resolution, and on UWP it's the `scale-100` resolution.

## Referencing the Resized Images
When you build your app projects, they will invoke a target in your shared code project to collect all of the `SharedImage` items to process.  These items will all be resized and automatically included in your app project as the appropriate type for an image resource.

You can reference images just like you normally would.  The important thing to note is that if your input image source is `.svg`, you will actually reference a `.png` in your app.

In Xamarin Forms you would use something like:

```xml
<Image Source="hamburger.png" WidthRequest="40" HeightRequest="20" />
```

# Planning

## Bitmaps
By default if no `BaseSize` attribute is specified on the `SharedImage` item, the image should not be resized at all and instead simply included as a standard image in the app head project (`Resources/drawable` on android, etc.).
If an `BaseSize` is specified, then the resizing rules apply to the image and different copies of the image should be made for the various display densities of each app head project.

## Vectors
Vectors provide the best results for resizing an image to the display densities of each device. 
By default if no `BaseSize` attribute is specified on the `SharedImage` item, the vector should not resized at all and is simply included as a standard vector image in the app head project (`Resources/drawable` on android, etc.).  

> On Android the vector should also be converted to a Vector Drawable format resource. 

Since by its very nature a vector’s size is unknown, if it is to be resized, a `BaseSize` must be included to support resizing to different display densities. Resize rules apply here. 

## Resize Rules

When an image needs to be resized for various display densities we need to know it's baseline or standard 1.0 device display density scale or factor in order to derive other image sizes to resize for other densities. 

For the purpose of adding images to a project this is being called the `BaseSize`, and is specified in the project’s `SharedImage` item.

When no `BaseSize` is specified, on android this maps to `drawable-mdpi`, on iOS this is the equivalent of `@1x`, and on UWP it is `scale-100`. 

When an `BaseSize` is specified this is considered the 1.0 scale factor for the images. All other density sizes are derived from this. 

The original size is typically the expected density independent pixel size you intend to use the image in. For example this is the size you would intend to set for the `RequestedWidth` and `RequestedHeight` of a Xamarin.Forms Image control.

There are some default densities depending on target platform:

### Android
- 1.0 `drawable-mdpi`
- 1.5 `drawable-hdpi`
- 2.0 `drawable-xhdpi`
- 3.0 `drawable-xxhdpi`
- 4.0 `drawable-xxxhdpi`

### iOS
- 1.0 `@1x`
- 2.0 `@2x`
- 3.0 `@3x`

### UWP 
- 1.0 `scale-100`
- 2.0 `scale-200`
- 3.0 `scale-300` ?
- 4.0 `scale-400` ?

 > Not sure if 3.0 and 4.0 scales are used in UWP or common.  Need to decide if we should generate these or not. Interestingly UWP also allows for high vs low contrast images.  Might be something to think about down the road.

## Other Platforms

Tizen? MacOS? WPF?

## IDE Integration

The proposed solution does not require but may benefit from some IDE customization or integration. 

Adding items to a csproj manually is becoming more normal in sdk style projects.

### Project Intellisense

It appears to be possible for us to add to the visual studio XSD for generating Intellisense inside project files so that attributes like `BaseSize` will show up as possible options. 

### Property Editor

You can already add images to a project which default to be in the  `None` item group. You will be able to select `SharedImage` as a build action. In addition it may be possible to add properties to the property editor for things like `BaseSize` to be accessible from the IDE UI.

## Other Features

There is an opportunity with things like vectors to provide some other interesting features:

- TintColor for vectors could look something like this, where all paths would be set to fill with the specified color:
    `<SharedImage Include="my.svg" Size="10,10" TintColor="#123456" />`
- Optimize/Crunch output images (eg: pngcrunch)
    - iOS does this as a build step already don’t think android does, not sure if this fits into this step or if we should look at baking png optimization into android core build
- Custom DPI outputs: This would be a power user feature which could allow the project item to specify the densities which will be generated for the image including the path and file suffix on a per platform basis, which might be helpful for things like app icons.  This could look something like:
    ```xml
    <SharedImage Include="my.svg" Size="10,10">
      <!-- Maybe Apple and Android add 5.0 scale support before we do? -->
      <OutputSize Scale="5.0" PathPrefix="drawable-xxxxhdpi" />
      <OutputSize Scale="5.0" FileNamePostfix="@5x" />
    </SharedImage>
    ```
- AppIcons - on iOS these are asset catalogs? Maybe there should be an attribute or a new build action specifically for app icons which generates mipmap drawables on android and asset catalogs on iOS since this is a pretty specific use case

# Progress

**Completed**
- Android msbuild targets are working correctly. 
- iOS msbuild targets are working correctly.
- Image resizing
    - SVG→Bitmap scaling works with Skia.Svg
    - Bitmap→Bitmap scaling works with SkiaSharp 

**In Progress**
- Add `TintColor` support for SVG
- Need to add svg→android vector processing support
- UWP
