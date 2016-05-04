using CoreProfiler.Timings;
using System;
using System.Collections.Generic;

namespace CoreProfiler
{
    /// <summary>
    /// Implements <see cref="CoreProfiler.IProfilingStep"/>.
    /// </summary>
    public class ProfilingStep : Timing, IProfilingStep
    {
        private readonly IProfiler _profiler;
        private bool _isDiscarded;
        private bool _isStopped;

        #region Constructors

        /// <summary>
        /// Initializes a <see cref="ProfilingStep"/>.
        /// </summary>
        /// <param name="profiler">
        ///     The <see cref="IProfiler"/> where
        ///     to add the <see cref="ProfilingStep"/> to when stops.
        /// </param>
        /// <param name="name">The name of the <see cref="ProfilingStep"/>.</param>
        /// <param name="tags">The tags of the step.</param>
        public ProfilingStep(IProfiler profiler, string name, TagCollection tags)
            : base(
                profiler
                , "step"
                , GetParentId(profiler)
                , name
            , tags)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            _profiler = profiler;
            StartMilliseconds = (long)_profiler.Elapsed.TotalMilliseconds;
            Sort = profiler.Elapsed.Ticks;
            ProfilingSession.ProfilingSessionContainer.CurrentSessionStepId = Id;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the timing of the profiling current step.
        /// </summary>
        /// <returns></returns>
        public ITiming GetStepTiming()
        {
            return this;
        }

        /// <summary>
        /// Discards the current profiling step.
        /// </summary>
        public void Discard()
        {
            _isDiscarded = true;
        }

        /// <summary>
        /// Add a tag to current profiling step.
        /// </summary>
        /// <param name="tag"></param>
        public void AddTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) return;

            if (Tags == null) Tags = new TagCollection();
            Tags.Add(tag);
        }

        /// <summary>
        /// Add a custom data field to current profiling step.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddField(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key)) return;

            if (Data == null) Data = new Dictionary<string, string>();
            Data[key] = value;
        }

        #endregion

        #region IProfilingStep Members

        void IProfilingStep.Discard()
        {
            Discard();
        }

        ITiming IProfilingStep.GetStepTiming()
        {
            return GetStepTiming();
        }

        void IProfilingStep.AddTag(string tag)
        {
            AddTag(tag);
        }

        void IProfilingStep.AddField(string key, string value)
        {
            AddField(key, value);
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Disposes the current instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizes the current <see cref="ProfilingStep"/>.
        /// </summary>
        ~ProfilingStep()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                // free managed resources

                // add step to profiling session
                Stop(true);
            }

            // free unmanaged resources
        }

        /// <summary>
        /// Stops the current <see cref="ProfilingStep"/> and adds the <see cref="ProfilingStep"/> to profiler.
        /// </summary>
        /// <param name="addToProfiler">
        ///     Whether or not add the current <see cref="ProfilingStep"/> to profiler when stops.
        /// </param>
        public void Stop(bool addToProfiler)
        {
            if (!_isDiscarded && !_isStopped)
            {
                DurationMilliseconds = (long)_profiler.Elapsed.TotalMilliseconds - StartMilliseconds;
                _isStopped = true;
                ProfilingSession.ProfilingSessionContainer.CurrentSessionStepId = ParentId;

                if (addToProfiler)
                {
                    _profiler.GetTimingSession().AddTiming(this);
                }
            }
        }

        #endregion

        #region Private Methods

        private static Guid? GetParentId(IProfiler profiler)
        {
            var parentStepId = ProfilingSession.ProfilingSessionContainer.CurrentSessionStepId;
            if (parentStepId.HasValue)
            {
                return parentStepId;
            }

            if (profiler != null)
            {
                return profiler.Id;
            }

            return null;
        }

        #endregion
    }
}
