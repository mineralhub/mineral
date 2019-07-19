using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Database2.Common
{
    public class HashDB : IBaseDB<byte[], byte[]>
    {
        #region Field
        private ConcurrentDictionary<byte[], byte[]> db = new ConcurrentDictionary<byte[], byte[]>();
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
        public byte[] Get(byte[] key)
        {
            if (this.db.TryGetValue(key, out byte[] value))
                return value;
            else
                return null;
        }

        public void Put(byte[] key, byte[] value)
        {
            this.db.TryAdd(key, value);
        }

        public void Remove(byte[] key)
        {
            this.db.TryRemove(key, out _);
        }

        public IEnumerator<KeyValuePair<byte[], byte[]>> GetEnumerator()
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
