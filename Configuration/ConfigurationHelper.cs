using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace EF.Diagnostics.Profiling.Configuration
{
    public static class ConfigurationHelper
    {
        internal static readonly ILoggerFactory LogFactory = new LoggerFactory();

        public static ILogger GetLogger<T>()
        {
            return LogFactory.CreateLogger<T>();
        }

        public static IConfiguration GetConfiguration()
        {
            return new ConfigurationBuilder()
               .AddJsonFile("coreprofiler.json", true)
               .Build();
        }
    }
}
