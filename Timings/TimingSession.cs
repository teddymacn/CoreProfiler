using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace EF.Diagnostics.Profiling.Timings
{
    /// <summary>
    /// The default timing session class.
    /// </summary>
    public sealed class TimingSession : Timing, ITimingSession
    {
        private readonly IProfiler _profiler;

        private const string Session = "session";
        private ConcurrentQueue<ITiming> _timings;

        #region ITimingSession Members

        /// <summary>
        /// Gets the machine name.
        /// </summary>
        public string MachineName { get; set; }

        /// <summary>
        /// Gets or sets the timings.
        /// </summary>
        public IEnumerable<ITiming> Timings
        {
            get { return _timings ?? (_timings = new ConcurrentQueue<ITiming>()); }
            set
            {
                _timings = null;

                if (value == null) return;

                var timings = new ConcurrentQueue<ITiming>(value);
                if (timings.Count == 0) return;

                _timings = timings;
            }
        }

        /// <summary>
        /// Adds a timing to the session.
        /// </summary>
        /// <param name="timing"></param>
        public void AddTiming(ITiming timing)
        {
            if (timing == null) throw new ArgumentNullException("timing");

            _timings.Enqueue(timing);
        }

        #endregion

        #region ITiming Members

        /// <summary>
        /// Gets or sets the duration milliseconds of the timing.
        /// </summary>
        public override long DurationMilliseconds
        {
            get
            {
                if (_profiler != null) return (long)_profiler.Elapsed.TotalMilliseconds;

                return base.DurationMilliseconds;
            }
            set { base.DurationMilliseconds = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a <see cref="TimingSession"/>.
        /// </summary>
        /// <param name="profiler"></param>
        /// <param name="name"></param>
        /// <param name="tags"></param>
        public TimingSession(IProfiler profiler, string name, TagCollection tags)
            : base(profiler, Session, null, name, tags)
        {
            _profiler = profiler;
            _timings = new ConcurrentQueue<ITiming>();
            Data = new Dictionary<string, string>();

            //TODO: use Environment.MachineName instead when it is implemented
            MachineName = Environment.GetEnvironmentVariable("COMPUTERNAME") ?? "Unknown";
        }

        /// <summary>
        /// Initializes a <see cref="TimingSession"/>.
        /// </summary>
        public TimingSession()
        {
        }

        #endregion
    }
}
