using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Storage;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Database2.Core;

namespace Mineral.Core.Database
{
    public abstract class MineralDatabase<T> : IMineralChainBase<T>
    {
        #region Field
        protected IDBSourceInter<byte[]> db_source = null;
        private string db_name = "";
        #endregion


        #region Property
        public IDBSourceInter<byte[]> DBSource { get { return this.db_source; } }
        #endregion


        #region Constructor
        protected MineralDatabase(string db_name)
        {
            this.db_name = db_name;

            if (Args.Instance.Storage.Engine.ToUpper().Equals("LEVELDB"))
            {
                this.db_source = new LevelDBDataSource(Args.Instance.GetOutputDirectoryByDBName(this.db_name), this.db_name);
            }
            else if (Args.Instance.Storage.Engine.ToUpper().Equals("ROCKSDB"))
            {
                string parent = Args.Instance.GetOutputDirectoryByDBName(this.db_name) + @"\" + Args.Instance.Storage.Directory;
                this.db_source = new RocksDBDataSource(parent, this.db_name);
            }

            this.db_source.Init();
        }

        protected MineralDatabase() { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Close()
        {
            this.db_source.Close();
        }

        public string GetDBName()
        {
            return this.db_name;
        }

        public string GetName()
        {
            return this.GetType().Name;
        }

        public T GetUnchecked(byte[] key)
        {
            return default(T);
        }

        public void Reset()
        {
            this.db_source.Reset();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator<KeyValuePair<byte[], byte[]>>)GetEnumerator();
        }

        public IEnumerator<KeyValuePair<byte[], byte[]>> GetEnumerator()
        {
            IEnumerator<KeyValuePair<byte[], byte[]>> result = null;
            if (this.db_source.GetType().Equals(typeof(LevelDBDataSource)))
            {
                result = ((LevelDBDataSource)this.db_source).GetEnumerator();
            }
            else if (this.db_source.GetType().Equals(typeof(RocksDBDataSource)))
            {
                result = ((RocksDBDataSource)this.db_source).GetEnumerator();
            }
            else
            {
                result = null;
            }

            return result;
        }

        #region Abstract - IMineralChainBase
        public abstract bool Contains(byte[] key);
        public abstract T Get(byte[] key);
        public abstract void Put(byte[] key, T value);
        public abstract void Delete(byte[] key);
        #endregion
        #endregion
    }
}
