using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Utils;
using Mineral.Core.Capsule;
using Mineral.Core.Exception;
using Mineral.Core.Witness;
using static Mineral.Core.Capsule.BlockCapsule;

namespace Mineral.Core.Database
{
    public class Manager
    {
        #region Field
        private BlockStore block_store = null;
        private BlockIndexStore block_index_store = null;
        private AccountStore account_store = null;
        private WitnessStore witness_store = null;
        private WitnessScheduleStore witness_schedule_store = null;
        private VotesStore votes_store = null;
        private DynamicPropertiesStore dynamic_properties_store = null;
        private AssetIssueStore asset_issue_store = null;

        private KhaosDatabase khaos_database = null;
        private WitnessController witness_controller = null;

        private BlockCapsule genesis_block = null;
        #endregion


        #region Property
        public BlockStore Block => this.block_store;
        public BlockIndexStore BlockIndex => this.block_index_store;
        public AccountStore Account => this.account_store;
        public WitnessStore Witness => this.witness_store;
        public WitnessScheduleStore WitnessSchedule => this.witness_schedule_store;
        public VotesStore Votes => this.votes_store;
        public DynamicPropertiesStore DynamicProperties => this.dynamic_properties_store;
        public AssetIssueStore AssetIssue => this.asset_issue_store;

        public BlockCapsule GenesisBlock => this.genesis_block;

        public WitnessController WitnessController
        {
            get { return this.witness_controller; }
            set { this.witness_controller = value ; }
        }
        #endregion


        #region Constructor
        public Manager()
        {
            block_store = new BlockStore("block");
            block_index_store = new BlockIndexStore("block-index");
            account_store = new AccountStore(this, "account");
            witness_store = new WitnessStore("witness");
            witness_schedule_store = new WitnessScheduleStore("siwtness_schedule");
            votes_store = new VotesStore("votes");
            dynamic_properties_store = new DynamicPropertiesStore("properties");
            asset_issue_store = new AssetIssueStore("asset-issue");

            witness_controller = new WitnessController(this);

            khaos_database = new KhaosDatabase("block_KDB");
    }
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

        public void AdjustBalance(byte[] address, long amount)
        {
            AccountCapsule account = Account.GetUnchecked(address);
            AdjustBalance(account, amount);
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
            BlockCapsule block = this.khaos_database.GetBlock(hash);
            if (block == null)
            {
                block = this.block_store.Get(hash.Hash);
            }

            return block;
        }

        public BlockId GetBlockIdByNum(long num)
        {
            return this.block_index_store.Get(num);
        }

        public BlockCapsule GetBlockByNum(long num)
        {
            return GetBlockById(GetBlockIdByNum(num));
        }

        public bool LastHeadBlockIsMaintenance()
        {
            return DynamicProperties.GetStateFlag() == 1;
        }
        #endregion
    }
}
