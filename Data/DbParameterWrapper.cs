using System.Data;
using System.Data.Common;

namespace EF.Diagnostics.Profiling.Data
{
    internal sealed class DbParameterWrapper : DbParameter
    {
        private readonly DbParameter _parameter;

        #region Constructors

        public DbParameterWrapper(DbParameter parameter)
        {
            _parameter = parameter;
        }

        #endregion

        #region DbParameter Members

        public override DbType DbType
        {
            get
            {
                return _parameter.DbType;
            }
            set
            {
                _parameter.DbType = value;
            }
        }

        public override ParameterDirection Direction
        {
            get
            {
                return _parameter.Direction;
            }
            set
            {
                _parameter.Direction = value;
            }
        }

        public override bool IsNullable
        {
            get
            {
                return _parameter.IsNullable;
            }
            set
            {
                if (_parameter != null)
                {
                    _parameter.IsNullable = value;
                }
            }
        }

        public override string ParameterName
        {
            get
            {
                return _parameter.ParameterName;
            }
            set
            {
                _parameter.ParameterName = value;
            }
        }

        public override void ResetDbType()
        {
            if (_parameter != null)
            {
                _parameter.ResetDbType();
            }
        }

        public override int Size
        {
            get
            {
                return _parameter.Size;
            }
            set
            {
                _parameter.Size = value;
            }
        }

        public override string SourceColumn
        {
            get
            {
                return _parameter.SourceColumn;
            }
            set
            {
                _parameter.SourceColumn = value;
            }
        }

        public override bool SourceColumnNullMapping
        {
            get
            {
                if (_parameter != null)
                {
                    return _parameter.SourceColumnNullMapping;
                }

                return false;
            }
            set
            {
                if (_parameter != null)
                {
                    _parameter.SourceColumnNullMapping = value;
                }
            }
        }

        public override object Value
        {
            get
            {
                return _parameter.Value;
            }
            set
            {
                _parameter.Value = value;
            }
        }

        public override byte Precision
        {
            get
            {
                return _parameter.Precision;
            }
            set
            {
                _parameter.Precision = value;
            }
        }

        public override byte Scale
        {
            get
            {
                return _parameter.Scale;
            }
            set
            {
                _parameter.Scale = value;
            }
        }

        #endregion
    }
}
