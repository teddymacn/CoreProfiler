using System;
using System.Collections.Generic;

namespace CoreProfiler.Timings
{
    /// <summary>
    /// Represents a generic timing.
    /// </summary>
    public interface ITiming
    {
        /// <summary>
        /// Gets or sets the type of the timing.
        /// </summary>
        string Type { get; set; }

        /// <summary>
        /// Gets or sets the identity of the timing.
        /// </summary>
        Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the identity of the parent timing.
        /// </summary>
        Guid? ParentId { get; set; }

        /// <summary>
        /// Gets or sets the name of the timing.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the UTC time of when the timing is started.
        /// </summary>
        DateTime Started { get; set; }

        /// <summary>
        /// Gets or sets the start milliseconds since the start of the profling session.
        /// </summary>
        long StartMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the duration milliseconds of the timing.
        /// </summary>
        long DurationMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the tags of the timing.
        /// </summary>
        TagCollection Tags { get; set; }

        /// <summary>
        /// Gets or sets the ticks of this timing for sorting.
        /// </summary>
        long Sort { get; set; }

        /// <summary>
        /// Gets or sets addtional data of the timing.
        /// </summary>
        Dictionary<string, string> Data { get; set; }
    }
}
