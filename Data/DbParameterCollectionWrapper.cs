using System;
using System.Data.Common;

namespace EF.Diagnostics.Profiling.Data
{
    internal sealed class DbParameterCollectionWrapper : DbParameterCollection
    {
        private readonly DbParameterCollection _parameterCollection;

        public DbParameterCollectionWrapper(DbParameterCollection parameterCollection)
        {
            _parameterCollection = parameterCollection;
        }

        #region DbParameterCollection Members

        public override int Add(object value)
        {
            return _parameterCollection.Add(value);
        }

        public override void AddRange(Array values)
        {
            if (_parameterCollection != null)
            {
                _parameterCollection.AddRange(values);
            }
            else
            {
                foreach (var value in values)
                {
                    Add(value);
                }
            }
        }

        public override void Clear()
        {
            _parameterCollection.Clear();
        }

        public override bool Contains(string value)
        {
            return _parameterCollection.Contains(value);
        }

        public override bool Contains(object value)
        {
            return _parameterCollection.Contains(value);
        }

        public override void CopyTo(Array array, int index)
        {
            if (array == null)
            {
                return;
            }

            _parameterCollection.CopyTo(array, index);
        }

        public override int Count
        {
            get { return _parameterCollection.Count; }
        }

        public override System.Collections.IEnumerator GetEnumerator()
        {
            return _parameterCollection.GetEnumerator();
        }

        protected override DbParameter GetParameter(string parameterName)
        {
            if (_parameterCollection != null && _parameterCollection.Contains(parameterName))
            {
                return _parameterCollection[parameterName];
            }

            if (_parameterCollection!= null && _parameterCollection.Contains(parameterName))
            {
                return new DbParameterWrapper(_parameterCollection[parameterName]);
            }

            return null;
        }

        protected override DbParameter GetParameter(int index)
        {
            if (index < 0 || index >= _parameterCollection.Count)
            {
                return null;
            }

            if (_parameterCollection != null)
            {
                return _parameterCollection[index];
            }

            return new DbParameterWrapper(_parameterCollection[index]);
        }

        public override int IndexOf(string parameterName)
        {
            return _parameterCollection.IndexOf(parameterName);
        }

        public override int IndexOf(object value)
        {
            return _parameterCollection.IndexOf(value);
        }

        public override void Insert(int index, object value)
        {
            _parameterCollection.Insert(index, value);
        }

        public override void Remove(object value)
        {
            _parameterCollection.Remove(value);
        }

        public override void RemoveAt(string parameterName)
        {
            _parameterCollection.RemoveAt(parameterName);
        }

        public override void RemoveAt(int index)
        {
            _parameterCollection.RemoveAt(index);
        }

        protected override void SetParameter(string parameterName, DbParameter value)
        {
            _parameterCollection[parameterName] = value;
        }

        protected override void SetParameter(int index, DbParameter value)
        {
            _parameterCollection[index] = value;
        }

        public override object SyncRoot
        {
            get { return _parameterCollection.SyncRoot; }
        }

        #endregion
    }
}
