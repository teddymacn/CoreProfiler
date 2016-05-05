using System;
using System.Threading.Tasks;

namespace CoreProfiler.Web
{
    public static class ProfilingSessionExtensions
    {
        public static async Task WebTimingAsync(this ProfilingSession current, string url, Func<string, Task> func)
        {            
            if (current == null && func != null)
			{
				await func(null);
				return;
			}
            
            if (current != null && func != null)
            {
                var webTiming = new WebTiming(current.Profiler, url);
                try
                {
                    await func(webTiming.CorrelationId);
                }
                finally
                {
                    webTiming.Stop();
                }
            }
        }
    }
}