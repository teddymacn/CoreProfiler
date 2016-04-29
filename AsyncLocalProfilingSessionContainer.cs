using System;
using System.Threading;

namespace EF.Diagnostics.Profiling
{
    /// <summary>
    /// The default AsyncLocal based <see cref="IProfilingSessionContainer"/> implementation.
    /// </summary>
    public class AsyncLocalProfilingSessionContainer : IProfilingSessionContainer
    {
        private static readonly AsyncLocal<Guid?> _profilingStepId = new AsyncLocal<Guid?>();
        private static readonly AsyncLocal<ProfilingSession> _profilingSession = new AsyncLocal<ProfilingSession>();

        #region Public Members

        /// <summary>
        /// Gets or sets the current ProfilingSession.
        /// </summary>
        public ProfilingSession CurrentSession
        {
            get
            {
                return _profilingSession.Value;
            }
            set
            {
                _profilingSession.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the current profiling step id.
        /// </summary>
        public Guid? CurrentSessionStepId
        {
            get { return _profilingStepId.Value; }
            set { _profilingStepId.Value = value; }
        }

        /// <summary>
        /// Clears the current profiling session &amp; step id.
        /// </summary>
        public void Clear()
        {
            _profilingSession.Value = null;
            _profilingStepId.Value = null;
        }

        #endregion

        #region ICurrentProfilingSessionContainer Members

            ProfilingSession IProfilingSessionContainer.CurrentSession
        {
            get { return CurrentSession; }
            set { CurrentSession = value; }
        }

        Guid? IProfilingSessionContainer.CurrentSessionStepId
        {
            get { return CurrentSessionStepId; }
            set { CurrentSessionStepId = value; }
        }

        void IProfilingSessionContainer.Clear()
        {
            Clear();
        }

        #endregion
    }
}
