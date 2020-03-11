#!/bin/bash

mkdir -p ./logs

msbuild /t:Restore ./Resizetizer.NT/Resizetizer.NT.csproj
msbuild /t:Rebuild ./Resizetizer.NT/Resizetizer.NT.csproj /bl:logs/resizetizer.binlog

rm -rf ./SampleApp/packages/resizetizer.nt

# Run restore/rebuilds
msbuild /t:Restore ./SampleApp/SampleApp.Android/SampleApp.Android.csproj
msbuild /t:Rebuild ./SampleApp/SampleApp.Android/SampleApp.Android.csproj /bl:logs/android.binlog

msbuild /t:Restore ./SampleApp/SampleApp.iOS/SampleApp.iOS.csproj
msbuild /t:Rebuild ./SampleApp/SampleApp.iOS/SampleApp.iOS.csproj /bl:logs/ios.binlog

# Touch the file to cause a change for incremental build
touch ./SampleApp/camera.svg

# Run incremental builds
msbuild /t:Build ./SampleApp/SampleApp.Android/SampleApp.Android.csproj /bl:logs/android-incremental.binlog
msbuild /t:Build ./SampleApp/SampleApp.iOS/SampleApp.iOS.csproj /bl:logs/ios-incremental.binlog
