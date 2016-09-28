using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CoreProfiler.Timings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;

namespace CoreProfiler.Web
{
    public class CoreProfilerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public const string XCorrelationId = "X-ET-Correlation-Id";

        private const string ViewUrl = "/coreprofiler/view";
        private const string ViewUrlNano = "/nanoprofiler/view";
        private const string Import = "import";
        private const string Export = "?export";
        private const string CorrelationId = "correlationId";

        /// <summary>
        /// The default Html of the view-result index page: ~/coreprofiler/view
        /// </summary>
        public static string ViewResultIndexHeaderHtml = "<h1>CoreProfiler Latest Profiling Results</h1>";

        /// <summary>
        /// The default Html of the view-result page: ~/coreprofiler/view/{uuid}
        /// </summary>
        public static string ViewResultHeaderHtml = "<h1>CoreProfiler Profiling Result</h1>";

        /// <summary>
        /// Tries to import drilldown result by remote address of the step
        /// </summary>
        public static bool TryToImportDrillDownResult;

        /// <summary>
        /// The handler to search for child profiling session by correlationId.
        /// </summary>
        public static Func<string, Guid?> DrillDownHandler { get; set; }

        /// <summary>
        /// The handler to search for parent profiling session by correlationId.
        /// </summary>
        public static Func<string, Guid?> DrillUpHandler { get; set; }

        public CoreProfilerMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<CoreProfilerMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            // disable view profiling if CircularBuffer is not enabled
            if (ProfilingSession.CircularBuffer == null)
            {
                await _next.Invoke(context);
                return;
            }
            
            ClearIfCurrentProfilingSessionStopped();
            
            var url = UriHelper.GetDisplayUrl(context.Request);
            ProfilingSession.Start(url);
            
