using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using LevelDB;
using Mineral.Common.Storage;
using Mineral.Core.Config.Arguments;

namespace Mineral.Core.Database2.Common
{
    public class LevelDB : IBaseDB<byte[], byte[]>, Flusher
    {
        #region Field
        private LevelDBDataSource db = null;
        private WriteOptions write_options = new WriteOptions() { Sync = Args.Instance.Storage.Sync };
        #endregion


        #region Property
        public long Size { get { return this.db != null ? this.db.GetTotal() : 0; } }
        public bool IsEmpty { get { return Size == 0; } }
        public LevelDBDataSource DB { get { return this.db; } }
        #endregion


        #region Constructor
        public LevelDB(string parent, string name)
        {
            this.db = new LevelDBDataSource(parent, name);
            db.Init();
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
