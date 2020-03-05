& msbuild /t:Rebuild .\Resizetizer.NT\Resizetizer.NT.csproj /bl

Copy-Item .\output\*.nupkg -Destination C:\nuget\

Remove-Item .\SampleApp\packages\resizetizer.nt -Force -Recurse

& msbuild /t:Restore .\Resizetizer.NT.sln

