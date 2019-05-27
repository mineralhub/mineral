using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Capsule;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Database2.Common;
using Mineral.Core.Database2.Core;

namespace Mineral.Core.Database
{
    public abstract class MineralStoreWithRevoking<T> : IChainBase<T>
        where T : ICapsule<T>
    {
        #region Field
        protected IRevokingDB revoking_db;
        private IRevokingDatabase revoking_database;
        private string db_name = "";
        #endregion


        #region Property
        #endregion


        #region Constructor
        protected MineralStoreWithRevoking(string db_name)
        {
            this.db_name = db_name;
            int db_version = Args.Instance.Storage.Version;
            string db_engine = Args.Instance.Storage.Engine;

            if (Args.Instance.Storage.Version == 2)
            {
                if (db_engine.ToUpper().Equals("LEVELDB"))
                    this.revoking_db = new RevokingDBWithCaching(this.db_name, typeof(LevelDB));
                else if (db_engine.ToUpper().Equals("ROCKSDB"))
                    this.revoking_db = new RevokingDBWithCaching(this.db_name, typeof(RocksDB));
                else
                    throw new System.Exception("Invalid database version");
            }
            else
            {
                throw new System.Exception("Invalid database version");
            }
        }

        protected MineralStoreWithRevoking(string db_name, Type db_type)
        {
            this.db_name = db_name;
            int db_version = Args.Instance.Storage.Version;
            string db_engine = Args.Instance.Storage.Engine;

            if (Args.Instance.Storage.Version == 2)
                this.revoking_db = new RevokingDBWithCaching(this.db_name, db_type);
            else
                throw new System.Exception("Invalid database version");
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private void Init()
        {
            this.revoking_database.Add(this.revoking_db);
        }

        private T Of(byte[] value)
        {
            try
            {
                return (T)Activator.CreateInstance(typeof(T));
            }
            catch
            {
                throw new ArgumentException();
            }
        }
        #endregion


        #region External Method
        public bool Contains(byte[] key)
        {
            return this.revoking_db.Contains(key);
        }

        public void Put(byte[] key, T item)
        {
            if (key == null || item == null)
                return;

            this.revoking_db.Put(key, item.GetData());
        }

        public T Get(byte[] key)
        {
            return Of(this.revoking_db.Get(key));
        }

        public void Delete(byte[] key)
        {
            this.revoking_db.Delete(key);
        }

        public void Close()
        {
            this.revoking_db.Close();
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
            return Of(this.revoking_db.GetUnchecked(key));
        }

        public void Reset()
        {
            this.revoking_db.Reset();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator<KeyValuePair<byte[], byte[]>>)GetEnumerator();
        }

        public IEnumerator<KeyValuePair<byte[], byte[]>> GetEnumerator()
        {
            return this.revoking_db.GetEnumerator();
        }
        #endregion
    }
}
