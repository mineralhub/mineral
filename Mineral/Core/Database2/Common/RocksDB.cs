using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Storage;
using Mineral.Core.Config.Arguments;
using RocksDbSharp;

namespace Mineral.Core.Database2.Common
{
    public class RocksDB : IBaseDB<byte[], byte[]>, Flusher
    {
        #region Field
        private RocksDBDataSource db = null;
        private WriteOptionWrapper write_options = WriteOptionWrapper.GetInstance().Sync(Args.Instance.Storage.Sync);
        #endregion


        #region Property
        public long Size { get { return this.db != null ? this.db.GetTotal() : 0; } }
        public bool IsEmpty { get { return Size == 0; } }
        public RocksDBDataSource DB { get { return this.db; } }
        #endregion


        #region Constructor
        public RocksDB(string parent, string name)
        {
            this.db = new RocksDBDataSource(
                parent + @"\" + Args.Instance.Storage.Directory,
                name);
            this.db.Init();
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Close()
        {
            this.db.Close();
        }

        public void Reset()
        {
            this.db.Reset();
        }


        public void Flush(Dictionary<byte[], byte[]> batch)
        {
            this.db.UpdateByBatch(batch);
        }

        public byte[] Get(byte[] key)
        {
            return this.db.GetData(key);
        }

        public void Put(byte[] key, byte[] value)
        {
            this.db.PutData(key, value);
        }

        public void Remove(byte[] key)
        {
            this.db.DeleteData(key);
        }

        public IEnumerator<KeyValuePair<byte[], byte[]>> GetEnumerator()
        {
            return this.db.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator<KeyValuePair<byte[], byte[]>>)GetEnumerator();
        }
        #endregion
    }
}
