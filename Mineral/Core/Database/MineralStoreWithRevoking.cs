using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mineral.Core.Capsule;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Database2.Common;
using Mineral.Core.Database2.Core;

namespace Mineral.Core.Database
{
    public abstract class MineralStoreWithRevoking<T, U> : IDatabase<T>
        where T : IProtoCapsule<U>

    {
        #region Field
        protected IRevokingDB revoking_db;
        private IRevokingDatabase revoking_database = new SnapshotManager();
        private string db_name = "";
        #endregion


        #region Property
        public long Size
        {
            get { return this.revoking_db.LongCount(); }
        }

        public string Name
        {
            get { return GetType().Name; }
        }
        public string DBName
        {
            get { return this.db_name; }
        }
        #endregion


        #region Constructor
        protected MineralStoreWithRevoking(string db_name)
        {
            this.db_name = db_name;
            this.revoking_db = new RevokingDBWithCaching(this.db_name);
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
                return (T)Activator.CreateInstance(typeof(T), new object[] { value });
            }
            catch (System.Exception e)
            {
                Logger.Error(e);
                throw new NullReferenceException();
            }
        }
        #endregion


        #region External Method
        public virtual bool Contains(byte[] key)
        {
            return this.revoking_db.Contains(key);
        }

        public virtual void Put(byte[] key, T item)
        {
            if (key == null || item == null)
                return;

            this.revoking_db.Put(key, item.Data);
        }

        public virtual T Get(byte[] key)
        {
            return Of(this.revoking_db.Get(key));
        }

        public virtual void Delete(byte[] key)
        {
            this.revoking_db.Delete(key);
        }

        public virtual void Close()
        {
            this.revoking_db.Close();
        }

        public virtual T GetUnchecked(byte[] key)
        {
            return Of(this.revoking_db.GetUnchecked(key));
        }

        public void Reset()
        {
            this.revoking_db.Reset();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator<KeyValuePair<byte[], T>>)GetEnumerator();
        }

        public IEnumerator<KeyValuePair<byte[], T>> GetEnumerator()
        {
            Dictionary<byte[], T> result = new Dictionary<byte[], T>();
            IEnumerator<KeyValuePair<byte[], byte[]>> it = this.revoking_db.GetEnumerator();
            while (it.MoveNext())
            {
                yield return new KeyValuePair<byte[], T>(it.Current.Key, Of(it.Current.Value));
            }
        }
        #endregion
    }
}
