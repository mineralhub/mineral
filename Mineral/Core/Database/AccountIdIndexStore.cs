using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Mineral.Core.Capsule;

namespace Mineral.Core.Database
{
    public class AccountIdIndexStore : MineralStoreWithRevoking<BytesCapsule, object>
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public AccountIdIndexStore(string db_name = "accountid-index") : base(db_name) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private static byte[] GetLowerCaseAccountId(byte[] account_id)
        {
            return ByteString.CopyFromUtf8(ByteString.CopyFrom(account_id).ToStringUtf8().ToLower()).ToByteArray();
        }
        #endregion


        #region External Method
        public override bool Contains(byte[] key)
        {
            byte[] value = this.revoking_db.GetUnchecked(GetLowerCaseAccountId(key));
            return value != null && value.Length > 0;
        }

        public override BytesCapsule Get(byte[] key)
        {
            byte[] value = this.revoking_db.GetUnchecked(GetLowerCaseAccountId(key));

            return value != null && value.Length > 0 ? new BytesCapsule(value) : null;
        }

        public byte[] Get(ByteString key)
        {
            BytesCapsule value = Get(key.ToByteArray());

            return value != null && value.Data != null ? value.Data : null;
        }

        public void Put(AccountCapsule account)
        {
            byte[] account_id = GetLowerCaseAccountId(account.AccountId.ToByteArray());
            Put(account_id, new BytesCapsule(account.Address.ToByteArray()));
        }
        #endregion
    }
}
