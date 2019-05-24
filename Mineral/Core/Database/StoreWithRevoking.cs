using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Capsule;
using Mineral.Core.Database2.Common;
using Mineral.Core.Database2.Core;

namespace Mineral.Core.Database
{
    public abstract class StoreWithRevoking<T> : IChainBase<T>
        where T : ICapsule<T>
    {
        #region Field
        protected IRevokingDB revoking_db;
        private IRevokingDatabase revoking_database;
        #endregion


        #region Property
        #endregion


        #region Constructor
        protected StoreWithRevoking(string db_name)
        {

        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Close()
        {
            throw new NotImplementedException();
        }

        public bool Contains(byte[] key)
        {
            throw new NotImplementedException();
        }

        public void Delete(byte[] key)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public T Get(byte[] key)
        {
            throw new NotImplementedException();
        }

        public string GetDBName()
        {
            throw new NotImplementedException();
        }

        public string GetName()
        {
            throw new NotImplementedException();
        }

        public T GetUnchecked(byte[] key)
        {
            throw new NotImplementedException();
        }

        public void Put(byte[] key, T item)
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
