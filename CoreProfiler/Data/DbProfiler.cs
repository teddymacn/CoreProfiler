using System;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Threading.Tasks;
using CoreProfiler.Timings;

namespace CoreProfiler.Data
{
    /// <summary>
    /// The default <see cref="IDbProfiler"/> implementation.
    /// </summary>
    public class DbProfiler : IDbProfiler
    {
        private readonly IProfiler _profiler;
        private readonly ConcurrentDictionary<DbDataReader, DbTiming> _inProgressDataReaders;

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="DbProfiler"/>.
        /// </summary>
        /// <param name="profiler">The profiler.</param>
        public DbProfiler(IProfiler profiler)
        {
            if (profiler == null)
            {
                throw new ArgumentNullException("profiler");
            }

            _profiler = profiler;
            _inProgressDataReaders = new ConcurrentDictionary<DbDataReader, DbTiming>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Executes &amp; profiles the execution of the specified <see cref="DbCommand"/>.
        /// </summary>
        /// <param name="executeType">The <see cref="DbExecuteType"/>.</param>
        /// <param name="command">The <see cref="DbCommand"/> to be executed &amp; profiled.</param>
        /// <param name="execute">
        ///     The execute handler, 
        ///     which should return the <see cref="DbDataReader"/> instance if it is an ExecuteReader operation.
        ///     If it is not ExecuteReader, it should return null.
        /// </param>
        /// <param name="tags">The tags of the <see cref="DbTiming"/> which will be created internally.</param>
        public virtual void ExecuteDbCommand(DbExecuteType executeType, DbCommand command, Func<DbDataReader> execute, TagCollection tags)
        {
            if (execute == null)
            {
                return;
            }

            if (command == null)
            {
                execute();
                return;
            }

            var dbTiming = new DbTiming(_profiler, executeType, command) { Tags = tags };

            var dataReader = execute();
            if (dataReader == null)
            {
                // if not executing reader, stop the sql timing right after execute()
                dbTiming.Stop();
                return;
            }

            dbTiming.FirstFetch();
            var reader = dataReader as ProfiledDbDataReader ??
                new ProfiledDbDataReader(dataReader, this);
            _inProgressDataReaders[reader] = dbTiming;
        }
        
        /// <summary>
        /// Executes &amp; profiles the execution of the specified <see cref="DbCommand"/> asynchronously.
        /// </summary>
        /// <param name="executeType">The <see cref="DbExecuteType"/>.</param>
        /// <param name="command">The <see cref="DbCommand"/> to be executed &amp; profiled.</param>
        /// <param name="execute">
        ///     The execute handler, 
        ///     which should return a scalar value.
        /// </param>
        /// <param name="tags">The tags of the <see cref="DbTiming"/> which will be created internally.</param>
        public async Task<object> ExecuteDbCommandAsync(DbExecuteType executeType, DbCommand command, Func<Task<object>> execute, TagCollection tags)
        {
            if (execute == null)
            {
                return null;
            }
            
            if (executeType == DbExecuteType.Reader)
                throw new NotSupportedException("ExecuteDbCommandAsync doesn't support executing data reader.");

            if (command == null)
            {
                return await execute();
            }

            var dbTiming = new DbTiming(_profiler, executeType, command) { Tags = tags };
            try
            {
                return await execute();
            }
            finally
            {
                dbTiming.Stop();
            }
        }

        /// <summary>
        /// Notifies the profiler that the data reader has finished reading
        /// so that the DB timing attached to the data reading could be stopped.
        /// </summary>
        /// <param name="dataReader">The <see cref="DbDataReader"/>.</param>
        public virtual void DataReaderFinished(DbDataReader dataReader)
        {
            if (dataReader == null)
            {
                return;
            }

            DbTiming dbTiming;
            if (_inProgressDataReaders.TryRemove(dataReader, out dbTiming))
            {
                dbTiming.Stop();
            }
        }

        #endregion

        #region IDbProfiler Members

        void IDbProfiler.ExecuteDbCommand(DbExecuteType executeType, DbCommand command, Func<DbDataReader> execute, TagCollection tags)
        {
            ExecuteDbCommand(executeType, command, execute, tags);
        }

        void IDbProfiler.DataReaderFinished(DbDataReader dataReader)
        {
            DataReaderFinished(dataReader);
        }

        #endregion
    }
}
