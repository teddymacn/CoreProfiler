using System.Data.Common;

namespace CoreProfiler.Data
{
    /// <summary>
    /// The execute types of the execution of <see cref="DbCommand"/>.
    /// </summary>
    public enum DbExecuteType : byte
    {
        /// <summary>
        /// ExecuteNonQuery
        /// </summary>
        NonQuery = 1,

        /// <summary>
        /// ExecuteReader
        /// </summary>
        Reader = 3,

        /// <summary>
        /// ExecuteScalar
        /// </summary>
        Scalar = 2
    }
}
