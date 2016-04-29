using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace EF.Diagnostics.Profiling
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.Read();
            
            ProfilingSession.Start("session1");

            using (ProfilingSession.Current.Step("step 1"))
            {
                Thread.Sleep(1000);
                Console.WriteLine(ProfilingSession.Current.Profiler.GetTimingSession().Timings.Count());
            }

            using (ProfilingSession.Current.Step("step 2"))
            {
                Console.WriteLine(ProfilingSession.Current.Profiler.GetTimingSession().Timings.Count());
                
                Task.Factory.StartNew(() =>
                {
                    using (ProfilingSession.Current.Step("step 3"))
                    {
                        Console.WriteLine(ProfilingSession.Current.Profiler.GetTimingSession().Timings.Count());
                        
                        if (ProfilingSession.Current != null)
                            Console.WriteLine("profiling ok");
                        
                        Console.WriteLine("Hello Async");
                    }
                });

                using (ProfilingSession.Current.Step("step 4"))
                {
                    Console.WriteLine(ProfilingSession.Current.Profiler.GetTimingSession().Timings.Count());
                    
                    Thread.Sleep(500);
                }
                
                Console.WriteLine(ProfilingSession.Current.Profiler.GetTimingSession().Timings.Count());
            }
            
            Console.WriteLine(ProfilingSession.Current.Profiler.GetTimingSession().Timings.Count());

            ProfilingSession.Stop();
        }
    }
}
