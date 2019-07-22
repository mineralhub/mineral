using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Storage;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Database2.Common;
using Mineral.Core.Database2.Core;

namespace Mineral.Core.Database
{
    public abstract class MineralDatabase<T> : IDatabase<T>
    {
        #region Field
        protected IDBSourceInter<byte[]> db_source = null;
        private string db_name = "";
        #endregion


        #region Property
        public IDBSourceInter<byte[]> DBSource
        {
            get { return this.db_source; }
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
        protected MineralDatabase(string db_name)
        {
            this.db_name = db_name;
            this.db_source = new LevelDBDataSource(Args.Instance.GetOutputDirectoryByDBName(this.db_name), this.db_name);
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

        public T GetUnchecked(byte[] key)
        {
            return default(T);
        }

        public void Reset()
        {
            this.db_source.Reset();
        }

        public IEnumerator<KeyValuePair<byte[], T>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
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
