using CoreProfiler.Timings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.ServiceModel.Channels;

namespace CoreProfiler.Wcf
{
    /// <summary>
    /// Represents the WCF timing of a WCF service call.
    /// </summary>
    public class WcfTiming : Timing
    {
        private readonly IProfiler _profiler;
        private const string WcfTimingType = "wcf";
        private const string CorrelationIdKey = "correlationId";

        /// <summary>
        /// Gets the correlationId of a web timing.
        /// </summary>
        public string CorrelationId
        {
            get { return Data[CorrelationIdKey]; }
        }

        #region Constructors

        /// <summary>
        /// Initializes a new WCF timing.
        /// </summary>
        /// <param name="profiler">
        ///     The <see cref="IProfiler"/> where
        ///     to add the timing to when stops.
        /// </param>
        /// <param name="requestMessage">
        ///     The request message of the WCF service method being called &amp; profiled.
        /// </param>
        public WcfTiming(IProfiler profiler, ref Message requestMessage)
            : base(profiler, WcfTimingType, ProfilingSession.ProfilingSessionContainer.CurrentSessionStepId, requestMessage.Headers.Action, null)
        {
            if (requestMessage == null)
            {
                throw new ArgumentNullException("requestMessage");
            }

            _profiler = profiler;
            StartMilliseconds = (long)_profiler.Elapsed.TotalMilliseconds;
            Sort = profiler.Elapsed.Ticks;
            Data = new Dictionary<string, string>();
            Data[CorrelationIdKey] = Guid.NewGuid().ToString("N");
            var requestMessageContent = ToXml(ref requestMessage);
            Data["requestMessage"] = requestMessageContent;
            Data["requestSize"] = requestMessageContent.Length.ToString(CultureInfo.InvariantCulture);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Stops the current WCF timing.
        /// </summary>
        public void Stop()
        {
            DurationMilliseconds = (long)_profiler.Elapsed.TotalMilliseconds - StartMilliseconds;

            _profiler.GetTimingSession().AddTiming(this);
        }

        #endregion

        #region Private Methods

        private static string ToXml(ref Message message)
        {
            if (message == null)
            {
                return null;
            }

            using (var buffer = message.CreateBufferedCopy(int.MaxValue))
            {
                message = buffer.CreateMessage();

                using (var messageCopy = buffer.CreateMessage())
                using (var reader = messageCopy.GetReaderAtBodyContents())
                {
                    return reader.ReadOuterXml();
                }
            }
        }

        #endregion
    }
}
