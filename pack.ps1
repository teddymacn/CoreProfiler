#echo off
cd CoreProfiler
del bin\Release\*.nupkg
dotnet pack -c Release
cd ../CoreProfiler.Web
del bin\Release\*.nupkg
dotnet pack -c Release
cd ../CoreProfiler.Wcf
del bin\Release\*.nupkg
dotnet pack -c Release
cd ..