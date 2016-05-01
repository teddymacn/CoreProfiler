using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using EF.Diagnostics.Profiling.Storages;
using EF.Diagnostics.Profiling.Timings;

namespace EF.Diagnostics.Profiling
{
    public class TestStorage : ProfilingStorageBase
    {
        public static ConcurrentQueue<ITimingSession> Queue = new ConcurrentQueue<ITimingSession>();

        protected override void Save(ITimingSession session)
        {
            Queue.Enqueue(session);
        }
    }

    public static class Program
    {
        public static void Main(string[] args)
        {
            //Console.Read();

            ProfilingSession.ProfilingStorage = new NoOperationProfilingStorage();

            Parallel.For(0, 10, i =>
            {
                Run();

                Task.Factory.StartNew(() =>
                {
                    if (ProfilingSession.Current != null)
                        Console.WriteLine("something wrong");
                }).Wait();
            });
        }

        private static void Run()
        {
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
                        if (ProfilingSession.Current != null)
                            Console.WriteLine("profiling ok");

                        Console.WriteLine("Hello Async");
                    }
                }).Wait();

                using (ProfilingSession.Current.Step(ProfilingSession.Current.Profiler.GetTimingSession().Name + " - step 4"))
                {
                    Thread.Sleep(100);
                }
            }

            lock (typeof (TestStorage))
            {
                Console.WriteLine(ProfilingSession.Current.Profiler.GetTimingSession().Name);
                foreach (var timing in ProfilingSession.Current.Profiler.GetTimingSession().Timings)
                {
                    Console.WriteLine(timing.Name);
                }
                Console.WriteLine("");
            }

            ProfilingSession.Stop();
        }
    }
}
