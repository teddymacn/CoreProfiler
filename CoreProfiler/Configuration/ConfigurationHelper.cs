using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;

namespace CoreProfiler.Configuration
{
    public static class ConfigurationHelper
    {
        public static ILogger GetLogger<T>()
        {
            return ProfilingSession.LoggerFactory.CreateLogger(typeof(T).FullName);
        }

        public static IConfiguration GetConfiguration()
        {
            return new ConfigurationBuilder()
               .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
               .AddJsonFile("coreprofiler.json", true)
               .Build();
        }
    }
}
