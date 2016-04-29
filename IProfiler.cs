using EF.Diagnostics.Profiling.Timings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EF.Diagnostics.Profiling
{
    /// <summary>
    /// Represents of a generic profiler.
    /// </summary>
    public interface IProfiler
    {
        /// <summary>
        /// Gets the identity of the profiler.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the elapsed time since the start of the <see cref="IProfiler"/>.
        /// </summary>
        /// <returns></returns>
        TimeSpan Elapsed { get; }

        /// <summary>
        /// Whether or not the current profiler is stopped.
        /// </summary>
        bool IsStopped { get; }

        /// <summary>
        /// Creates an <see cref="IProfilingStep"/> that will time the code between its creation and disposal.
        /// </summary>
        /// <param name="name">The name of the step.</param>
        /// <param name="tags">The tags of the step.</param>
        /// <returns>Returns the created <see cref="IProfilingStep"/>.</returns>
        IProfilingStep Step(string name, TagCollection tags);

        /// <summary>
        /// Returns an <see cref="System.IDisposable"/> that will ignore the profiling between its creation and disposal.
        /// </summary>
        /// <returns>Returns the created <see cref="System.IDisposable"/> as the ignored step.</returns>
        IDisposable Ignore();

        /// <summary>
        /// Stops the profiling of the current profiler.
        /// </summary>
        /// <param name="discardResults">
        /// When true, ignore the profiling results of the profiler.
        /// </param>
        void Stop(bool discardResults = false);

        /// <summary>
        /// Gets the timing session of the current profiler.
        /// </summary>
        /// <returns></returns>
        ITimingSession GetTimingSession();
    }
}
