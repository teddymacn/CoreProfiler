#echo off
cd CoreProfiler
dotnet build -c Release --no-incremental
cd ../CoreProfiler.Web
dotnet build -c Release --no-incremental
cd ../CoreProfiler.Wcf
dotnet build -c Release --no-incremental
cd ..