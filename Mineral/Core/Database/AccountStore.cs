using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Capsule;
using Protocol;

namespace Mineral.Core.Database
{
    public class AccountStore
        : MineralStoreWithRevoking<AccountCapsule, Account>
    {
        #region Field
        private Dictionary<string, byte[]> asserts_address = new Dictionary<string, byte[]>();
        private 
        #endregion


        #region Property
        #endregion


        #region Constructor
        public AccountStore(string db_name) : base(db_name) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        #endregion
    }
}
