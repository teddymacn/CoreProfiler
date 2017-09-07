using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using CoreProfiler.Timings;

namespace CoreProfiler.Data
{
    /// <summary>
    /// A <see cref="DbCommand"/> wrapper which supports DB profiling.
    /// </summary>
    public class ProfiledDbCommand : DbCommand
    {
        private readonly DbCommand _command;
        private readonly Func<IDbProfiler> _getDbProfiler;

        #region Properties

        /// <summary>
        /// Gets or sets the tags of the <see cref="DbTiming"/> which will be created internally.
        /// </summary>
        public TagCollection Tags { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a <see cref="ProfiledDbCommand"/>.
        /// </summary>
        /// <param name="command">The <see cref="DbCommand"/> to be profiled.</param>
        /// <param name="dbProfiler">The <see cref="IDbProfiler"/>.</param>
        /// <param name="tags">The tags of the <see cref="DbTiming"/> which will be created internally.</param>
        public ProfiledDbCommand(DbCommand command, IDbProfiler dbProfiler, IEnumerable<string> tags = null)
            : this(command, dbProfiler, tags == null ? null : new TagCollection(tags))
        {
        }

        /// <summary>
        /// Initializes a <see cref="ProfiledDbCommand"/>.
        /// </summary>
        /// <param name="command">The <see cref="DbCommand"/> to be profiled.</param>
        /// <param name="dbProfiler">The <see cref="IDbProfiler"/>.</param>
        /// <param name="tags">The tags of the <see cref="DbTiming"/> which will be created internally.</param>
        public ProfiledDbCommand(DbCommand command, IDbProfiler dbProfiler, TagCollection tags)
            :this(command, () => dbProfiler, tags)
        {
        }
        
        /// <summary>
        /// Initializes a <see cref="ProfiledDbCommand"/>.
        /// </summary>
        /// <param name="command">The <see cref="DbCommand"/> to be profiled.</param>
        /// <param name="getDbProfiler">Gets the <see cref="IDbProfiler"/>.</param>
        /// <param name="tags">The tags of the <see cref="DbTiming"/> which will be created internally.</param>        
        public ProfiledDbCommand(DbCommand command, Func<IDbProfiler> getDbProfiler, IEnumerable<string> tags = null)
            : this(command, getDbProfiler, tags == null ? null : new TagCollection(tags))
        {
        }
        
        /// <summary>
        /// Initializes a <see cref="ProfiledDbCommand"/>.
        /// </summary>
        /// <param name="command">The <see cref="DbCommand"/> to be profiled.</param>
        /// <param name="getDbProfiler">Gets the <see cref="IDbProfiler"/>.</param>
        /// <param name="tags">The tags of the <see cref="DbTiming"/> which will be created internally.</param>
        public ProfiledDbCommand(DbCommand command, Func<IDbProfiler> getDbProfiler, TagCollection tags)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            if (getDbProfiler == null)
            {
                throw new ArgumentNullException("getDbProfiler");
            }
            
            _command = command;
            _getDbProfiler = getDbProfiler;

            Tags = tags;
        }

        #endregion

        #region DbCommand Members

        /// <summary>
        /// Attempts to cancels the execution of a <see cref="DbCommand"/>.
        /// </summary>
        public override void Cancel()
        {
            _command.Cancel();
        }

        /// <summary>
        /// Gets or sets the text command to run against the data source. 
        /// </summary>
        public override string CommandText
        {
            get
            {
                return _command.CommandText;
            }
            set
            {
                _command.CommandText = value;
            }
        }

        /// <summary>
        /// Gets or sets the wait time before terminating the attempt to execute a command and generating an error. 
        /// </summary>
        public override int CommandTimeout
        {
            get
            {
                return _command.CommandTimeout;
            }
            set
            {
                _command.CommandTimeout = value;
            }
        }

        /// <summary>
        /// Indicates or specifies how the <see cref="CommandText"/> property is interpreted. 
        /// </summary>
        public override CommandType CommandType
        {
            get
            {
                return _command.CommandType;
            }
            set
            {
                _command.CommandType = value;
            }
        }

        /// <summary>
        /// Creates a new instance of a <see cref="DbParameter"/> object. 
        /// </summary>
        /// <returns>Returns the created <see cref="DbParameter"/>.</returns>
        protected override DbParameter CreateDbParameter()
        {
            return _command.CreateParameter();
        }

        /// <summary>
        /// Gets or sets the <see cref="DbConnection"/> used by this DbCommand. 
        /// </summary>
        protected override DbConnection DbConnection
        {
            get
            {
                return _command.Connection;
            }
            set
            {
                if (value is ProfiledDbConnection)
                    _command.Connection = (value as ProfiledDbConnection).WrappedConnection;
                else
                    _command.Connection = value;
            }
        }

