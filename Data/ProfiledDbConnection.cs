using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace CoreProfiler.Data
{
    /// <summary>
    /// A <see cref="DbConnection"/> wrapper which supports DB profiling.
    /// </summary>
    public class ProfiledDbConnection : DbConnection
    {
        private readonly DbConnection _connection;
        private readonly IDbProfiler _dbProfiler;

        #region Constructors

        /// <summary>
        /// Initializes a <see cref="ProfiledDbConnection"/>.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/> to be profiled.</param>
        /// <param name="dbProfiler">The <see cref="IDbProfiler"/>.</param>
        public ProfiledDbConnection(DbConnection connection, IDbProfiler dbProfiler)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            if (dbProfiler == null)
            {
                throw new ArgumentNullException("dbProfiler");
            }
            
            _connection = connection;
            if (_connection != null)
            {
                _connection.StateChange += StateChangeHandler;
            }
            _dbProfiler = dbProfiler;
        }

        #endregion

        #region DbConnection Members

        /// <summary>
        /// Starts a database transaction. 
        /// </summary>
        /// <param name="isolationLevel">Specifies the isolation level for the transaction. </param>
        /// <returns>An object representing the new transaction.</returns>
        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            var transaction = _connection.BeginTransaction(isolationLevel);
            var profiledTransaction = transaction as ProfiledDbTransaction;
            if (profiledTransaction != null)
            {
                return profiledTransaction;
            }

            return new ProfiledDbTransaction(transaction, _dbProfiler);
        }

        /// <summary>
        /// Changes the current database for an open connection. 
        /// </summary>
        /// <param name="databaseName">Specifies the name of the database for the connection to use.</param>
        public override void ChangeDatabase(string databaseName)
        {
            _connection.ChangeDatabase(databaseName);
        }

        /// <summary>
        /// Closes the connection to the database. This is the preferred method of closing any open connection. 
        /// </summary>
        public override void Close()
        {
            _connection.Close();
        }

        /// <summary>
        /// Gets or sets the string used to open the connection. 
        /// </summary>
        public override string ConnectionString
        {
            get
            {
                return _connection.ConnectionString;
            }
            set
            {
                _connection.ConnectionString = value;
            }
        }

        /// <summary>
        /// Creates and returns a <see cref="DbCommand"/> object associated with the current connection. 
        /// </summary>
        /// <returns></returns>
        protected override DbCommand CreateDbCommand()
        {
            var command = _connection.CreateCommand();
            var profiledCommand = command as ProfiledDbCommand;
            if (profiledCommand != null)
            {
                return profiledCommand;
            }

            return new ProfiledDbCommand(command, _dbProfiler);
        }

        /// <summary>
        /// Gets the name of the database server to which to connect. 
        /// </summary>
        public override string DataSource
        {
            get { return _connection.DataSource; }
        }

        /// <summary>
        /// Gets the name of the current database after a connection is opened, or the database name specified in the connection string before the connection is opened. 
        /// </summary>
        public override string Database
        {
            get { return _connection.Database; }
        }

        /// <summary>
        /// Opens a database connection with the settings specified by the ConnectionString. 
        /// </summary>
        public override void Open()
        {
            _connection.Open();
        }

        /// <summary>
        /// Gets a string that represents the version of the server to which the object is connected. 
        /// </summary>
        public override string ServerVersion
        {
            get { return _connection.ServerVersion; }
        }

        /// <summary>
        /// Gets a string that describes the state of the connection. 
        /// </summary>
        public override ConnectionState State
        {
            get { return _connection.State; }
        }
        
        /// <summary>
        /// Gets the time to wait while establishing a connection before terminating the attempt and generating an error. 
        /// </summary>
        public override int ConnectionTimeout
        {
            get
            {
                return _connection.ConnectionTimeout;
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ProfiledDbConnection"/> and optionally releases the managed resources. 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_connection != null)
                {
                    _connection.StateChange -= StateChangeHandler;

                    if (_connection.State != ConnectionState.Closed)
                    {
                        _connection.Dispose();
                    }
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Opens the connection.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task OpenAsync(CancellationToken cancellationToken)
        {
            return _connection.OpenAsync(cancellationToken);
        }

        #endregion

        #region Private Methods

        private void StateChangeHandler(object sender, StateChangeEventArgs stateChangeEventArgs)
        {
            OnStateChange(stateChangeEventArgs);
        }

        #endregion
    }
}
