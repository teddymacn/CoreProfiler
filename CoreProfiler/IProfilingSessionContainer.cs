using System;

namespace CoreProfiler
{
    /// <summary>
    /// Represents a container of the current ProfilingSession.
    /// </summary>
    public interface IProfilingSessionContainer
    {
        /// <summary>
        /// Gets or sets the current ProfilingSession.
        /// </summary>
        ProfilingSession CurrentSession { get; set; }

        /// <summary>
        /// Gets or sets the current profiling step id.
        /// </summary>
        Guid? CurrentSessionStepId { get; set; }

        /// <summary>
        /// Clears the current profiling session &amp; step id.
        /// </summary>
        void Clear();
    }
}
