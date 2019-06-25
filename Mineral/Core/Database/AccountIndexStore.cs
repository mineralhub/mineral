using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Mineral.Core.Capsule;

namespace Mineral.Core.Database
{
    public class AccountIndexStore : MineralStoreWithRevoking<BytesCapsule, object>
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public AccountIndexStore(string db_name = "account-index") : base(db_name) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Put(AccountCapsule account)
        {
            Put(account.AccountName.ToByteArray(), new BytesCapsule(account.Address.ToByteArray()));
        }

        public byte[] Get(ByteString name)
        {
            return Get(name.ToByteArray())?.Data;
        }

        public override bool Contains(byte[] key)
        {
            byte[] value = this.revoking_db.GetUnchecked(key);

            return value != null && value.Length > 0;
        }

        public override BytesCapsule Get(byte[] key)
        {
            byte[] value = this.revoking_db.GetUnchecked(key);

            return (value != null && value.Length > 0) ? new BytesCapsule(value) : null;
        }
        #endregion
    }
}
