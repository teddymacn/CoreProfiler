using System.Collections.Generic;

namespace EF.Diagnostics.Profiling
{
    /// <summary>
    /// Represents a generic circular buffer.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ICircularBuffer<T> : IEnumerable<T>
    {
        /// <summary>
        /// Adds an item to the buffer.
        /// </summary>
        /// <param name="item"></param>
        void Add(T item);
    }
}
