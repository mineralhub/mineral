using Mineral.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Database2.Common
{
    public class TxCacheDB : IBaseDB<byte[], byte[]>, Flusher
    {
        #region Field
        private readonly int BLOCK_COUNT = 70000;
        private Dictionary<byte[], long> db = new Dictionary<byte[], long>();
        private MultiSortedDictionary<long, byte[]> block_num = new MultiSortedDictionary<long, byte[]>();
        #endregion


        #region Property
        public long Size
        {
            get { return this.db.Count; }
        }
        public bool IsEmpty
        {
            get { return this.db.IsNullOrEmpty(); }
        }
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private void RemoveEldest()
        {
            List<long> keys = new List<long>(block_num.Keys);
            if (keys.Count > BLOCK_COUNT)
            {
                keys.Sort();
                this.block_num.Remove(keys[0]);
                Logger.Debug(
                    string.Format("RemoveEldest block number ; {0} block count : {1}", keys[0], keys.Count));
            }
        }
        #endregion


        #region External Method
        public byte[] Get(byte[] key)
        {
            byte[] result = null;
            if (this.db.TryGetValue(key, out long value))
            {
                result = BitConverter.GetBytes(value);
            }

            return result;
        }

        public IEnumerator<KeyValuePair<byte[], byte[]>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void Put(byte[] key, byte[] value)
        {
            if (key == null || value == null)
            {
                return;
            }

            long v = BitConverter.ToInt64(value, 0);
            this.block_num.Add(v, key);
            this.db.Add(key, v);
            RemoveEldest();
        }

        public void Remove(byte[] key)
        {
            if (key != null)
            {
                this.db.Remove(key);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.db.GetEnumerator();
        }

        public void Flush(Dictionary<byte[], byte[]> batch)
        {
            foreach (var item in batch)
            {
                Put(item.Key, item.Value);
            }
        }

        public void Close()
        {
            Reset();
            this.db = null;
            this.block_num = null;
        }

        public void Reset()
        {
            this.db.Clear();
            this.block_num.Clear();
        }
        #endregion
    }
}
