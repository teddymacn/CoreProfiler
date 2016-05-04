.nuget\nuget push CoreProfiler\bin\Release\*.symbols.nupkg
del CoreProfiler\bin\Release\*.symbols.nupkg
.nuget\nuget push CoreProfiler\bin\Release\*.nupkg

.nuget\nuget push CoreProfiler.Web\bin\Release\*.symbols.nupkg
del CoreProfiler.Web\bin\Release\*.symbols.nupkg
.nuget\nuget push CoreProfiler.Web\bin\Release\*.nupkg
