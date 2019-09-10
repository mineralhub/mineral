using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Capsule;
using Mineral.Core.Database.Fast.Callback;
using Mineral.Core.Database.Fast.Callback.StoreTrie;
using Mineral.Utils;
using Protocol;

namespace Mineral.Core.Database
{
    public class AccountStore
        : MineralStoreWithRevoking<AccountCapsule, Account>
    {
        #region Field
        private static Dictionary<string, byte[]> asserts_address = new Dictionary<string, byte[]>();
        #endregion


        #region Property
        #endregion


        #region Constructor
        public AccountStore(string db_name = "account")
            : base(db_name)
        {
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public static void SetAccount(GenesisBlockConfig args)
        {
            foreach (Config.Arguments.Account account in args.Assets)
            {
                asserts_address.Add(account.Name, Wallet.Base58ToAddress(account.Address));
            }
        }

        public AccountCapsule GetBlackHole()
        {
            asserts_address.TryGetValue("Blackhole", out byte[] value);
            return GetUnchecked(value);
        }

        public AccountCapsule GetSun()
        {
            asserts_address.TryGetValue("Sun", out byte[] value);
            return GetUnchecked(value);
        }

        public AccountCapsule GetZion()
        {
            asserts_address.TryGetValue("Zion", out byte[] value);
            return GetUnchecked(value);
        }

        public override void Put(byte[] key, AccountCapsule item)
        {
            base.Put(key, item);
            Manager.Instance.FastSyncCallback.AccountCallBack(key, item);
        }

        public override AccountCapsule Get(byte[] key)
        {
            byte[] value = this.revoking_db.GetUnchecked(key);
            return value.IsNotNullOrEmpty() ? new AccountCapsule(value) : null;
        }

        public override void Close()
        {
            base.Close();
            Manager.Instance.AccountStateTrie.Close();
        }
        #endregion
    }
}
