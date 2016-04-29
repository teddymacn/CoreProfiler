using System.Collections.Generic;

namespace EF.Diagnostics.Profiling.Timings
{
    /// <summary>
    /// Represents a profiling timing session.
    /// </summary>
    public interface ITimingSession : ITiming
    {
        /// <summary>
        /// Gets or sets the machine name.
        /// </summary>
        string MachineName { get; set; }

        /// <summary>
        /// Gets or sets the timings.
        /// </summary>
        IEnumerable<ITiming> Timings { get; set; }

        /// <summary>
        /// Adds a timing to the session.
        /// </summary>
        /// <param name="timing"></param>
        void AddTiming(ITiming timing);
    }
}
