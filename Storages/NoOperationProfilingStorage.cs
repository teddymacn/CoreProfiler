using CoreProfiler.Timings;

namespace CoreProfiler.Storages
{
    /// <summary>
    /// A <see cref="IProfilingStorage"/> implementation which performs no operation.
    /// </summary>
    public sealed class NoOperationProfilingStorage : ProfilingStorageBase
    {
        #region JsonProfilingStorage Members

        /// <summary>
        /// Saves an <see cref="ITimingSession"/>.
        /// </summary>
        /// <param name="session"></param>
        protected override void Save(ITimingSession session)
        {
            // no operation
        }

        #endregion
    }
}
