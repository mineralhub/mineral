using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Capsule;
using Protocol;

namespace Mineral.Core.Database
{
    public class DelegatedResourceAccountIndexStore
        : MineralStoreWithRevoking<DelegatedResourceAccountIndexCapsule, DelegatedResourceAccountIndex>
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public DelegatedResourceAccountIndexStore(string db_name = "DelegatedResourceAccountIndex") : base(db_name) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override DelegatedResourceAccountIndexCapsule Get(byte[] key)
        {
            byte[] value = this.revoking_db.GetUnchecked(key);

            return value != null && value.Length > 0 ? new DelegatedResourceAccountIndexCapsule(value) : null;
        }
        #endregion
    }
}
