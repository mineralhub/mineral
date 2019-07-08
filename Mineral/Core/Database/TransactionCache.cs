using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Capsule;

namespace Mineral.Core.Database
{
    public class TransactionCache : MineralStoreWithRevoking<BytesCapsule, object>
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public TransactionCache(string db_name = "trans-cache") : base(db_name) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        #endregion
    }
}
