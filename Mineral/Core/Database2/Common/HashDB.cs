using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Database2.Common
{
    public class HashDB : IBaseDB<Key, Value>
    {
        #region Field
        private ConcurrentDictionary<Key, Value> db = new ConcurrentDictionary<Key, Value>(Environment.ProcessorCount * 2, 50000, new KeyEqualComparer());
        #endregion


        #region Property
        public long Size { get { return this.db.Count; } }
        public bool IsEmpty { get { return this.db.IsEmpty; } }
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public Value Get(Key key)
        {
            if (this.db.TryGetValue(key, out Value value))
                return value;
            else
                return null;
        }

        public void Put(Key key, Value value)
        {
            if (this.db.TryGetValue(key, out Value old))
            {
                this.db.TryUpdate(key, value, old);
            }
            else
            {
                this.db.TryAdd(key, value);
            }
        }

        public void Remove(Key key)
        {
            this.db.TryRemove(key, out _);
        }

        public IEnumerator<KeyValuePair<Key, Value>> GetEnumerator()
        {
            return this.db.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator <KeyValuePair<byte[], byte[]>>)GetEnumerator();
        }
        #endregion
    }
}
