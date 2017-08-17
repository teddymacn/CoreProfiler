#echo off
cd CoreProfiler
del bin\Release\*.nupkg
dotnet pack -c Release --include-symbols
cd ../CoreProfiler.Web
del bin\Release\*.nupkg
dotnet pack -c Release --include-symbols
cd ../CoreProfiler.Wcf
del bin\Release\*.nupkg
dotnet pack -c Release --include-symbols
cd ..