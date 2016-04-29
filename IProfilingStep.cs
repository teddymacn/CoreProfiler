using EF.Diagnostics.Profiling.Timings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EF.Diagnostics.Profiling
{
    /// <summary>
    /// Represents a profiling step of a <see cref="IProfiler"/>.
    /// </summary>
    public interface IProfilingStep : IDisposable
    {
        /// <summary>
        /// Discards the current profiling step.
        /// </summary>
        void Discard();

        /// <summary>
        /// Gets the timing of the profiling current step.
        /// </summary>
        /// <returns></returns>
        ITiming GetStepTiming();

        /// <summary>
        /// Stops the current <see cref="ProfilingStep"/> and adds the <see cref="ProfilingStep"/> to profiler.
        /// </summary>
        /// <param name="addToProfiler">
        ///     Whether or not add the current <see cref="ProfilingStep"/> to profiler when stops.
        /// </param>
        void Stop(bool addToProfiler);

        /// <summary>
        /// Add a tag to current profiling step.
        /// </summary>
        /// <param name="tag"></param>
        void AddTag(string tag);

        /// <summary>
        /// Add a custom data field to current profiling step.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void AddField(string key, string value);
    }
}
