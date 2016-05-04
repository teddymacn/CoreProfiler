using CoreProfiler.ProfilingFilters;
using CoreProfiler.Storages;
using CoreProfiler.Timings;
using System.Collections.Generic;

namespace CoreProfiler.Configuration
{
    /// <summary>
    /// Reprensent a configuration provider.
    /// </summary>
    public interface IConfigurationProvider
    {
        /// <summary>
        /// Gets the profiling storage.
        /// </summary>
        IProfilingStorage Storage { get; }

        /// <summary>
        /// Gets the profiling filters.
        /// </summary>
        IEnumerable<IProfilingFilter> Filters { get; }

        /// <summary>
        /// Gets the profiling circular buffer.
        /// </summary>
        ICircularBuffer<ITimingSession> CircularBuffer { get; }
    }
}
