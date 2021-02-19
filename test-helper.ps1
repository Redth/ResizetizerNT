New-Item -ItemType Directory -Path .\logs\ -Force
New-Item -ItemType Directory -Path .\output\ -Force

& msbuild /r /t:Rebuild .\Resizetizer.NT\Resizetizer.NT.csproj /bl:logs\resizetizer.binlog

Remove-Item .\SampleApp\packages\resizetizer.nt -Force -Recurse

# Run restore/rebuilds
& msbuild /r /t:Rebuild .\SampleApp\SampleApp.Android\SampleApp.Android.csproj /bl:logs\android.binlog
& msbuild /r /t:Rebuild .\SampleApp\SampleApp.iOS\SampleApp.iOS.csproj /bl:logs\ios.binlog
& msbuild /r /t:Rebuild .\SampleApp\SampleApp.UWP\SampleApp.UWP.csproj /bl:logs\uwp.binlog
& msbuild /r /t:Rebuild .\SampleApp\SampleApp.WPF\SampleApp.WPF.csproj /bl:logs\wpf.binlog

# Touch the file to cause a change for incremental build
(Get-ChildItem ".\SampleApp\SampleApp\camera.svg").LastWriteTime = Get-Date
(Get-ChildItem ".\SampleApp\SampleApp\feather.ttf").LastWriteTime = Get-Date

# Run incremental builds
& msbuild /t:Build .\SampleApp\SampleApp.Android\SampleApp.Android.csproj /bl:logs\android-incremental.binlog
& msbuild /t:Build .\SampleApp\SampleApp.iOS\SampleApp.iOS.csproj /bl:logs\ios-incremental.binlog
& msbuild /t:Build .\SampleApp\SampleApp.UWP\SampleApp.UWP.csproj /bl:logs\uwp-incremental.binlog
& msbuild /t:Build .\SampleApp\SampleApp.WPF\SampleApp.WPF.csproj /bl:logs\wpf-incremental.binlog
