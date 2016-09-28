.nuget\nuget push CoreProfiler\bin\Release\*.symbols.nupkg -Source https://nuget.smbsrc.net/
del CoreProfiler\bin\Release\*.symbols.nupkg
.nuget\nuget push CoreProfiler\bin\Release\*.nupkg -Source https://www.nuget.org

.nuget\nuget push CoreProfiler.Web\bin\Release\*.symbols.nupkg -Source https://nuget.smbsrc.net/
del CoreProfiler.Web\bin\Release\*.symbols.nupkg
.nuget\nuget push CoreProfiler.Web\bin\Release\*.nupkg -Source https://www.nuget.org

.nuget\nuget push CoreProfiler.Wcf\bin\Release\*.symbols.nupkg -Source https://nuget.smbsrc.net/
del CoreProfiler.Wcf\bin\Release\*.symbols.nupkg
.nuget\nuget push CoreProfiler.Wcf\bin\Release\*.nupkg -Source https://www.nuget.org
