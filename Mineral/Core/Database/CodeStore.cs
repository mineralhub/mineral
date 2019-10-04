using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Capsule;

namespace Mineral.Core.Database
{
    public class CodeStore : MineralStoreWithRevoking<CodeCapsule, byte[]>
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public CodeStore(IRevokingDatabase revoking_database, string db_name = "code")
            : base(revoking_database, db_name)
        {
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override CodeCapsule Get(byte[] key)
        {
            return GetUnchecked(key);
        }
        #endregion
    }
}
