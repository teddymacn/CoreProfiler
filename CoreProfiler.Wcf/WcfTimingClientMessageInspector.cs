using System.Globalization;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace CoreProfiler.Wcf
{
    public sealed class WcfTimingClientMessageInspector : IClientMessageInspector
    {
        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            var wcfTiming = correlationState as WcfTiming;
            if (wcfTiming == null)
            {
                return;
            }

            var profilingSession = GetCurrentProfilingSession();
            if (profilingSession == null)
            {
                return;
            }

            // set the start output milliseconds as when we start reading the reply message
            wcfTiming.Data["outputStartMilliseconds"] = ((long)profilingSession.Profiler.Elapsed.TotalMilliseconds).ToString(CultureInfo.InvariantCulture);

            if (reply != null)
            {
                // only if using HTTP binding, try to get content-length header value (if exists) as output size
                if (reply.Properties.ContainsKey(HttpResponseMessageProperty.Name))
                {
                    var property = (HttpResponseMessageProperty)reply.Properties[HttpResponseMessageProperty.Name];
                    int contentLength;
                    if (int.TryParse(property.Headers[HttpResponseHeader.ContentLength], out contentLength) && contentLength > 0)
                    {
                        wcfTiming.Data["responseSize"] = contentLength.ToString(CultureInfo.InvariantCulture);
                    }
                }
            }
            wcfTiming.Stop();
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            var profilingSession = GetCurrentProfilingSession();
            if (profilingSession == null)
            {
                return null;
            }

            var wcfTiming = new WcfTiming(profilingSession.Profiler, ref request);
            wcfTiming.Data["remoteAddress"] = channel.RemoteAddress.ToString();

            // add correlationId as a header of sub wcf call
            // so that we could drill down to the wcf profiling session from current profiling session
            if (!Equals(request.Headers.MessageVersion, MessageVersion.None))
            {
                var untypedHeader = new MessageHeader<string>(wcfTiming.CorrelationId).GetUntypedHeader(
                    Constants.XCorrelationId, Constants.WcfHeaderNamespace);
                request.Headers.Add(untypedHeader);
            }

            // return wcfTiming as correlationState of AfterReceiveReply() to stop the WCF timing in AfterReceiveReply()
            return wcfTiming;
        }

        #region Private Methods

        private static ProfilingSession GetCurrentProfilingSession()
        {
            var profilingSession = ProfilingSession.Current;
            if (profilingSession == null)
            {
                return null;
            }

            if (profilingSession.Profiler.IsStopped)
            {
                ProfilingSession.ProfilingSessionContainer.Clear();
                profilingSession = null;
            }

            return profilingSession;
        }

        #endregion
    }
}
