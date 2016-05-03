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
        private readonly IDbProfiler _dbProfiler;
        private DbConnection _dbConnection;
        private DbParameterCollection _dbParameterCollection;
        private DbTransaction _dbTransaction;

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
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            if (dbProfiler == null)
            {
                throw new ArgumentNullException("dbProfiler");
            }
            
            _command = command;
            _dbProfiler = dbProfiler;

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
                if (_dbConnection == null && _command.Connection == null && (_command == null || _command.Connection == null))
                {
                    return null;
                }

                if (_dbConnection == null)
                {
                    var conn = _command.Connection;

                    var profiledDbConnection = conn as ProfiledDbConnection;
                    if (profiledDbConnection != null)
                    {
                        _dbConnection = profiledDbConnection;
                    }
                    else
                    {
                        _dbConnection = new ProfiledDbConnection(conn, _dbProfiler);
                    }
                }

                return _dbConnection;
            }
            set
            {
                _command.Connection = _dbConnection = value;
            }
        }

        /// <summary>
        /// Gets the collection of <see cref="DbParameter"/> objects. 
        /// </summary>
        protected override DbParameterCollection DbParameterCollection
        {
            get
            {
                if (_command.Parameters == null && (_command == null || _command.Parameters == null))
                {
                    return null;
                }

                if (_dbParameterCollection == null)
                {
                    if (_command != null)
                    {
                        _dbParameterCollection = _command.Parameters;
                    }
                    else if (_command.Parameters != null)
                    {
                        _dbParameterCollection = new DbParameterCollectionWrapper(_command.Parameters);
                    }
                }

                return _dbParameterCollection;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DbTransaction"/> within which this <see cref="DbCommand"/> object executes. 
        /// </summary>
        protected override DbTransaction DbTransaction
        {
            get
            {
                if (_command.Transaction == null)
                {
                    return null;
                }

                if (_dbTransaction == null)
                {
                    var trans = _command.Transaction;

                    var profiledDbTransaction = trans as ProfiledDbTransaction;
                    if (profiledDbTransaction != null)
                    {
                        _dbTransaction = profiledDbTransaction;
                    }
                    else
                    {
                        _dbTransaction = new ProfiledDbTransaction(trans, _dbProfiler);
                    }
                }

                return _dbTransaction;
            }
            set
            {
                _dbTransaction = value;
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
            DbDataReader reader = null;
            _dbProfiler.ExecuteDbCommand(
                DbExecuteType.Reader
                , _command
                , () => reader = _command.ExecuteReader(behavior)
                , Tags);

            var profiledReader = reader as ProfiledDbDataReader;
            if (profiledReader != null)
            {
                return profiledReader;
            }

            return new ProfiledDbDataReader(reader, _dbProfiler);
        }

        /// <summary>
        /// Executes a SQL statement against a connection object. 
        /// </summary>
        /// <returns>Returns The number of rows affected. </returns>
        public override int ExecuteNonQuery()
        {
            int affected = 0;
            _dbProfiler.ExecuteDbCommand(
                DbExecuteType.NonQuery, _command, () => { affected = _command.ExecuteNonQuery(); return null; }, Tags);
            return affected;
        }

        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query. All other columns and rows are ignored. 
        /// </summary>
        /// <returns>The first column of the first row in the result set. </returns>
        public override object ExecuteScalar()
        {
            object returnValue = null;
            _dbProfiler.ExecuteDbCommand(
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

        /// <summary>
        /// Executes NonQuery.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
        {
            return _command.ExecuteNonQueryAsync(cancellationToken);
        }

        /// <summary>
        /// Executes Scalar.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
        {
            return _command.ExecuteScalarAsync(cancellationToken);
        }

        #endregion
    }
}
