using System.Data;
using System.Data.Common;

namespace EF.Diagnostics.Profiling.Data
{
    /// <summary>
    /// A wrapper of <see cref="DbTransaction"/> which supports DB profiling.
    /// </summary>
    public class ProfiledDbTransaction : DbTransaction
    {
        private readonly DbTransaction _transaction;
        private readonly IDbProfiler _dbProfiler;
        private DbConnection _dbConnection;

        #region Constructors

        /// <summary>
        /// Initializes a <see cref="ProfiledDbTransaction"/>.
        /// </summary>
        /// <param name="transaction">The <see cref="DbTransaction"/> to be profiled.</param>
        /// <param name="dbProfiler">The <see cref="IDbProfiler"/>.</param>
        public ProfiledDbTransaction(DbTransaction transaction, IDbProfiler dbProfiler)
        {
            _transaction = transaction;
            _dbProfiler = dbProfiler;
        }

        #endregion

        #region DbTransaction Members

        /// <summary>
        /// Commits the database transaction. 
        /// </summary>
        public override void Commit()
        {
            _transaction.Commit();
        }

        /// <summary>
        /// Returns the <see cref="DbConnection"/> object associated with the transaction. 
        /// </summary>
        protected override DbConnection DbConnection
        {
            get
            {
                if (_transaction.Connection == null)
                {
                    return null;
                }

                if (_dbConnection == null)
                {
                    var profiledDbConnection = _transaction.Connection as ProfiledDbConnection;
                    if (profiledDbConnection != null)
                    {
                        _dbConnection = profiledDbConnection;
                    }
                    else
                    {
                        _dbConnection = new ProfiledDbConnection(_transaction.Connection, _dbProfiler);
                    }
                }

                return _dbConnection;
            }
        }

        /// <summary>
        /// Returns <see cref="IsolationLevel"/> for this transaction. 
        /// </summary>
        public override IsolationLevel IsolationLevel
        {
            get { return _transaction.IsolationLevel; }
        }

        /// <summary>
        /// Rolls back a transaction from a pending state. 
        /// </summary>
        public override void Rollback()
        {
            _transaction.Rollback();
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ProfiledDbTransaction"/> and optionally releases the managed resources. 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _transaction.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}
