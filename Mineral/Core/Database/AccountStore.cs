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
        private DatabaseManager db_manager = null;
        private FastSyncCallBack fast_sync_callback = null;
        private AccountStateStoreTrie account_state_store = null;
        private static Dictionary<string, byte[]> asserts_address = new Dictionary<string, byte[]>();
        #endregion


        #region Property
        #endregion


        #region Constructor
        public AccountStore(DatabaseManager db_manager,
                            AccountStateStoreTrie account_state,
                            FastSyncCallBack callback,
                            string db_name = "account")
            : base(db_name)
        {
            this.db_manager = db_manager;
            this.fast_sync_callback = callback;
            this.account_state_store = account_state;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public static void SetAccount(Config.Arguments.Args.GenesisBlockArgs args)
        {
            foreach (Config.Arguments.Account account in args.Assets)
            {
                asserts_address.Add(account.Name, Wallet.DecodeFromBase58Check(account.Address));
            }
        }

        public AccountCapsule GetBlackHole()
        {
            asserts_address.TryGetValue("Blockhole", out byte[] value);
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
            fast_sync_callback.AccountCallBack(key, item);
        }

        public override AccountCapsule Get(byte[] key)
        {
            byte[] value = this.revoking_db.GetUnchecked(key);
            return value.IsNotNullOrEmpty() ? new AccountCapsule(value) : null;
        }

        public override void Close()
        {
            base.Close();
            account_state_store.Close();
        }
        #endregion
    }
}
