#echo off
cd CoreProfiler
dotnet pack -c Release
cd ../CoreProfiler.Web
dotnet pack -c Release
cd ..