using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace EF.Diagnostics.Profiling.Timings
{
    /// <summary>
    /// The default timing class.
    /// </summary>
    public class Timing : ITiming
    {
        private readonly IProfiler _profiler;
        private DateTime? _started;

        #region Properties

        /// <summary>
        /// Gets the type of timing.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets the Identity.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets the identity of the parent timing.
        /// </summary>
        public Guid? ParentId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the UTC time of when the timing is started.
        /// </summary>
        public virtual DateTime Started
        {
            get
            {
                if (_started.HasValue)
                {
                    return _started.Value;
                }

                if (_profiler == null) return default(DateTime);

                var timingSession = _profiler.GetTimingSession();
                if (timingSession == null) return default(DateTime);

                return _profiler.GetTimingSession().Started.AddMilliseconds(StartMilliseconds);
            }
            set { _started = value; }
        }

        /// <summary>
        /// Gets or sets the start milliseconds since the start of the profling session.
        /// </summary>
        public long StartMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the duration milliseconds of the current timing.
        /// </summary>
        public virtual long DurationMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the tags of this timing.
        /// </summary>
        public TagCollection Tags { get; set; }

        /// <summary>
        /// Gets or sets the ticks of this timing for sorting.
        /// </summary>
        public long Sort { get; set; }

        /// <summary>
        /// Gets or sets addtional data of the timing.
        /// </summary>
        public Dictionary<string, string> Data { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a <see cref="Timing"/>
        /// </summary>
        /// <param name="profiler">The <see cref="IProfiler"/>.</param>
        /// <param name="type">The type of timing.</param>
        /// <param name="parentId">The identity of the parent timing.</param>
        /// <param name="name">The name of the timing.</param>
        /// <param name="tags">The tags of the timing.</param>
        public Timing(IProfiler profiler, string type, Guid? parentId, string name, TagCollection tags)
        {
            _profiler = profiler;
            Type = type;
            ParentId = parentId;
            Name = name;
            Tags = tags;

            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Initializes a <see cref="Timing"/>
        /// </summary>
        public Timing()
        {
        }

        #endregion
    }
}
