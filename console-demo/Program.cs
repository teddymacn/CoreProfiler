using System;
using CoreProfiler;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Console.ReadKey();
            
            // initialize logger as the Console log provider
            // the json storage writes log to the ProfilingSession.LoggerFactory based logger
            ProfilingSession.LoggerFactory.AddConsole();
            
            // in coreprofiler.json file, we configured the profiling storage as the json storage
            // the json storage use the Microsoft.Extensions.Logging API for profiling log persistence
            
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