        /// <summary>
        /// Gets the collection of <see cref="DbParameter"/> objects. 
        /// </summary>
        protected override DbParameterCollection DbParameterCollection
        {
            get
            {
                return _command.Parameters;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DbTransaction"/> within which this <see cref="DbCommand"/> object executes. 
        /// </summary>
        protected override DbTransaction DbTransaction
        {
            get
            {
                return _command.Transaction;
            }
            set
            {
                if (value is ProfiledDbTransaction)
                    _command.Transaction = (value as ProfiledDbTransaction).WrappedTransaction;
                else
                    _command.Transaction = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the command object should be visible in a customized interface control. 
        /// </summary>
        public override bool DesignTimeVisible
        {
            get { return _command.DesignTimeVisible; }
            set { _command.DesignTimeVisible = value; }
        }

        /// <summary>
        /// Executes the command text against the connection. 
        /// </summary>
        /// <param name="behavior">The <see cref="CommandBehavior"/>.</param>
        /// <returns></returns>
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            var dbProfiler = _getDbProfiler();
            if (dbProfiler == null) return _command.ExecuteReader();
            
            DbDataReader reader = null;
            dbProfiler.ExecuteDbCommand(
                DbExecuteType.Reader
                , _command
                , () => reader = _command.ExecuteReader(behavior)
                , Tags);

            var profiledReader = reader as ProfiledDbDataReader;
            if (profiledReader != null)
            {
                return profiledReader;
            }

            return new ProfiledDbDataReader(reader, dbProfiler);
        }

        /// <summary>
        /// Executes a SQL statement against a connection object. 
        /// </summary>
        /// <returns>Returns The number of rows affected. </returns>
        public override int ExecuteNonQuery()
        {
            var dbProfiler = _getDbProfiler();
            if (dbProfiler == null) return _command.ExecuteNonQuery();
            
            int affected = 0;
            dbProfiler.ExecuteDbCommand(
                DbExecuteType.NonQuery, _command, () => { affected = _command.ExecuteNonQuery(); return null; }, Tags);
            return affected;
        }

        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query. All other columns and rows are ignored. 
        /// </summary>
        /// <returns>The first column of the first row in the result set. </returns>
        public override object ExecuteScalar()
        {
            var dbProfiler = _getDbProfiler();
            if (dbProfiler == null) return _command.ExecuteScalar();
            
            object returnValue = null;
            dbProfiler.ExecuteDbCommand(
                DbExecuteType.Scalar, _command, () => { returnValue = _command.ExecuteScalar(); return null; }, Tags);
            return returnValue;
        }

        /// <summary>
        /// Creates a prepared (or compiled) version of the command on the data source.
        /// </summary>
        public override void Prepare()
        {
            _command.Prepare();
        }

        /// <summary>
        /// Gets or sets how command results are applied to a row.
        /// </summary>
        public override UpdateRowSource UpdatedRowSource
        {
            get
            {
                return _command.UpdatedRowSource;
            }
            set
            {
                _command.UpdatedRowSource = value;
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ProfiledDbCommand"/> and optionally releases the managed resources. 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _command.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            var dbProfiler = _getDbProfiler();
            if (dbProfiler == null) return await _command.ExecuteReaderAsync(behavior, cancellationToken);

            var result = await dbProfiler.ExecuteDbCommandAsync(
                DbExecuteType.Reader
                , _command
                , async () => await _command.ExecuteReaderAsync(behavior, cancellationToken)
                , Tags);

            var reader = result as DbDataReader;
            if (reader == null) return null;

            var profiledReader = reader as ProfiledDbDataReader;
            if (profiledReader != null)
            {
                return profiledReader;
            }

            return new ProfiledDbDataReader(reader, dbProfiler);
        }

        /// <summary>
        /// Executes NonQuery.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
        {
            var dbProfiler = _getDbProfiler();
            if (dbProfiler == null) return (int)(await _command.ExecuteNonQueryAsync(cancellationToken));
            
            return (int)await dbProfiler.ExecuteDbCommandAsync(
                DbExecuteType.NonQuery, _command, async () => { return await _command.ExecuteNonQueryAsync(cancellationToken); }, Tags);
        }

        /// <summary>
        /// Executes Scalar.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
        {
            var dbProfiler = _getDbProfiler();
            if (dbProfiler == null) return await _command.ExecuteScalarAsync(cancellationToken);
            
            return await dbProfiler.ExecuteDbCommandAsync(
                DbExecuteType.Scalar, _command, async () => { return await _command.ExecuteScalarAsync(cancellationToken); }, Tags);
        }

        #endregion
    }
}
