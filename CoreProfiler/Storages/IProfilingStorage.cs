using CoreProfiler.Timings;

namespace CoreProfiler.Storages
{
    /// <summary>
    /// Represents a generic profiling storage.
    /// </summary>
    public interface IProfilingStorage
    {
        /// <summary>
        /// Saves a profiling timing session.
        /// </summary>
        /// <param name="session">The <see cref="ITimingSession"/> to be saved.</param>
        void SaveSession(ITimingSession session);
    }
}
