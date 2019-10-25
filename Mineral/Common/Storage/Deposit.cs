using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Google.Protobuf;
using Mineral.Common.Runtime.VM;
using Mineral.Core;
using Mineral.Core.Capsule;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Database;
using Mineral.Cryptography;
using Mineral.Utils;

namespace Mineral.Common.Storage
{
    using VMConfig = Runtime.Config.VMConfig;
    using VMStorage = Runtime.VM.Program.Storage;

    public class Deposit : IDeposit
    {
        #region Field
        private static readonly byte[] LATEST_PROPOSAL_NUM = Encoding.UTF8.GetBytes("LATEST_PROPOSAL_NUM");
        private static readonly byte[] WITNESS_ALLOWANCE_FROZEN_TIME = Encoding.UTF8.GetBytes("WITNESS_ALLOWANCE_FROZEN_TIME");
        private static readonly byte[] MAINTENANCE_TIME_INTERVAL = Encoding.UTF8.GetBytes("MAINTENANCE_TIME_INTERVAL");
        private static readonly byte[] NEXT_MAINTENANCE_TIME = Encoding.UTF8.GetBytes("NEXT_MAINTENANCE_TIME");

        private DatabaseManager db_manager = null;
        private Deposit parent = null;

        private Dictionary<Key, Value> account_cache = new Dictionary<Key, Value>();
        private Dictionary<Key, Value> transaction_cache = new Dictionary<Key, Value>();
        private Dictionary<Key, Value> block_cache = new Dictionary<Key, Value>();
        private Dictionary<Key, Value> witness_cache = new Dictionary<Key, Value>();
        private Dictionary<Key, Value> code_cache = new Dictionary<Key, Value>();
        private Dictionary<Key, Value> contract_cache = new Dictionary<Key, Value>();

        private Dictionary<Key, Value> votes_cache = new Dictionary<Key, Value>();
        private Dictionary<Key, Value> proposal_cache = new Dictionary<Key, Value>();
        private Dictionary<Key, Value> dynamic_properties_cache = new Dictionary<Key, Value>();
        private Dictionary<Key, VMStorage> storage_cache = new Dictionary<Key, VMStorage>();
        private Dictionary<Key, Value> asset_issue_cache = new Dictionary<Key, Value>();
        #endregion


        #region Property
        public DatabaseManager DBManager
        {
            get { return this.db_manager; }
        }

        public Deposit Parent
        {
            get { return this.parent; }
            set { this.parent = value; }
        }
        #endregion


