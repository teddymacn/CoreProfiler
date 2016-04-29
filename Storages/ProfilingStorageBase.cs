using EF.Diagnostics.Profiling.Configuration;
using EF.Diagnostics.Profiling.Timings;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Threading;

namespace EF.Diagnostics.Profiling.Storages
{
    /// <summary>
    /// Asynchronous saving profiling timing sessions with a single-thread-queue worker. 
    /// The worker thread is automatically started when the first item is added.
    /// Override the Save() method for custom saving logic.
    /// 
    /// All methods and properties are thread safe.
    /// </summary>
    /// <remarks></remarks>
    public abstract class ProfilingStorageBase : IProfilingStorage
    {
        private static readonly ILogger Logger = ConfigurationHelper.GetLogger<ProfilingStorageBase>();

        private readonly ConcurrentQueue<ITimingSession> _sessionQueue = new ConcurrentQueue<ITimingSession>();
        private Thread _workerThread;
        private readonly AutoResetEvent _processWait = new AutoResetEvent(false);
        private readonly ManualResetEvent _entryWait = new ManualResetEvent(true);
        private const string OnQueueOverflowEventMessage = "ProfilingStorageBase worker queue overflowed";

        /// <summary>
        /// The infinite queue length.
        /// </summary>
        public const int Infinite = -1;

        /// <summary>
        /// Disables the queue, which means, each call to SaveResult saves the result immediately.
        /// </summary>
        public const int Inline = 0;

        /// <summary>
        /// The max length of the internal queue.
        /// Max queue length must be -1 (infinite), 0 (process inline) or a positive number.
        /// </summary>
        public int MaxQueueLength { get; set; }

        /// <summary>
        /// The time the worker thread sleeps.
        /// A long sleep period or infinite can cause the process to live longer than necessary.
        /// </summary>
        public int ThreadSleepMilliseconds { get; set; }

        #region  Constructors
        /// <summary>
        /// Constructs a new <see cref="ProfilingStorageBase"/>.
        /// </summary>
        protected ProfilingStorageBase()
        {
            MaxQueueLength = 10000;
            ThreadSleepMilliseconds = 100;
        }

        #endregion

        #region IProfilingStorage Members

        void IProfilingStorage.SaveSession(ITimingSession session)
        {
            SaveSession(session);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Saves a profiling timing session.
        /// </summary>
        /// <param name="session">The <see cref="ITimingSession"/> to be saved.</param>
        public void SaveSession(ITimingSession session)
        {
            if (MaxQueueLength == Inline)
            {
                Save(session);
            }
            else if (Count < MaxQueueLength || MaxQueueLength == Infinite)
            {
                Enqueue(session);
                InvokeThreadStart();
            }
            else
            {
                OnQueueOverflow(session);
            }
        }

        #endregion

        #region Protected Members

        /// <summary>
        /// Gets the number of items in the queue.
        /// </summary>
        protected int Count
        {
            get
            {
                return _sessionQueue.Count;
            }
        }

        /// <summary>
        /// Saves an <see cref="ITimingSession"/>.
        /// </summary>
        /// <param name="session"></param>
        protected abstract void Save(ITimingSession session);

        /// <summary>
        /// Enqueues a session to internal queue.
        /// </summary>
        /// <param name="session">The <see cref="ITimingSession"/> to be enqueued.</param>
        protected void Enqueue(ITimingSession session)
        {
            _sessionQueue.Enqueue(session);
        }

        /// <summary>
        /// Tries to dequeue a session from internal queue for processing.
        /// </summary>
        /// <param name="session">The <see cref="ITimingSession"/> to be dequeued.</param>
        /// <returns>Returns the dequeued <see cref="ITimingSession"/>.</returns>
        protected bool TryDequeue(out ITimingSession session)
        {
            return _sessionQueue.TryDequeue(out session);
        }

        /// <summary>
        /// What to do on internal queue overflow.
        /// 
        /// By default, it will delay the enqueue of session for at most 5000ms and log exception.
        /// </summary>
        /// <param name="session">The <see cref="ITimingSession"/> being enqueued when internal queue overflow.</param>
        protected virtual void OnQueueOverflow(ITimingSession session)
        {
            // On overflow, never block the main thread running,
            // simply throw away the item at the top of the queue, enqueue the new item and log the event
            // so basically, the queue works like a ring buffer
            ITimingSession temp;
            TryDequeue(out temp);
            Enqueue(session);

            Logger.LogError(OnQueueOverflowEventMessage);
        }

        #endregion

        #region Private Methods

        private void InvokeThreadStart()
        {
            lock (_sessionQueue)
            {
                // Kick off thread if not there
                if (_workerThread == null)
                {
                    _workerThread = new Thread(SaveQueuedSessions) { Name = GetType().Name };
                    _workerThread.Start();
                }

                // Signal process to continue
                _processWait.Set();
            }
        }

        private void SaveQueuedSessions()
        {
            do
            {
                // Suspend for a while
                _processWait.WaitOne(ThreadSleepMilliseconds);

                // Upgrade to foreground thread
                Thread.CurrentThread.IsBackground = false;

                // set null the current profiling session bound to the running thread to release the memory
                ProfilingSession.SetCurrentProfilingSession(null);

                // Save all the queued sessions
                ITimingSession session;
                while (TryDequeue(out session))
                {
                    Save(session);

                    // Signal waiting threads to continue
                    _entryWait.Set();
                }

                // Downgrade to background thread while waiting
                Thread.CurrentThread.IsBackground = true;

            } while (true);
        }

        #endregion
    }
}
