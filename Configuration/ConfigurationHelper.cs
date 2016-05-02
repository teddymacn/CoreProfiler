using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.Configuration.Json;

namespace EF.Diagnostics.Profiling.Configuration
{
    public static class ConfigurationHelper
    {
        public static ILogger GetLogger<T>()
        {
            return new LoggerFactory().CreateLogger<T>();
        }

        public static IConfiguration GetConfiguration()
        {
            return new ConfigurationBuilder()
               .AddJsonFile("coreprofiler.json", true)
               .Build();
        }
    }
}
