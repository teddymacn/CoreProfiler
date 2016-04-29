using EF.Diagnostics.Profiling.ProfilingFilters;
using EF.Diagnostics.Profiling.Storages;
using EF.Diagnostics.Profiling.Timings;
using System.Collections.Generic;

namespace EF.Diagnostics.Profiling.Configuration
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
