using System;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using EF.Diagnostics.Profiling.Data;
using EF.Diagnostics.Profiling.Storages;
using EF.Diagnostics.Profiling.Timings;
using Microsoft.Data.Sqlite;

namespace EF.Diagnostics.Profiling
{
    public class TestStorage : ProfilingStorageBase
    {
        public static ConcurrentQueue<ITimingSession> Queue = new ConcurrentQueue<ITimingSession>();

        protected override void Save(ITimingSession session)
        {
            Queue.Enqueue(session);

            Console.WriteLine("saved - " + session.Name);
        }
    }

    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.Read();

            ProfilingSession.ProfilingStorage = new TestStorage();

            Run();

            Console.ReadKey();
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

                Task.Factory.StartNew(async () =>
                {
                    using (ProfilingSession.Current.Step(ProfilingSession.Current.Profiler.GetTimingSession().Name + " - step 3"))
                    {
                        if (ProfilingSession.Current != null)
                            Console.WriteLine("profiling ok");

                        Console.WriteLine("Hello Async");

                        using (var conn = GetConnection())
                        using (var cmd = conn.CreateCommand())
                        {
                            await conn.OpenAsync();

                            cmd.CommandText = "select * from country";
                            using (var rdr = await cmd.ExecuteReaderAsync())
                            {
                                while (await rdr.ReadAsync())
                                {
                                    Console.WriteLine(rdr.GetString(0));
                                }
                            }
                        }
                    }
                }).Wait();

                using (ProfilingSession.Current.Step(ProfilingSession.Current.Profiler.GetTimingSession().Name + " - step 4"))
                {
                    Thread.Sleep(100);
                }
            }

            ProfilingSession.Stop();
        }

        private static DbConnection GetConnection()
        {
            return new ProfiledDbConnection(new SqliteConnection(@"Data Source=D:\git\CoreProfilerDev\demo.sqlite;"), new DbProfiler(ProfilingSession.Current.Profiler));
        }
    }
}
