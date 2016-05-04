using System;
using Serilog;
using Serilog.Events;
using CoreProfiler;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Console.ReadKey();
            
            // initialize Serilog log format and log target
            // here we use console as log target
            Log.Logger = new LoggerConfiguration()
                .WriteTo.LiterateConsole(LogEventLevel.Information, "{Message}{NewLine}")
                .CreateLogger();
            
            // in coreprofiler.json file, we configured the profiling storage as the json storage
            // and the logProvider as Serilog provider
            // the json storage use the Microsoft.Extensions.Logging API for profiling log persistence
            // when the logProvider is specified as Serilog, all profiling logs are persisted via Serilog
            // in this example, we configured Serilog to write to console
            
            // different from in web application, for non-web application,
            // we need to call ProfilingSession.Start()/Stop() explicitly
            // to start/stop a profiling session
            ProfilingSession.Start("session" + Thread.CurrentThread.ManagedThreadId);

            using (ProfilingSession.Current.Step(ProfilingSession.Current.Profiler.GetTimingSession().Name + " - step 1"))
            {
                Thread.Sleep(200);
            }
            
            using (ProfilingSession.Current.Step(ProfilingSession.Current.Profiler.GetTimingSession().Name + " - step 2"))
            {
                Task.Factory.StartNew(() =>
                {
                    using (ProfilingSession.Current.Step(ProfilingSession.Current.Profiler.GetTimingSession().Name + " - step 3"))
                    {
                        Console.WriteLine("Hello Async");
                        Thread.Sleep(50);
                    }
                }).Wait();
                
                using (ProfilingSession.Current.Step(ProfilingSession.Current.Profiler.GetTimingSession().Name + " - step 4"))
                {
                    Thread.Sleep(100);
                }
            }
            
            ProfilingSession.Stop();
        }
    }
}
