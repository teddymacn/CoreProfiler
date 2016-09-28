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

- For profiling a web applications, you need to add references to both CoreProfiler and CoreProfiler.Web packages.

- For profiling a console or web application which call wcf services, please also reference Console.Wcf package.

    To enable wcf profiling in a .net core project, you need to implement the partial method below for each of your wcf client:

    ``` csharp
    static partial void ConfigureEndpoint(ServiceEndpoint serviceEndpoint, ClientCredentials clientCredentials)
    {
        serviceEndpoint.EndpointBehaviors.Add(new WcfProfilingBehavior());
    }
    ```

- In the Startup.cs file, you need to add the code below as the first app.UseXXX() pipeline to enable CoreProfiler profiling:

	app.UseCoreProfiler(drillDown:true); //if drillDown=true, try to drill down child requests from external apps when view profiling results

- Add a [coreprofiler.json](https://github.com/teddymacn/CoreProfiler/blob/master/mvc-ef-demo/coreprofiler.json) file to your project and make sure it is in [the include of publishOptions in your project.json](https://github.com/teddymacn/CoreProfiler/blob/master/mvc-ef-demo/project.json#L28).

- If you want to profile DB queries, you need to wrap DbConnection instances like for example: [here](https://github.com/teddymacn/CoreProfiler/blob/master/mvc-ef-demo/Startup.cs#L24).

- In the coreprofiler.json file, you could configure some options of the profiling, including specifying custom configuration provider, log provider and persistence storage for persisting, profiling filters, number of profiling results to be kept in memory for display, etc.

You can check the [mvc-ef-demo](https://github.com/teddymacn/CoreProfiler/tree/master/mvc-ef-demo) sample for how to enable profiling in a mvc + entityframeworkcore web application.

You can check the [console-demo](https://github.com/teddymacn/CoreProfiler/blob/master/console-demo) sample for how to use JsonProfilingStorage to persist logs and [how to start/stop profiling in a non-web application](https://github.com/teddymacn/CoreProfiler/blob/master/console-demo/Program.cs).


Sample Projects
---------------

- [cross-app-profiling-demo](https://github.com/teddymacn/cross-app-profiling-demo) - Sample projects to demonstrate cross-application performance profiling with coreprofiler/nanoprofiler.

