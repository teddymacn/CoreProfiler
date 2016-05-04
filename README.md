CoreProfiler
============

CoreProfiler is a light weight performance profiling library for .NET Core.

CoreProfiler is a port of [NanoProfiler](https://github.com/englishtown/nanoprofiler) by the same author of NanoProfiler.

The same as NanoProfiler, for web application, CoreProfiler provides a wonderful view-result Web UI supports view latest profiling results in a tree-timeline view (simply visit ~/coreprofiler/view in your web application). 

For documentations of NanoProfiler, please check out wiki pages: https://github.com/englishtown/nanoprofiler/wiki

How to compile the source code?
-------------------------------

- Download latest version of dotnet cli from [https://github.com/dotnet/cli](https://github.com/dotnet/cli)
- git clone https://github.com/teddymacn/CoreProfiler.git
- Run .\build.ps1
- You could also use [Visual Studio Code](https://code.visualstudio.com/) to open the project folders for editting

License terms
-------------
CoreProfiler is released under the [MIT license](https://mit-license.org/).

Basic usage
-----------

- For profiling a web applications, you need to add references to both CoreProfiler and CoreProfiler.Web.
- In the Startup.cs file, you need to add the code below to enable CoreProfiler profiling:

	app.UseCoreProfiler();

- Add a [coreprofiler.json](https://github.com/teddymacn/CoreProfiler/blob/master/mvc-ef-demo/coreprofiler.json) file to your project and make sure it is in [the include of copyToOutput in your project.json](https://github.com/teddymacn/CoreProfiler/blob/master/mvc-ef-demo/project.json#L23).
- If you want to profile DB queries, you need to wrap DbConnection instances like for example: [here](https://github.com/teddymacn/CoreProfiler/blob/master/mvc-ef-demo/Startup.cs#L24).
- In the coreprofiler.json file, you could configure some options of the profiling, including specifying custom configuration provider, log provider and persistence storage for persisting, profiling filters, number of profiling results to be kept in memory for display, etc. I'll add more documents to talk about them soon later.

You can check the [mvc-ef-demo](https://github.com/teddymacn/CoreProfiler/tree/master/mvc-ef-demo) project in the source code for the details.
