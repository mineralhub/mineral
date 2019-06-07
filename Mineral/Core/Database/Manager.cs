using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Utils;
using Mineral.Core.Capsule;
using Mineral.Core.Exception;

namespace Mineral.Core.Database
{
    public class Manager
    {
        #region Field
        public BlockStore block_store;
        private AccountStore account_store;
        private DynamicPropertiesStore dynamic_properties_store = new DynamicPropertiesStore("properties");
        private AssetIssueStore asset_issue_store = new AssetIssueStore("asset-issue");
        #endregion


        #region Property
        public BlockStore Block => this.block_store;
        public AccountStore Account => this.account_store;
        public DynamicPropertiesStore DynamicProperties => this.dynamic_properties_store;
        public AssetIssueStore AssetIssue => this.asset_issue_store;
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public long GetHeadBlockTimestamp()
        {
            return this.dynamic_properties_store.GetLatestBlockHeaderTimestamp();
        }

        public void AdjustBalance(AccountCapsule account, long amount)
        {
            long balance = account.Balance;
            if (balance == 0)
                return;

            if (amount < 0 && balance < -amount)
            {
                throw new BalanceInsufficientException(account.Address.ToHexString() + " insufficient balance");
            }

            account.Balance += amount;
            this.account_store.Put(account.Address.ToByteArray(), account);
        }

        public BlockCapsule GetBlockById(SHA256Hash hash)
        {
            BlockCapsule block = this.
        }
        #endregion
    }
}