        #region Contructor
        protected Deposit(DatabaseManager db_manager, Deposit parent)
        {
            this.db_manager = db_manager;
            this.parent = parent;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private void CommitAccountCache(Deposit deposit)
        {
            foreach (var pair in this.account_cache)
            {
                if (pair.Value.Type.IsCreate || pair.Value.Type.IsDirty)
                {
                    if (deposit != null)
                    {
                        deposit.PutAccount(pair.Key, pair.Value);
                    }
                    else
                    {
                        this.db_manager.Account.Put(pair.Key.Data, pair.Value.ToCapsule<AccountCapsule, Protocol.Account>());
                    }
                }
            }
        }

        private void CommitTransactionCache(Deposit deposit)
        {
            foreach (var pair in this.transaction_cache)
            {
                if (pair.Value.Type.IsCreate || pair.Value.Type.IsDirty)
                {
                    if (deposit != null)
                    {
                        deposit.PutTransaction(pair.Key, pair.Value);
                    }
                    else
                    {
                        this.db_manager.Transaction.Put(pair.Key.Data, pair.Value.ToCapsule<TransactionCapsule, Protocol.Transaction>());
                    }
                }
            }
        }

        private void CommitBlockCache(Deposit deposit)
        {
            foreach (var pair in this.block_cache)
            {
                if (pair.Value.Type.IsCreate || pair.Value.Type.IsDirty)
                {
                    if (deposit != null)
                    {
                        deposit.PutBlock(pair.Key, pair.Value);
                    }
                    else
                    {
                        this.db_manager.Block.Put(pair.Key.Data, pair.Value.ToCapsule<BlockCapsule, Protocol.Block>());
                    }
                }
            }
        }

        private void CommitWitnessCache(Deposit deposit)
        {
            foreach (var pair in this.witness_cache)
            {
                if (pair.Value.Type.IsCreate || pair.Value.Type.IsDirty)
                {
                    if (deposit != null)
                    {
                        deposit.PutWitness(pair.Key, pair.Value);
                    }
                    else
                    {
                        this.db_manager.Witness.Put(pair.Key.Data, pair.Value.ToCapsule<WitnessCapsule, Protocol.Witness>());
                    }
                }
            }
        }

        private void CommitCodeCache(Deposit deposit)
        {
            foreach (var pair in this.witness_cache)
            {
                if (pair.Value.Type.IsCreate || pair.Value.Type.IsDirty)
                {
                    if (deposit != null)
                    {
                        deposit.PutCode(pair.Key, pair.Value);
                    }
                    else
                    {
                        this.db_manager.Code.Put(pair.Key.Data, pair.Value.ToCapsule<CodeCapsule, byte[]>());
                    }
                }
            }
        }

        private void CommitContractCache(Deposit deposit)
        {
            foreach (var pair in this.contract_cache)
            {
                if (pair.Value.Type.IsCreate || pair.Value.Type.IsDirty)
                {
                    if (deposit != null)
                    {
                        deposit.PutContract(pair.Key, pair.Value);
                    }
                    else
                    {
                        this.db_manager.Contract.Put(pair.Key.Data, pair.Value.ToCapsule<ContractCapsule, Protocol.SmartContract>());
                    }
                }
            }
        }

        private void CommitStorageCache(Deposit deposit)
        {
            foreach (var pair in this.storage_cache)
            {
                if (deposit != null)
                {
                    deposit.PutStorage(pair.Key, pair.Value);
                }
                else
                {
                    pair.Value.Commit();
                }
            }
        }

        private void CommitVoteCache(Deposit deposit)
        {
            foreach (var pair in this.votes_cache)
            {
                if (pair.Value.Type.IsCreate || pair.Value.Type.IsDirty)
                {
                    if (deposit != null)
                    {
                        deposit.PutVotes(pair.Key, pair.Value);
                    }
                    else
                    {
                        this.db_manager.Votes.Put(pair.Key.Data, pair.Value.ToCapsule<VotesCapsule, Protocol.Votes>());
                    }
                }
            }
        }

        private void CommitProposalCache(Deposit deposit)
        {
            foreach (var pair in this.proposal_cache)
            {
                if (pair.Value.Type.IsCreate || pair.Value.Type.IsDirty)
                {
                    if (deposit != null)
                    {
                        deposit.PutProposal(pair.Key, pair.Value);
                    }
                    else
                    {
                        this.db_manager.Proposal.Put(pair.Key.Data, pair.Value.ToCapsule<ProposalCapsule, Protocol.Proposal>());
                    }
                }
            }
        }

        private void CommitDynamicPropertiesCache(Deposit deposit)
        {
            foreach (var pair in this.dynamic_properties_cache)
            {
                if (pair.Value.Type.IsCreate || pair.Value.Type.IsDirty)
                {
                    if (deposit != null)
                    {
                        deposit.PutDynamicProperties(pair.Key, pair.Value);
                    }
                    else
                    {
                        this.db_manager.DynamicProperties.Put(pair.Key.Data, pair.Value.ToCapsule<BytesCapsule, object>());
                    }
                }
            }
        }
        #endregion


        #region External Method
        public static Deposit CreateRoot(DatabaseManager db_manager)
        {
            return new Deposit(db_manager, null);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public long AddBalance(byte[] address, long value)
        {
            AccountCapsule account = GetAccount(address) ?? CreateAccount(address, Protocol.AccountType.Normal);

            long balance = account.Balance;
            if (value == 0)
                return balance;

            if (value < 0 && balance < -value)
                throw new System.Exception(account.CreateDatabaseKey().ToHexString() + " insufficient balance");

            account.Balance = balance + value;

            this.account_cache.Put(
                new Key(address),
                Value.Create(account.Data, ValueType.VALUE_TYPE_DIRTY | this.account_cache[new Key(address)].Type.Type));

            return account.Balance;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public long AddTokenBalance(byte[] address, byte[] token_id, long value)
        {
            byte[] id = ByteUtil.StripLeadingZeroes(token_id);
            AccountCapsule account = GetAccount(address) ?? CreateAccount(address, Protocol.AccountType.Normal);

            account.AssetV2.TryGetValue(Encoding.UTF8.GetString(id), out long balance);
            if (value == 0)
                return balance;

            if (value < 0 && balance < -value)
                throw new System.Exception(account.CreateDatabaseKey().ToHexString() + " insufficient balance");

            if (value >= 0)
                account.AddAssetAmountV2(id, value, this.db_manager);
            else
                account.ReduceAssetAmountV2(id, -value, this.db_manager);

            this.account_cache.Put(
                new Key(address),
                Value.Create(account.Data, ValueType.VALUE_TYPE_DIRTY | this.account_cache[new Key(address)].Type.Type));

            account.AssetV2.TryGetValue(Encoding.UTF8.GetString(id), out long result);

            return result;

        }

        public void Commit()
        {
            Deposit deposit = null;
            if (parent != null)
            {
                deposit = parent;
            }

            CommitAccountCache(deposit);
            CommitTransactionCache(deposit);
            CommitBlockCache(deposit);
            CommitWitnessCache(deposit);
            CommitCodeCache(deposit);
            CommitContractCache(deposit);
            CommitStorageCache(deposit);
            CommitVoteCache(deposit);
            CommitProposalCache(deposit);
            CommitDynamicPropertiesCache(deposit);
        }

        public AccountCapsule CreateAccount(byte[] address, Protocol.AccountType type)
        {
            AccountCapsule account = new AccountCapsule(ByteString.CopyFrom(address), type);
            this.account_cache.Add(new Key(address), new Value(account.Data, ValueType.VALUE_TYPE_CREATE));

            return account;
        }

        public AccountCapsule CreateAccount(byte[] address, string account_name, Protocol.AccountType type)
        {
            AccountCapsule account = new AccountCapsule(
                                            ByteString.CopyFrom(address),
                                            ByteString.CopyFromUtf8(account_name),
                                            type);

            this.account_cache.Add(new Key(address), new Value(account.Data, ValueType.VALUE_TYPE_CREATE));

            return account;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void CreateContract(byte[] address, ContractCapsule contract)
        {
            this.contract_cache.Add(new Key(address), Value.Create(contract.Data, ValueType.VALUE_TYPE_CREATE));
        }

        public void UpdateContract(byte[] address, ContractCapsule contract)
        {
            this.contract_cache.Put(new Key(address), Value.Create(contract.Data, ValueType.VALUE_TYPE_DIRTY));
        }

        public void DeleteContract(byte[] address)
        {
            this.db_manager.Code.Delete(address);
            this.db_manager.Account.Delete(address);
            this.db_manager.Contract.Delete(address);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public AccountCapsule GetAccount(byte[] address)
        {
            AccountCapsule result = null;
            Key key = new Key(address);

            if (this.account_cache.ContainsKey(key))
            {
                result = this.account_cache[key].ToCapsule<AccountCapsule, Protocol.Account>();
            }
            else
            {
                if (this.parent != null)
                    result = this.parent.GetAccount(address);
                else
                    result = this.db_manager.Account.Get(address);

                if (result != null)
                    this.account_cache.Add(key, Value.Create(result.Data));
            }

            return result;
        }

        public AssetIssueCapsule GetAssetIssue(byte[] token_id)
        {
            AssetIssueCapsule asset_issue = null;
            byte[] id = ByteUtil.StripLeadingZeroes(token_id);
            Key key = new Key(id);

            if (this.asset_issue_cache.ContainsKey(key))
            {
                asset_issue = this.asset_issue_cache[key].ToCapsule<AssetIssueCapsule, Protocol.AssetIssueContract>();
            }
            else
            {
                if (this.parent != null)
                    asset_issue = this.parent.GetAssetIssue(id);
                else
                    asset_issue = this.db_manager.AssetIssue.Get(id);

                if (asset_issue != null)
                    this.asset_issue_cache.Add(key, Value.Create(asset_issue.Data));
            }

            return asset_issue;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public long GetBalance(byte[] address)
        {
            AccountCapsule account = GetAccount(address);
            return account != null ? account.Balance : 0;
        }

        public byte[] GetBlackHoleAddress()
        {
            return this.db_manager.Account.GetBlackHole().Address.ToByteArray();
        }

        public BlockCapsule GetBlock(byte[] hash)
        {
            BlockCapsule block = null;
            Key key = Key.Create(hash);

            if (this.block_cache.ContainsKey(key))
            {
                block = this.block_cache[key].ToCapsule<BlockCapsule, Protocol.Block>();
            }
            else
            {
                try
                {
                    if (this.parent != null)
                        block = this.parent.GetBlock(hash);
                    else
                        block = this.db_manager.Block.Get(hash);
                }
                catch
                {
                    block = null;
                }

                if (block != null)
                    this.block_cache.Add(key, Value.Create(block.Data));
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public byte[] GetCode(byte[] address)
        {
            byte[] result = null;
            Key key = new Key(address);

            if (this.code_cache.ContainsKey(key))
            {
                result = this.code_cache[key].ToCapsule<CodeCapsule, byte[]>().Data;
            }
            else
            {
                if (this.parent != null)
                {
                    result = this.parent.GetCode(address);
                }
                else
                {
                    result = this.db_manager.Code.Get(address)?.Data;

                    if (result != null)
                        this.code_cache.Add(key, Value.Create(result));
                }
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public ContractCapsule GetContract(byte[] address)
        {
            ContractCapsule result = null;
            Key key = new Key(address);

            if (this.proposal_cache.ContainsKey(key))
            {
                result = this.contract_cache[key].ToCapsule<ContractCapsule, Protocol.SmartContract>();
            }
            else
            {
                if (this.parent != null)
                    result = this.parent.GetContract(address);
                else
                    result = this.db_manager.Contract.Get(address);

                if (result != null)
                    this.contract_cache.Add(key, Value.Create(result.Data));
            }

            return result;
        }

        public BytesCapsule GetDynamic(byte[] dynamic_key)
        {
            BytesCapsule result = null;
            Key key = new Key(dynamic_key);

            if (this.dynamic_properties_cache.ContainsKey(key))
            {
                result = this.dynamic_properties_cache[key].ToCapsule<BytesCapsule, object>();
            }
            else
            {
                if (this.parent != null)
                {
                    result = this.parent.GetDynamic(dynamic_key);
                }
                else
                {
                    try
                    {
                        result = this.db_manager.DynamicProperties.Get(dynamic_key);
                    }
                    catch
                    {
                        Logger.Warning("Not found dynamic property : " + dynamic_key.ToString());
                        result = null;
                    }
                }

                if (result != null)
                    this.dynamic_properties_cache.Add(key, Value.Create(result.Data));
            }

            return result;
        }

        public long GetLatestProposalNum()
        {
            return BitConverter.ToInt64(GetDynamic(LATEST_PROPOSAL_NUM).Data, 0);
        }

        public long GetMaintenanceTimeInterval()
        {
            return BitConverter.ToInt64(GetDynamic(MAINTENANCE_TIME_INTERVAL).Data, 0);
        }

        public long GetNextMaintenanceTime()
        {
            return BitConverter.ToInt64(GetDynamic(NEXT_MAINTENANCE_TIME).Data, 0);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public ProposalCapsule GetProposalCapsule(byte[] id)
        {
            ProposalCapsule result = null;
            Key key = new Key(id);

            if (this.proposal_cache.ContainsKey(key))
            {
                result = this.proposal_cache[key].ToCapsule<ProposalCapsule, Protocol.Proposal>();
            }
            else
            {
                if (this.parent != null)
                    result = this.parent.GetProposalCapsule(id);
                else
                    result = this.db_manager.Proposal.Get(id);

                if (result != null)
                    this.proposal_cache.Add(key, Value.Create(result.Data));
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public VMStorage GetStorage(byte[] address)
        {
            VMStorage result = null;
            Key key = new Key(address);

            if (this.storage_cache.ContainsKey(key))
            {
                result = this.storage_cache[key];
            }
            else
            {
                if (this.parent != null)
                {
                    VMStorage parent_storage = this.parent.GetStorage(address);
                    result = VMConfig.EnergyLimitHardFork ? new VMStorage(parent_storage) : parent_storage;
                }
                else
                {
                    result = new VMStorage(address, this.db_manager.StorageRow);
                }
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public DataWord GetStorageValue(byte[] address, DataWord key)
        {
            DataWord result = null;
            address = Wallet.ToAddAddressPrefix(address);
            if (GetAccount(address) != null)
            {
                Key address_key = new Key(address);
                VMStorage storage = null;

                if (this.storage_cache.ContainsKey(address_key))
                {
                    storage = this.storage_cache[address_key];
                }
                else
                {
                    storage = GetStorage(address);
                    this.storage_cache.Add(address_key, storage);
                }
                result = storage.Get(key);
            }

            return result;
        }

        public long GetTokenBalance(byte[] address, byte[] token_id)
        {
            long result = 0;
            AccountCapsule account = GetAccount(address);

            if (account != null)
            {
                string token = Encoding.UTF8.GetString(ByteUtil.StripLeadingZeroes(token_id));
                account.AssetV2.TryGetValue(token, out result);
            }

            return result;
        }

        public TransactionCapsule GetTransaction(byte[] hash)
        {
            TransactionCapsule transaction = null;
            Key key = new Key(hash);

            if (this.transaction_cache.ContainsKey(key))
            {
                transaction = this.transaction_cache[key].ToCapsule<TransactionCapsule, Protocol.Transaction>();
            }
            else
            {
                if (this.parent != null)
                {
                    transaction = this.parent.GetTransaction(hash);
                }
                else
                {
                    try
                    {
                        transaction = this.db_manager.Transaction.Get(hash);
                    }
                    catch
                    {
                        transaction = null;
                    }
                }

                if (transaction != null)
                    this.transaction_cache.Add(key, Value.Create(transaction.Data));
            }

            return transaction;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public VotesCapsule GetVotesCapsule(byte[] address)
        {
            VotesCapsule result = null;
            Key key = new Key(address);

            if (this.votes_cache.ContainsKey(key))
            {
                result = this.votes_cache[key].ToCapsule<VotesCapsule, Protocol.Votes>();
            }
            else
            {
                if (this.parent != null)
                    result = this.parent.GetVotesCapsule(address);
                else
                    result = this.db_manager.Votes.Get(address);

                if (result != null)
                    this.votes_cache.Add(key, Value.Create(result.Data));
            }

            return result;
        }

        public WitnessCapsule GetWitness(byte[] address)
        {
            WitnessCapsule result = null;
            Key key = new Key(address);

            if (this.witness_cache.ContainsKey(key))
            {
                result = this.witness_cache[key].ToCapsule<WitnessCapsule, Protocol.Witness>();
            }
            else
            {
                if (this.parent != null)
                    result = this.parent.GetWitness(address);
                else
                    result = this.db_manager.Witness.Get(address);

                if (result != null)
                    this.witness_cache.Add(key, Value.Create(result.Data));
            }

            return result;
        }

        public long GetWitnessAllowanceFrozenTime()
        {
            long result = 0;
            byte[] frozen_time = GetDynamic(WITNESS_ALLOWANCE_FROZEN_TIME).Data;

            if (frozen_time.Length >= 8)
            {
                result = BitConverter.ToInt64(GetDynamic(WITNESS_ALLOWANCE_FROZEN_TIME).Data, 0);
            }
            else
            {
                byte[] frozen = new byte[8];
                Array.Copy(frozen_time, 0, frozen, 8 - frozen_time.Length, frozen_time.Length);
                result = BitConverter.ToInt64(frozen, 0);
            }

            return result;
        }

        public IDeposit NewDepositChild()
        {
            return new Deposit(this.db_manager, this);
        }

        public void PutAccount(Key key, Value value)
        {
            this.account_cache.Put(key, value);
        }

        public void PutAccountValue(byte[] address, AccountCapsule account)
        {
            this.account_cache.Put(
                new Key(address),
                new Value(account.Data, ValueType.VALUE_TYPE_CREATE));

        }

        public void PutBlock(Key key, Value value)
        {
            this.block_cache.Put(key, value);
        }

        public void PutCode(Key key, Value value)
        {
            this.code_cache.Put(key, value);
        }

        public void PutContract(Key key, Value value)
        {
            this.contract_cache.Put(key, value);
        }

        public void PutDynamicProperties(Key key, Value value)
        {
            this.dynamic_properties_cache.Put(key, value);
        }

        public void PutDynamicPropertiesWithLatestProposalNum(long num)
        {
            this.dynamic_properties_cache.Put(
                new Key(LATEST_PROPOSAL_NUM),
                new Value(new BytesCapsule(BitConverter.GetBytes(num)).Data, ValueType.VALUE_TYPE_CREATE));
        }

        public void PutProposal(Key key, Value value)
        {
            this.proposal_cache.Put(key, value);
        }

        public void PutProposalValue(byte[] address, ProposalCapsule proposal)
        {
            this.proposal_cache.Put(
                new Key(address),
                new Value(proposal.Data, ValueType.VALUE_TYPE_CREATE));
        }

        public void PutStorage(Key key, VMStorage cache)
        {
            this.storage_cache.Put(key, cache);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void PutStorageValue(byte[] address, DataWord key, DataWord value)
        {
            address = Wallet.ToAddAddressPrefix(address);
            if (GetAccount(address) != null)
            {
                Key address_key = new Key(address);
                VMStorage storage = null;

                if (this.storage_cache.ContainsKey(address_key))
                {
                    storage = this.storage_cache[address_key];
                }
                else
                {
                    storage = GetStorage(address);
                    this.storage_cache.Add(address_key, storage);
                }
                storage.Put(key, value);
            }
        }

        public void PutTransaction(Key key, Value value)
        {
            this.transaction_cache.Put(key, value);
        }

        public void PutVotes(Key key, Value value)
        {
            this.votes_cache.Put(key, value);
        }

        public void PutVoteValue(byte[] address, VotesCapsule votes)
        {
            this.votes_cache.Put(
                new Key(address),
                new Value(votes.Data, ValueType.VALUE_TYPE_CREATE));
        }

        public void PutWitness(Key key, Value value)
        {
            this.witness_cache.Put(key, value);
        }

        public void SaveCode(byte[] address, byte[] code)
        {
            this.code_cache.Put(new Key(address), Value.Create(code, ValueType.VALUE_TYPE_CREATE));

            if (VMConfig.AllowTvmConstantinople)
            {
                ContractCapsule contract = GetContract(address);
                contract.CodeHash = Hash.SHA3(code);

                UpdateContract(address, contract);
            }
        }

        public void SetParent(Deposit deposit)
        {
            this.parent = deposit;
        }
        #endregion

    }
}
