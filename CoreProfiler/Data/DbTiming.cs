using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Text;
using CoreProfiler.Timings;

namespace CoreProfiler.Data
{
    /// <summary>
    /// Represents a DB timing of a <see cref="DbCommand"/> execution.
    /// </summary>
    public sealed class DbTiming : Timing
    {
        private readonly IProfiler _profiler;

        #region Constructors

        /// <summary>
        /// Initializes a <see cref="DbTiming"/>.
        /// </summary>
        /// <param name="profiler">
        ///     The <see cref="IProfiler"/> where
        ///     to add the timing to when stops.
        /// </param>
        /// <param name="executeType">The <see cref="DbExecuteType"/> of the <see cref="DbCommand"/> being executed &amp; profiled.</param>
        /// <param name="command">The <see cref="DbCommand"/> being executed &amp; profiled.</param>
        public DbTiming(
            IProfiler profiler, DbExecuteType executeType, DbCommand command)
            : base(profiler, "db", ProfilingSession.ProfilingSessionContainer.CurrentSessionStepId, command == null ? null : command.CommandText, null)
        {
            if (profiler == null) throw new ArgumentNullException("profiler");
            if (command == null) throw new ArgumentNullException("command");

            _profiler = profiler;
            StartMilliseconds = (long)_profiler.Elapsed.TotalMilliseconds;
            Sort = profiler.Elapsed.Ticks;
            Data = new Dictionary<string, string>();

            Data["executeType"] = executeType.ToString().ToLowerInvariant();

            if (command.Parameters == null || command.Parameters.Count == 0) return;

            Data["parameters"] = SerializeParameters(command.Parameters);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Calculates the first fetch milliseconds of this DB operation.
        /// </summary>
        public void FirstFetch()
        {
            Data["readStart"] = ((long)_profiler.Elapsed.TotalMilliseconds - StartMilliseconds).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Stops the current DB timing.
        /// </summary>
        public void Stop()
        {
            DurationMilliseconds = (long)_profiler.Elapsed.TotalMilliseconds - StartMilliseconds;
            if (!Data.ContainsKey("readStart"))
            {
                Data["readStart"] = DurationMilliseconds.ToString(CultureInfo.InvariantCulture);
            }

            _profiler.GetTimingSession().AddTiming(this);
        }

        #endregion

        #region Private Methods

        private static string SerializeParameters(DbParameterCollection parameters)
        {
            var sb = new StringBuilder();

            foreach (DbParameter parameter in parameters)
            {
                sb.Append(parameter.ParameterName);
                sb.Append("(");
                sb.Append(parameter.DbType);
                sb.Append(", ");
                sb.Append(parameter.Direction);
                if (parameter.IsNullable)
                {
                    sb.Append(", nullable");
                }
                sb.Append("): ");
                sb.Append(parameter.Value == null || parameter.Value == DBNull.Value ? "NULL" : SerializeParameterValue(parameter.Value));
                sb.Append("\r\n");
            }

            return sb.ToString();
        }

        private static string SerializeParameterValue(object value)
        {
            if (value == null)
                return "NULL";

            return value.ToString();
        }

        #endregion
    }
}
