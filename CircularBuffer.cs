using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CoreProfiler
{
    /// <summary>
    /// A ConcurrentQueue based simple circular buffer implementation.
    /// </summary>
    public sealed class CircularBuffer<T> : ICircularBuffer<T>
    {
        private readonly int _size;
        private readonly Func<T, bool> _shouldBeExcluded;
        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();

        #region Constructors

        /// <summary>
        /// Initializes a <see cref="CircularBuffer{T}"/>.
        /// </summary>
        /// <param name="size">The size of the circular buffer.</param>
        /// <param name="shouldBeExcluded">Whether or not, an item should not be saved in circular buffer.</param>
        public CircularBuffer(int size = 100, Func<T, bool> shouldBeExcluded = null)
        {
            _size = size;
            _shouldBeExcluded = shouldBeExcluded;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds an item to buffer.
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            if (_size <= 0) return;

            if (_shouldBeExcluded == null || !_shouldBeExcluded(item))
            {
                _queue.Enqueue(item);
                if (_queue.Count > _size)
                {
                    _queue.TryDequeue(out item);
                }
            }
        }

        #endregion

        #region IEnumerable<T> Members

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _queue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _queue.GetEnumerator();
        }

        #endregion
    }
}
