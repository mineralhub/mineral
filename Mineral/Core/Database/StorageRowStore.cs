using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Capsule;

namespace Mineral.Core.Database
{
    public class StorageRowStore : MineralStoreWithRevoking<StorageRowCapsule, byte[]>
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public StorageRowStore(string db_name = "storage-row") : base(db_name) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override StorageRowCapsule Get(byte[] key)
        {
            return GetUnchecked(key);
        }
        #endregion
    }
}
