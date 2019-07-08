using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Capsule;

namespace Mineral.Core.Database
{
    public class RecentBlockStore : MineralStoreWithRevoking<BytesCapsule, object>
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public RecentBlockStore(string db_name = "recent-block") : base(db_name) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override BytesCapsule Get(byte[] key)
        {
            byte[] value = this.revoking_db.Get(key);

            return new BytesCapsule(value);
        }
        #endregion
    }
}
