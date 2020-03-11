If(!(test-path .\logs\))
{
	New-Item -ItemType Directory -Path .\logs\
}

& msbuild /t:Restore .\Resizetizer.NT\Resizetizer.NT.csproj
& msbuild /t:Rebuild .\Resizetizer.NT\Resizetizer.NT.csproj /bl:logs\resizetizer.binlog

Remove-Item .\SampleApp\packages\resizetizer.nt -Force -Recurse

# Run restore/rebuilds
& msbuild /t:Restore .\SampleApp\SampleApp.Android\SampleApp.Android.csproj
& msbuild /t:Rebuild .\SampleApp\SampleApp.Android\SampleApp.Android.csproj /bl:logs\android.binlog

& msbuild /t:Restore .\SampleApp\SampleApp.iOS\SampleApp.iOS.csproj
& msbuild /t:Rebuild .\SampleApp\SampleApp.iOS\SampleApp.iOS.csproj /bl:logs\ios.binlog

& msbuild /t:Restore .\SampleApp\SampleApp.UWP\SampleApp.UWP.csproj
& msbuild /t:Rebuild .\SampleApp\SampleApp.UWP\SampleApp.UWP.csproj /bl:logs\uwp.binlog

& msbuild /t:Restore .\SampleApp\SampleApp.WPF\SampleApp.WPF.csproj
& msbuild /t:Rebuild .\SampleApp\SampleApp.WPF\SampleApp.WPF.csproj /bl:logs\wpf.binlog

# Touch the file to cause a change for incremental build
(Get-ChildItem .\SampleApp\camera.svg).LastWriteTime = Get-Date

# Run incremental builds
& msbuild /t:Build .\SampleApp\SampleApp.Android\SampleApp.Android.csproj /bl:logs\android-incremental.binlog
& msbuild /t:Build .\SampleApp\SampleApp.iOS\SampleApp.iOS.csproj /bl:logs\ios-incremental.binlog
& msbuild /t:Build .\SampleApp\SampleApp.UWP\SampleApp.UWP.csproj /bl:logs\uwp-incremental.binlog
& msbuild /t:Build .\SampleApp\SampleApp.WPF\SampleApp.WPF.csproj /bl:logs\wpf-incremental.binlog
