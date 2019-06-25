using System;
using System.Collections.Generic;
using System.Linq;
using Mineral.Core.Capsule;
using Protocol;

namespace Mineral.Core.Database
{
    public class DelegatedResourceStore : MineralStoreWithRevoking<DelegatedResourceCapsule, DelegatedResource>
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public DelegatedResourceStore(string db_name = "DelegatedResource") : base(db_name) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override DelegatedResourceCapsule Get(byte[] key)
        {
            return base.Get(key);
        }

        public List<DelegatedResourceCapsule> GetByFrom(byte[] key)
        {
            return this.revoking_db.GetValuesNext(key, long.MaxValue)
                        .Select(value => new DelegatedResourceCapsule(value))
                        .Where(capsule => capsule != null)
                        .ToList();
        }
        #endregion
    }
}
