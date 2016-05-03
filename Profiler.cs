using CoreProfiler.Storages;
using CoreProfiler.Timings;
using System;
using System.Diagnostics;
using System.Linq;

namespace CoreProfiler
{
    /// <summary>
    /// The default <see cref="IProfiler"/> implementation.
    /// </summary>
    public class Profiler : IProfiler
    {
        private readonly IProfilingStorage _storage;
        private readonly Stopwatch _stopwatch;
        private readonly ITimingSession _timingSession;

        #region Constructors

        /// <summary>
        /// Initializes a <see cref="Profiler"/> class instance.
        /// </summary>
        /// <param name="name">The profiler name.</param>
        /// <param name="storage">The profiler storage.</param>
        /// <param name="tags">Tags of the profiler.</param>
        public Profiler(string name, IProfilingStorage storage, TagCollection tags)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }
            if (storage == null)
            {
                throw new ArgumentNullException("storage");
            }

            _storage = storage;
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            _timingSession = new TimingSession(this, name, tags) { Started = DateTime.UtcNow };
            var rootTiming = new ProfilingStep(this, "root", null);
            _timingSession.AddTiming(rootTiming);
        }

        #endregion

        #region IProfiler Members

        /// <summary>
        /// Gets the identity of the profiler.
        /// </summary>
        public Guid Id
        {
            get { return _timingSession.Id; }
        }

        /// <summary>
        /// Gets the elapsed time since the start of the <see cref="IProfiler"/>.
        /// </summary>
        public TimeSpan Elapsed
        {
            get { return _stopwatch.Elapsed; }
        }

        /// <summary>
        /// Whether or not the current profiler is stopped.
        /// </summary>
        public bool IsStopped
        {
            get { return !_stopwatch.IsRunning; }
        }

        /// <summary>
        /// Creates an <see cref="IProfilingStep"/> that will time the code between its creation and disposal.
        /// </summary>
        /// <param name="name">The name of the step.</param>
        /// <param name="tags">The tags of the step.</param>
        /// <returns>Returns the created <see cref="IProfilingStep"/>.</returns>
        public virtual IProfilingStep Step(string name, TagCollection tags)
        {
            return new ProfilingStep(this, name, tags);
        }

        /// <summary>
        /// Returns an <see cref="System.IDisposable"/> that will ignore the profiling between its creation and disposal.
        /// </summary>
        /// <returns>Returns the created <see cref="System.IDisposable"/> as the ignored step.</returns>
        public virtual IDisposable Ignore()
        {
            IProfilingStep ignoredStep = new ProfilingStep(this, "ignored step", null);
            ignoredStep.Discard();
            return ignoredStep;
        }

        /// <summary>
        /// Stops the current profiler.
        /// </summary>
        /// <param name="discardResults">
        /// When true, ignore the profiling results of the profiler.
        /// </param>
        public void Stop(bool discardResults)
        {
            if (IsStopped) return;

            _stopwatch.Stop();

            // stop the root step timing
            var rootStep = GetTimingSession().Timings.FirstOrDefault() as IProfilingStep;
            if (rootStep != null)
            {
                // the root step is added to profiling session on created,
                // so don't need to add it again on stopping
                rootStep.Stop(false);
            }

            // save result
            if (!discardResults)
            {
                var session = GetTimingSession();
                AddAggregationFields(session);
                _storage.SaveSession(session);
            }
        }

        /// <summary>
        /// Gets the timing session of the current profiler.
        /// </summary>
        /// <returns></returns>
        public ITimingSession GetTimingSession()
        {
            return _timingSession;
        }

        #endregion

        #region Private Methods

        private static void AddAggregationFields(ITimingSession session)
        {
            if (session.Timings == null || !session.Timings.Any()) return;

            var groups = session.Timings.GroupBy(timing => timing.Type);
            foreach (var group in groups)
            {
                if (string.Equals("step", group.Key)) continue;

                session.Data[group.Key + "Count"] = group.Count().ToString();
                session.Data[group.Key + "Duration"] = ((long)group.Sum(timing => timing.DurationMilliseconds)).ToString();
            }
        }

        #endregion
    }
}
