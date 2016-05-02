using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EF.Diagnostics.Profiling.Configuration;
using EF.Diagnostics.Profiling.Storages;
using EF.Diagnostics.Profiling.Timings;
using Microsoft.Extensions.Logging;

namespace EF.Diagnostics.Profiling.Storage.Json
{
    /// <summary>
    /// A <see cref="IProfilingStorage"/> implementation which persists profiling results as json via slf4net.
    /// </summary>
    public class JsonProfilingStorage : ProfilingStorageBase
    {
        private static readonly ILogger Logger = ConfigurationHelper.GetLogger<JsonProfilingStorage>();

        /// <summary>
        /// Data filed names which should be treated as integer fields.
        /// </summary>
        public static string[] IntegerDataFieldNames { get; set; }

        #region Constructors

        /// <summary>
        /// Initializes a <see cref="JsonProfilingStorage"/>.
        /// </summary>
        public JsonProfilingStorage()
        {
            IntegerDataFieldNames = new[] { "Count", "Size", "econds" };
        }

        #endregion

        #region JsonProfilingStorage Members

        /// <summary>
        /// Saves an <see cref="ITimingSession"/>.
        /// </summary>
        /// <param name="session"></param>
        protected override void Save(ITimingSession session)
        {
            if (!Logger.IsEnabled(LogLevel.Information))
            {
                return;
            }

            if (session == null)
            {
                return;
            }

            SaveSessionJson(session);

            if (session.Timings == null) return;

            foreach (var timing in session.Timings)
            {
                if (timing == null) continue;

                SaveTimingJson(session, timing);
            }
        }

        #endregion

        #region Protected Members

        /// <summary>
        /// Whether or not a field is an integer field.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected virtual bool IsIntFieldName(string key)
        {
            if (IntegerDataFieldNames == null || !IntegerDataFieldNames.Any()) return false;

            return IntegerDataFieldNames.Any(key.EndsWith);
        }

        #endregion

        #region Private Methods

        private void SaveSessionJson(ITimingSession session)
        {
            var sb = new StringBuilder();
            sb.Append("{");

            AppendSessionSharedFields(sb, session);
            AppendTimingFields(sb, session);

            sb.Append("}");

            Logger.LogInformation(sb.ToString());
        }

        private void SaveTimingJson(ITimingSession session, ITiming timing)
        {
            var sb = new StringBuilder();
            sb.Append("{");

            AppendSessionSharedFields(sb, session);
            AppendTimingFields(sb, timing);

            sb.Append("}");

            Logger.LogInformation(sb.ToString());
        }

        private static void AppendSessionSharedFields(StringBuilder sb, ITimingSession session)
        {
            AppendField(sb, "sessionId", session.Id.ToString("N"), null);
            AppendField(sb, "machine", session.MachineName);
        }

        private void AppendTimingFields(StringBuilder sb, ITiming timing)
        {
            AppendField(sb, "type", timing.Type);
            AppendField(sb, "id", timing.Id.ToString("N"));
            if (timing.ParentId.HasValue)
                AppendField(sb, "parentId", timing.ParentId.Value.ToString("N"));
            AppendField(sb, "name", timing.Name);
            AppendField(sb, "started", timing.Started);
            AppendField(sb, "start", timing.StartMilliseconds);
            AppendField(sb, "duration", timing.DurationMilliseconds);
            AppendField(sb, "tags", timing.Tags);
            AppendField(sb, "sort", timing.Sort);
            AppendDataFields(sb, timing.Data);
        }

        private static void EncodeAndAppendJsString(StringBuilder sb, string s)
        {
            foreach (var c in s)
            {
                switch (c)
                {
                    case '\'':
                        sb.Append("\\\'");
                        break;
                    case '\"':
                        sb.Append("\\\"");
                        break;
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        var i = (int)c;
                        if (i < 32 || i > 127)
                        {
                            sb.AppendFormat("\\u{0:X04}", i);
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
        }

        private void AppendDataFields(StringBuilder sb, Dictionary<string, string> data)
        {
            if (data == null) return;

            foreach (var keyValue in data)
            {
                if (keyValue.Value == null) continue;

                if (IsIntFieldName(keyValue.Key))
                {
                    AppendField(sb, keyValue.Key, long.Parse(keyValue.Value));
                }
                else
                {
                    AppendField(sb, keyValue.Key, keyValue.Value);
                }
            }
        }

        private static void AppendField(StringBuilder sb, string key, string value, string separator = ",")
        {
            if (separator != null)
                sb.Append(separator);

            sb.Append("\"");
            sb.Append(key);
            sb.Append("\":\"");
            EncodeAndAppendJsString(sb, value);
            sb.Append("\"");
        }

        private static void AppendField(StringBuilder sb, string key, long value, string separator = ",")
        {
            if (separator != null)
                sb.Append(separator);

            sb.Append("\"");
            sb.Append(key);
            sb.Append("\":");
            sb.Append(value);
        }

        private static void AppendField(StringBuilder sb, string key, DateTime value, string separator = ",")
        {
            if (separator != null)
                sb.Append(separator);

            sb.Append("\"");
            sb.Append(key);
            sb.Append("\":\"");
            sb.Append(value.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFZ")); //ISO8601
            sb.Append("\"");
        }

        private static void AppendField(StringBuilder sb, string key, TagCollection value, string separator = ",")
        {
            if (value == null || !value.Any()) return;

            if (separator != null)
                sb.Append(separator);

            sb.Append("\"");
            sb.Append(key);

            sb.Append("\":\"");
            var separator2 = "";
            foreach (var tag in value)
            {
                sb.Append(separator2);
                EncodeAndAppendJsString(sb, tag);

                separator2 = ",";
            }
            sb.Append("\"");
        }

        #endregion
    }
}
