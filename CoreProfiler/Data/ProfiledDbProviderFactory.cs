using System;
using System.Data.Common;

namespace CoreProfiler.Data
{
    /// <summary>
    /// A <see cref="DbProviderFactory"/> wrapper which supports DB profiling.
    /// </summary>
    public class ProfiledDbProviderFactory : DbProviderFactory
    {
        private readonly DbProviderFactory _dbProviderFactory;
        private readonly IDbProfiler _dbProfiler;

        #region Constructors

        /// <summary>
        /// Initializes a <see cref="ProfiledDbProviderFactory"/>.
        /// </summary>
        /// <param name="dbProviderFactory">The <see cref="DbProviderFactory"/> to be profiled.</param>
        /// <param name="dbProfiler">The <see cref="IDbProfiler"/>.</param>
        public ProfiledDbProviderFactory(DbProviderFactory dbProviderFactory, IDbProfiler dbProfiler)
        {
            if (dbProviderFactory == null)
            {
                throw new ArgumentNullException("dbProviderFactory");
            }

            if (dbProfiler == null)
            {
                throw new ArgumentNullException("dbProfiler");
            }

            _dbProviderFactory = dbProviderFactory;
            _dbProfiler = dbProfiler;
        }

        #endregion

        #region DbProviderFactory Members

        /// <summary>
        /// Creates and returns a <see cref="DbCommand"/> object associated with the current connection. 
        /// </summary>
        /// <returns>Returns the created <see cref="DbCommand"/></returns>
        public override DbCommand CreateCommand()
        {
            var command = _dbProviderFactory.CreateCommand();
            if (command == null)
            {
                return null;
            }

            var profiledCommand = command as ProfiledDbCommand;
            if (profiledCommand != null)
            {
                return profiledCommand;
            }

            return new ProfiledDbCommand(command, _dbProfiler);
        }
        
        /// <summary>
        /// Returns a new instance of the provider's class that implements the DbConnection class. 
        /// </summary>
        /// <returns></returns>
        public override DbConnection CreateConnection()
        {
            var connection = _dbProviderFactory.CreateConnection();
            if (connection == null)
            {
                return null;
            }

            var profiledConnection = connection as ProfiledDbConnection;
            if (profiledConnection != null)
            {
                return profiledConnection;
            }

            return new ProfiledDbConnection(connection, _dbProfiler);
        }

        /// <summary>
        /// Returns a new instance of the provider's class that implements the <see cref="DbConnectionStringBuilder"/> class. 
        /// </summary>
        /// <returns></returns>
        public override DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return _dbProviderFactory.CreateConnectionStringBuilder();
        }

        /// <summary>
        /// Returns a new instance of the provider's class that implements the <see cref="DbParameter"/> class. 
        /// </summary>
        /// <returns></returns>
        public override DbParameter CreateParameter()
        {
            return _dbProviderFactory.CreateParameter();
        }

        #endregion
    }
}
