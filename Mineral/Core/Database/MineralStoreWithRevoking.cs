using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mineral.Core.Capsule;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Database2.Common;
using Mineral.Core.Database2.Core;
using Mineral.Core.Exception;

namespace Mineral.Core.Database
{
    public abstract class MineralStoreWithRevoking<T, U> : IDatabase<T>
        where T : IProtoCapsule<U>

    {
        #region Field
        protected IRevokingDB revoking_db = null;
        private IRevokingDatabase revoking_database = null;
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
        protected MineralStoreWithRevoking(IRevokingDatabase revoking_database, string db_name)
        {
            this.db_name = db_name;
            this.revoking_db = new RevokingDBWithCaching(this.db_name, typeof(Core.Database2.Common.LevelDB));
            this.revoking_database = revoking_database;
            this.revoking_database.Add(this.revoking_db);
        }

        protected MineralStoreWithRevoking(IRevokingDatabase revoking_database, string db_name, Type db_type)
        {
            this.db_name = db_name;
            this.revoking_db = new RevokingDBWithCaching(db_name, db_type);
            this.revoking_database = revoking_database;
            this.revoking_database.Add(this.revoking_db);
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private T Of(byte[] value)
        {
            if (value == null)
                return default(T);

            try
            {
                return (T)Activator.CreateInstance(typeof(T), new object[] { value });
            }
            catch (System.Exception e)
            {
                throw new BadItemException(e.Message);
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