            // set correlationId if exists in header
            var correlationId = GetCorrelationIdFromHeaders(context);
            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                ProfilingSession.Current.AddField("correlationId", correlationId);
            }
            
            // only supports GET method for view results
            if (context.Request.Method != "GET")
            {
                await _next.Invoke(context);
                return;
            }
            
            var path = context.Request.Path.ToString().TrimEnd('/');
            if (path.EndsWith("/coreprofiler-resources/icons"))
            {
                context.Response.ContentType = "image/png";
                var iconsStream = GetType().GetTypeInfo().Assembly.GetManifestResourceStream("Web.icons.png");
                using (var br = new BinaryReader(iconsStream))
                {
                    await context.Response.Body.WriteAsync(br.ReadBytes((int)iconsStream.Length), 0, (int)iconsStream.Length);
                }
                return;
            }
            
            if (path.EndsWith("/coreprofiler-resources/css"))
            {
                context.Response.ContentType = "text/css";
                var cssStream = GetType().GetTypeInfo().Assembly.GetManifestResourceStream("Web.treeview_timeline.css");
                using (var sr = new StreamReader(cssStream))
                {
                    await context.Response.WriteAsync(sr.ReadToEnd());
                }
                return;
            }
            
            // view index of all latest results: ~/coreprofiler/view
            if (path.EndsWith(ViewUrl, StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(ViewUrlNano, StringComparison.OrdinalIgnoreCase))
            {
                // try to handle import/export first
                var import = context.Request.Query[Import];
                if (Uri.IsWellFormedUriString(import, UriKind.Absolute))
                {
                    await ImportSessionsFromUrl(import);
                    return;
                }

                if (context.Request.QueryString.ToString() == Export)
                {
                    context.Response.ContentType = "application/json";

                    await context.Response.WriteAsync(ImportSerializer.SerializeSessions(ProfilingSession.CircularBuffer));
                    return;
                }

                var exportCorrelationId = context.Request.Query[CorrelationId];
                if (!string.IsNullOrEmpty(exportCorrelationId))
                {
                    context.Response.ContentType = "application/json";
                    var result = ProfilingSession.CircularBuffer.FirstOrDefault(
                            r => r.Data != null && r.Data.ContainsKey(CorrelationId) && r.Data[CorrelationId] == exportCorrelationId);
                    if (result != null)
                    {
                        await context.Response.WriteAsync(ImportSerializer.SerializeSessions(new[] { result }));
                        return;
                    }
                }

                // render result list view
                context.Response.ContentType = "text/html";

                var sb = new StringBuilder();
                sb.Append("<head>");
                sb.Append("<title>CoreProfiler Latest Profiling Results</title>");
                sb.Append("<style>th { width: 200px; text-align: left; } .gray { background-color: #eee; } .nowrap { white-space: nowrap;padding-right: 20px; vertical-align:top; } </style>");
                sb.Append("</head");
                sb.Append("<body>");
                sb.Append(ViewResultIndexHeaderHtml);
                
                sb.Append("<table>");
                sb.Append("<tr><th class=\"nowrap\">Time (UTC)</th><th class=\"nowrap\">Duration (ms)</th><th>Url</th></tr>");
                var latestResults = ProfilingSession.CircularBuffer.OrderByDescending(r => r.Started);
                var i = 0;
                foreach (var result in latestResults)
                {
                    sb.Append("<tr");
                    if ((i++) % 2 == 1)
                    {
                        sb.Append(" class=\"gray\"");
                    }
                    sb.Append("><td class=\"nowrap\">");
                    sb.Append(result.Started.ToString("yyyy-MM-ddTHH:mm:ss.FFF"));
                    sb.Append("</td><td class=\"nowrap\">");
                    sb.Append(result.DurationMilliseconds);
                    sb.Append("</td><td><a href=\"view/");
                    sb.Append(result.Id.ToString());
                    sb.Append("\" target=\"_blank\">");
                    sb.Append(result.Name.Replace("\r\n", " "));
                    sb.Append("</a></td></tr>");
                }
                sb.Append("</table>");

                sb.Append("</body>");

                await context.Response.WriteAsync(sb.ToString());
                return;
            }
            
            // view specific result by uuid: ~/coreprofiler/view/{uuid}
            if (path.IndexOf(ViewUrl, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                context.Response.ContentType = "text/html";

                var sb = new StringBuilder();
                sb.Append("<head>");
                sb.Append("<meta charset=\"utf-8\" />");
                sb.Append("<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\" />");
                sb.Append("<title>CoreProfiler Profiling Result</title>");
                sb.Append("<link rel=\"stylesheet\" href=\"./coreprofiler-resources/css\" />");
                sb.Append("</head");
                sb.Append("<body>");
                sb.Append("<h1>CoreProfiler Profiling Result</h1>");

                var uuid = path.Split('/').Last();
                var result = ProfilingSession.CircularBuffer.FirstOrDefault(
                        r => r.Id.ToString().ToLowerInvariant() == uuid.ToLowerInvariant());
                if (result != null)
                {
                    if (TryToImportDrillDownResult)
                    {
                        // try to import drill down results
                        foreach (var timing in result.Timings)
                        {
                            if (timing.Data == null || !timing.Data.ContainsKey(CorrelationId)) continue;
                            Guid parentResultId;
                            if (!Guid.TryParse(timing.Data[CorrelationId], out parentResultId)
                                || ProfilingSession.CircularBuffer.Any(r => r.Id == parentResultId)) continue;

                            string remoteAddress;
                            if (!timing.Data.TryGetValue("remoteAddress", out remoteAddress))
                                remoteAddress = timing.Name;

                            if (!Uri.IsWellFormedUriString(remoteAddress, UriKind.Absolute)) continue;

                            if (!remoteAddress.StartsWith("http", StringComparison.OrdinalIgnoreCase)) continue;

                            var pos = remoteAddress.IndexOf("?");
                            if (pos > 0) remoteAddress = remoteAddress.Substring(0, pos);
                            if (remoteAddress.Split('/').Last().Contains(".")) remoteAddress = remoteAddress.Substring(0, remoteAddress.LastIndexOf("/"));

                            try
                            {
                                await ImportSessionsFromUrl(remoteAddress + "/coreprofiler/view?" + CorrelationId + "=" + parentResultId.ToString("N"));
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.Write(ex.Message);

                                //ignore exceptions
                            }
                        }
                    }

                    // render result tree
                    sb.Append("<div class=\"css-treeview\">");

                    // print summary
                    sb.Append("<ul>");
                    sb.Append("<li class=\"summary\">");
                    PrintDrillUpLink(sb, result);
                    sb.Append(result.Name.Replace("\r\n", " "));
                    sb.Append("</li>");
                    sb.Append("<li class=\"summary\">");
                    if (result.Data != null)
                    {
                        foreach (var keyValue in result.Data)
                        {
                            if (string.IsNullOrWhiteSpace(keyValue.Value)) continue;

                            sb.Append("<b>");
                            sb.Append(keyValue.Key);
                            sb.Append(": </b>");
                            var encodedValue = WebUtility.HtmlEncode(keyValue.Value);
                            if (keyValue.Key.EndsWith("Count") || keyValue.Key.EndsWith("Duration"))
                            {
                                sb.Append("<span class=\"");
                                sb.Append(keyValue.Key);
                                sb.Append("\">");
                                sb.Append(encodedValue);
                                sb.Append("</span>");
                            }
                            else
                            {
                                sb.Append(encodedValue);
                            }
                            sb.Append(" &nbsp; ");
                        }
                    }
                    sb.Append("<b>machine: </b>");
                    sb.Append(result.MachineName);
                    sb.Append(" &nbsp; ");
                    if (result.Tags != null && result.Tags.Any())
                    {
                        sb.Append("<b>tags: </b>");
                        sb.Append(string.Join(", ", result.Tags));
                        sb.Append(" &nbsp; ");
                    }
                    sb.Append("</li>");
                    sb.Append("</ul>");

                    var totalLength = result.DurationMilliseconds;
                    if (totalLength == 0)
                    {
                        totalLength = 1;
                    }
                    var factor = 300.0/totalLength;

                    // print ruler
                    sb.Append("<ul>");
                    sb.Append("<li class=\"ruler\"><span style=\"width:300px\">0</span><span style=\"width:80px\">");
                    sb.Append(totalLength);
                    sb.Append(
                        " (ms)</span><span style=\"width:20px\">&nbsp;</span><span style=\"width:60px\">Start</span><span style=\"width:60px\">Duration</span><span style=\"width:20px\">&nbsp;</span><span>Timing Hierarchy</span></li>");
                    sb.Append("</ul>");

                    // print timings
                    sb.Append("<ul class=\"timing\">");
                    PrintTimings(result, result.Id, sb, factor);
                    sb.Append("");
                    sb.Append("</ul>");
                    sb.Append("</div>");

                    // print timing data popups
                    foreach (var timing in result.Timings)
                    {
                        if (timing.Data == null || !timing.Data.Any()) continue;

                        sb.Append("<aside id=\"data_");
                        sb.Append(timing.Id.ToString());
                        sb.Append("\" style=\"display:none\" class=\"modal\">");
                        sb.Append("<div>");
                        sb.Append("<h4><code>");
                        sb.Append(timing.Name.Replace("\r\n", " "));
                        sb.Append("</code></h4>");
                        sb.Append("<textarea>");
                        foreach (var keyValue in timing.Data)
                        {
                            if (string.IsNullOrWhiteSpace(keyValue.Value)) continue;

                            sb.Append(keyValue.Key);
                            sb.Append(":\r\n");
                            var value = keyValue.Value.Trim();

                            if (value.StartsWith("<"))
                            {
                                // asuume it is XML
                                // try to format XML with indent
                                var doc = new XmlDocument();
                                try
                                {
                                    doc.LoadXml(value);
                                    var ms = new MemoryStream();
                                    var xwSettings = new XmlWriterSettings
                                    {
                                        Encoding = new UTF8Encoding(false),
                                        Indent = true,
                                        IndentChars = "\t"
                                    };
                                    using (var writer = XmlWriter.Create(ms, xwSettings))
                                    {
                                        doc.Save(writer);
                                        ms.Seek(0, SeekOrigin.Begin);
                                        using (var sr = new StreamReader(ms))
                                        {
                                            value = sr.ReadToEnd();
                                        }
                                    }
                                }
                                catch
                                {
                                    //squash exception
                                }
                            }
                            sb.Append(value);
                            sb.Append("\r\n\r\n");
                        }
                        if (timing.Tags != null && timing.Tags.Any())
                        {
                            sb.Append("tags:\r\n");
                            sb.Append(timing.Tags);
                            sb.Append("\r\n");
                        }
                        sb.Append("</textarea>");
                        sb.Append(
                            "<a href=\"#close\" title=\"Close\" onclick=\"this.parentNode.parentNode.style.display='none'\">Close</a>");
                        sb.Append("</div>");
                        sb.Append("</aside>");
                    }
                }
                else
                {
                    sb.Append("Specified result does not exist!");
                }
                sb.Append("</body>");

                await context.Response.WriteAsync(sb.ToString());
                return;
            }
            
            try
            {
                await _next.Invoke(context);
            }
            catch (System.Exception)
            {
                // stop and save profiling results on error
                using (ProfilingSession.Current.Step("Stop on Error")) { }
                
                throw;
            }
            finally{
                ProfilingSession.Stop();
            }
        }
        
        #region Private Methods
        
        private void PrintTimings(ITimingSession session, Guid parentId, StringBuilder sb, double factor)
        {
            var timings = session.Timings.Where(s => s.ParentId == parentId);
            foreach (var timing in timings)
            {
                PrintTiming(session, timing, sb, factor);
            }
        }

        private void PrintTiming(ITimingSession session, ITiming timing, StringBuilder sb, double factor)
        {
            sb.Append("<li><span class=\"timing\" style=\"padding-left: ");
            var start = Math.Floor(timing.StartMilliseconds*factor);
            if (start > 300)
            {
                start = 300;
            }
            sb.Append(start);
            sb.Append("px\"><span class=\"bar ");
            sb.Append(timing.Type);
            sb.Append("\" title=\"");
            sb.Append(WebUtility.HtmlEncode(timing.Name.Replace("\r\n", " ")));
            sb.Append("\" style=\"width: ");
            var width = (int)Math.Round(timing.DurationMilliseconds*factor);
            if (width > 300)
            {
                width = 300;
            }
            else if (width == 0)
            {
                width = 1;
            }
            sb.Append(width);
            sb.Append("px\"></span><span class=\"start\">+");
            sb.Append(timing.StartMilliseconds);
            sb.Append("</span><span class=\"duration\">");
            sb.Append(timing.DurationMilliseconds);
            sb.Append("</span></span>");
            var hasChildTimings = session.Timings.Any(s => s.ParentId == timing.Id);
            if (hasChildTimings)
            {
                sb.Append("<input type=\"checkbox\" id=\"t_");
                sb.Append(timing.Id.ToString());
                sb.Append("\" checked=\"checked\" /><label for=\"t_");
                sb.Append(timing.Id.ToString());
                sb.Append("\">");
                PrintDataLink(sb, timing);
                PrintDrillDownLink(sb, timing);
                sb.Append(WebUtility.HtmlEncode(timing.Name.Replace("\r\n", " ")));
                sb.Append("</label>");
                sb.Append("<ul>");
                PrintTimings(session, timing.Id, sb, factor);
                sb.Append("</ul>");
            }
            else
            {
                sb.Append("<span class=\"leaf\">");
                PrintDataLink(sb, timing);
                PrintDrillDownLink(sb, timing);
                sb.Append(WebUtility.HtmlEncode(timing.Name.Replace("\r\n", " ")));
                sb.Append("</span>");
            }
            sb.Append("</li>");
        }

        private void PrintDataLink(StringBuilder sb, ITiming timing)
        {
            if (timing.Data == null || !timing.Data.Any()) return;

            sb.Append("[<a href=\"#data_");
            sb.Append(timing.Id.ToString());
            sb.Append("\" onclick=\"document.getElementById('data_");
            sb.Append(timing.Id.ToString());
            sb.Append("').style.display='block';\" class=\"openModal\">data</a>] ");
        }

        private void PrintDrillDownLink(StringBuilder sb, ITiming timing)
        {
            if (timing.Data == null || !timing.Data.ContainsKey("correlationId")) return;

            var correlationId = timing.Data["correlationId"];

            Guid? drillDownSessionId = null;
            if (DrillDownHandler == null)
            {
                var drillDownSession = ProfilingSession.CircularBuffer.FirstOrDefault(s => s.Data != null && s.Data.ContainsKey("correlationId") && s.Data["correlationId"] == correlationId);
                if (drillDownSession != null) drillDownSessionId = drillDownSession.Id;
            }
            else
            {
                drillDownSessionId = DrillDownHandler(correlationId);
            }

            if (!drillDownSessionId.HasValue) return;

            sb.Append("[<a href=\"./");
            sb.Append(drillDownSessionId);
            sb.Append("\">drill down</a>] ");
        }

        private void PrintDrillUpLink(StringBuilder sb, ITimingSession session)
        {
            if (session.Data == null || !session.Data.ContainsKey("correlationId")) return;

            var correlationId = session.Data["correlationId"];

            Guid? drillUpSessionId = null;
            if (DrillUpHandler == null)
            {
                var drillUpSession = ProfilingSession.CircularBuffer.FirstOrDefault(s => s.Timings != null && s.Timings.Any(t => t.Data != null && t.Data.ContainsKey("correlationId") && t.Data["correlationId"] == correlationId));
                if (drillUpSession != null) drillUpSessionId = drillUpSession.Id;
            }
            else
            {
                drillUpSessionId = DrillUpHandler(correlationId);
            }

            if (!drillUpSessionId.HasValue) return;

            sb.Append("[<a href=\"./");
            sb.Append(drillUpSessionId);
            sb.Append("\">drill up</a>] ");
        }
        
        private static void ClearIfCurrentProfilingSessionStopped()
        {
            var profilingSession = ProfilingSession.Current;
            if (profilingSession == null)
            {
                return;
            }

            if (profilingSession.Profiler.IsStopped)
            {
                ProfilingSession.ProfilingSessionContainer.Clear();
            }
        }

        private string GetCorrelationIdFromHeaders(HttpContext context)
        {
            if (context.Request.Headers.Keys.Contains(XCorrelationId))
            {
                var correlationIds = context.Request.Headers.GetCommaSeparatedValues(XCorrelationId);
                if (correlationIds != null)
                {
                    return correlationIds.FirstOrDefault();
                }
            }

            return null;
        }

        private async Task ImportSessionsFromUrl(string importUrl)
        {
            IEnumerable<ITimingSession> sessions = null;

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(importUrl);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    sessions = ImportSerializer.DeserializeSessions(content);
                }
            }

            if (sessions == null)
            {
                return;
            }

            if (ProfilingSession.CircularBuffer == null)
            {
                return;
            }

            var existingIds = ProfilingSession.CircularBuffer.Select(session => session.Id).ToList();
            foreach (var session in sessions)
            {
                if (!existingIds.Contains(session.Id))
                {
                    ProfilingSession.CircularBuffer.Add(session);
                }
            }
        }

        #endregion
    }
}