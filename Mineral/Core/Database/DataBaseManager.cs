using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Mineral.Common.LogsFilter;
using Mineral.Common.Overlay.Discover.Node;
using Mineral.Common.Utils;
using Mineral.Core.Capsule;
using Mineral.Core.Config;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Database.Api;
using Mineral.Core.Database.Fast.Callback;
using Mineral.Core.Database2.Common;
using Mineral.Core.Database2.Core;
using Mineral.Core.Exception;
using Mineral.Core.Service;
using Mineral.Core.Witness;
using Mineral.Utils;
using Protocol;
using static Mineral.Core.Capsule.BlockCapsule;
using static Protocol.Transaction.Types;
using static Protocol.Transaction.Types.Contract.Types;

namespace Mineral.Core.Database
{
    public class DatabaseManager
    {
        #region Field
        private IRevokingDatabase revoking_store = null;
        private KhaosDatabase khaos_database = null;
        private BlockStore block_store = null;
        private RecentBlockStore recent_block_store = null;
        private BlockIndexStore block_index_store = null;
        private TransactionStore transaction_store = null;
        private TransactionCache transaction_cache = null;
        private TransactionHistoryStore transaction_history_store = null;
        private AccountStore account_store = null;
        private AccountIndexStore account_index_store = null;
        private AccountIdIndexStore account_id_index_store = null;
        private WitnessStore witness_store = null;
        private WitnessScheduleStore witness_schedule_store = null;
        private VotesStore votes_store = null;
        private ProposalStore proposal_store = null;
        private AssetIssueStore asset_issue_store = null;
        private AssetIssueV2Store asset_issue_v2_store = null;
        private ExchangeStore exchange_store = null;
        private ExchangeV2Store exchange_v2_store = null;
        private CodeStore code_store = null;
        private ContractStore contract_store = null;
        private StorageRowStore storage_row_store = null;
        private DelegatedResourceStore delegated_resource_store = null;
        private DelegatedResourceAccountIndexStore delegate_resource_Account_index_store;
        private DynamicPropertiesStore dynamic_properties_store = null;
        private PeerStore peer_store = null;

        private ForkController fork_controller = ForkController.Instance;
        private WitnessController witness_controller = null;
        private ProposalController proposal_controller = null;
        private WitnessService witness_service = null;
        private BlockCapsule genesis_block = null;

        private SessionOptional session = SessionOptional.Instance;
        private BlockingCollection<TransactionCapsule> pending_transactions = new BlockingCollection<TransactionCapsule>();
        private BlockingCollection<TransactionCapsule> pop_transactions = new BlockingCollection<TransactionCapsule>();
        private ConcurrentQueue<TransactionCapsule> push_transactions = new ConcurrentQueue<TransactionCapsule>();
        private ConcurrentQueue<TransactionCapsule> repush_transactions = new ConcurrentQueue<TransactionCapsule>();

        private MemoryCache transaction_id_cache = MemoryCache.Default;
        private HashSet<string> owner_addresses = new HashSet<string>();
        private long latest_solidified_block_number = 0;
        #endregion


        #region Property
        public IRevokingDatabase RevokeStore => this.revoking_store;
        public BlockStore Block => this.block_store;
        public BlockIndexStore BlockIndex => this.block_index_store;
        public TransactionStore Transaction => this.transaction_store;
        public AccountStore Account => this.account_store;
        public AccountIndexStore AccountIndex => this.account_index_store;
        public AccountIdIndexStore AccountIdIndex => this.account_id_index_store;
        public WitnessStore Witness => this.witness_store;
        public WitnessScheduleStore WitnessSchedule => this.witness_schedule_store;
        public VotesStore Votes => this.votes_store;
        public ProposalStore Proposal => this.proposal_store;
        public AssetIssueStore AssetIssue => this.asset_issue_store;
        public AssetIssueStore AssetIssueV2 => this.asset_issue_v2_store;
        public ExchangeStore Exchange => this.exchange_store;
        public ExchangeStore ExchangeFinal => this.dynamic_properties_store.GetAllowSameTokenName() == 0 ? this.exchange_store : this.exchange_v2_store;
        public ExchangeV2Store ExchangeV2 => this.exchange_v2_store;
        public CodeStore Code => this.code_store;
        public ContractStore Contract => this.contract_store;
        public StorageRowStore StorageRow => this.storage_row_store;
        public DelegatedResourceStore DelegatedResource => this.delegated_resource_store;
        public DelegatedResourceAccountIndexStore DelegateResourceAccountIndex => this.delegate_resource_Account_index_store;
        public DynamicPropertiesStore DynamicProperties => this.dynamic_properties_store;

        public BlockCapsule GenesisBlock => this.genesis_block;

        public BlockId GenesisBlockId
        {
            get { return this.genesis_block != null ? this.genesis_block.Id : null; }
        }

        public BlockId SolidBlockId
        {
            get
            {
                try
                {
                    long num = this.dynamic_properties_store.GetLatestSolidifiedBlockNum();
                    return GetBlockByNum(num)?.Id;
                }
                catch
                {
                    return GenesisBlock?.Id;
                }
            }
        }

        public BlockId HeadBlockId
        {
            get
            {
                return new BlockId(this.dynamic_properties_store.GetLatestBlockHeaderHash(),
                                   this.dynamic_properties_store.GetLatestBlockHeaderNumber());
            }
        }

        public ForkController ForkController
        {
            get { return this.fork_controller; }
        }

        public WitnessController WitnessController
        {
            get { return this.witness_controller; }
            set { this.witness_controller = value; }
        }

        public WitnessService WitnessService
        {
            get { return this.witness_service; }
            set { this.witness_service = value; }
        }

        public BlockingCollection<TransactionCapsule> PendingTransactions
        {
            get { return this.pending_transactions; }
        }

        public BlockingCollection<TransactionCapsule> PopTransactions
        {
            get { return this.pop_transactions; }
        }

        public ConcurrentQueue<TransactionCapsule> RePushTransactions
        {
            get { return this.repush_transactions; }
        }

        public SessionOptional Session
        {
            get { return this.session; }
        }

        public bool HasBlock
        {
            get { return this.block_store.GetEnumerator().MoveNext() || !this.khaos_database.IsEmpty; }
        }

        public bool IsNeedToUpdateAsset
        {
            get { return this.dynamic_properties_store.GetTokenUpdateDone() == 0; }
        }

        public bool IsGeneratingBlock
        {
            get { return Args.Instance.IsWitness ? this.witness_controller.IsGeneratingBlock : false; }
        }

        public bool IsRunRepushThread { get; set; } = true;
        public bool IsEventPluginLoaded { get; set; }
        #endregion


        #region Constructor
        public DatabaseManager()
        {
            this.revoking_store = new SnapshotManager();
            this.khaos_database = new KhaosDatabase("block_KDB");

            this.block_store = new BlockStore("block");
            this.block_index_store = new BlockIndexStore("block-index");
            this.recent_block_store = new RecentBlockStore("recent-block");
            this.transaction_store = new TransactionStore(this.block_store, this.khaos_database, "transaction");
            this.transaction_cache = new TransactionCache("trans-cache");
            this.transaction_history_store = new TransactionHistoryStore("transaction_history_store");
            this.account_store = new AccountStore("account");
            this.account_index_store = new AccountIndexStore("account-index");
            this.account_id_index_store = new AccountIdIndexStore("accountid-index");
            this.witness_store = new WitnessStore("witness");
            this.witness_schedule_store = new WitnessScheduleStore("siwtness_schedule");
            this.votes_store = new VotesStore("votes");
            this.proposal_store = new ProposalStore("proposal");
            this.asset_issue_store = new AssetIssueStore("asset-issue");
            this.asset_issue_v2_store = new AssetIssueV2Store("asset-issue-v2");
            this.exchange_store = new ExchangeStore("exchange");
            this.exchange_v2_store = new ExchangeV2Store("exchange-v2");
            this.code_store = new CodeStore("code");
            this.contract_store = new ContractStore("contract");
            this.storage_row_store = new StorageRowStore("storage-row");
            this.delegated_resource_store = new DelegatedResourceStore("delegate-resource");
            this.delegate_resource_Account_index_store = new DelegatedResourceAccountIndexStore("delegate-resource-account-index");
            this.dynamic_properties_store = new DynamicPropertiesStore("properties");
            this.peer_store = new PeerStore();

            this.witness_controller = new WitnessController(this);
            this.proposal_controller = new ProposalController(this);
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private void InitGenesis()
        {
            this.genesis_block = BlockCapsule.GenerateGenesisBlock();

            // TODO : InitAccount, InitWitness 판단 추가해야함
            if (!ContainBlock(this.genesis_block.Id))
            {
                if (HasBlock)
                {
                    Logger.Error(
                        string.Format(
                            "Genesis block modify, please delete database directory({0}) and restart",
                            Args.Instance.GetOutputDirectory()));

                    Environment.Exit(0);
                }
                else
                {
                    Logger.Info("Create genesis block");

                    this.block_store.Put(this.genesis_block.Id.Hash, this.genesis_block);
                    this.block_index_store.Put(this.genesis_block.Id);

                    Logger.Info("Save block: " + this.genesis_block.ToString());

                    this.dynamic_properties_store.PutLatestBlockHeaderNumber(0);
                    this.dynamic_properties_store.PutLatestBlockHeaderHash(ByteString.CopyFrom(this.genesis_block.Id.Hash));
                    this.dynamic_properties_store.PutLatestBlockHeaderTimestamp(this.genesis_block.Timestamp);

                    InitAccount();
                    InitWitness();
                    this.witness_controller.InitializeWitness();
                    this.khaos_database.Start(this.genesis_block);
                    UpdateRecentBlock(this.genesis_block);
                }
            }
        }

        private void InitAccount()
        {
            Args.Instance.GenesisBlock.Assets.ForEach(asset =>
            {
                byte[] name = Encoding.UTF8.GetBytes(asset.Name);
                byte[] address = Wallet.Base58ToAddress(asset.Address);

                asset.Type = AccountType.Normal;
                AccountCapsule account = new AccountCapsule(ByteString.CopyFrom(name),
                                                            ByteString.CopyFrom(address),
                                                            asset.Type,
                                                            asset.Balance);

                this.account_store.Put(address, account);
                this.account_id_index_store.Put(account);
                this.account_index_store.Put(account);
            });
        }

        private void InitWitness()
        {
            Args.Instance.GenesisBlock.Witnesses.ForEach(w =>
            {
                AccountCapsule account = null;
                ByteString address = ByteString.CopyFrom(w.Address);

                if (!this.account_store.Contains(w.Address))
                {
                    account = new AccountCapsule(ByteString.Empty, address, AccountType.AssetIssue, 0);
                }
                else
                {
                    account = this.account_store.GetUnchecked(w.Address);
                }

                account.IsWitness = true;
                this.account_store.Put(w.Address, account);

                WitnessCapsule witness = new WitnessCapsule(address, w.VoteCount, w.Url);
                witness.IsJobs = true;
                this.witness_store.Put(w.Address, witness);

            });
        }

        private void InitCacheTransactions()
        {
            Logger.Info("Begin to init transactions cache.");

            long start = Helper.CurrentTimeMillis();
            long head_num = this.dynamic_properties_store.GetLatestBlockHeaderNumber();
            long recent_block_count = this.recent_block_store.Count();

            long block_count = 0;
            long empty_block_count = 0;

            for (long i = head_num - recent_block_count + 1; i < head_num; i++)
            {
                try
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            Interlocked.Increment(ref block_count);
                            BlockCapsule block = GetBlockByNum(i);
                            if (block.Transactions.IsNullOrEmpty())
                            {
                                Interlocked.Increment(ref empty_block_count);
                            }

                            foreach (TransactionCapsule tx in block.Transactions)
                            {
                                this.transaction_cache.Put(tx.Id.Hash, new BytesCapsule(BitConverter.GetBytes(i)));
                            }
                        }
                        catch (ItemNotFoundException e)
                        {
                            Logger.Info("initilaize transaction cache error.");
                            throw new IllegalStateException("initilaize transaction cache error.");

                        }
                        catch (BadItemException e)
                        {
                            Logger.Info("initilaize transaction cache error.");
                            throw new IllegalStateException("initilaize transaction cache error.");
                        }

                    }).Wait();
                }
                catch (System.Exception e)
                {
                    Logger.Info(e.Message);
                }
            }

            Logger.Info(
                string.Format(
                    "end to init txs cache. trxids:{0}, block count:{1}, empty block count:{2}, cost:{3}",
                    this.transaction_cache.Count(),
                    Interlocked.Read(ref block_count),
                    Interlocked.Read(ref empty_block_count),
                    Helper.CurrentTimeMillis() - start
            ));
        }

        private void RePushLoop()
        {
            TransactionCapsule transaction = null;

            while (IsRunRepushThread)
            {
                try
                {
                    if (IsGeneratingBlock)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    this.repush_transactions.TryPeek(out transaction);
                    if (transaction != null)
                    {
                        RePush(transaction);
                    }
                    else
                    {
                        Thread.Sleep(50);
                    }
                }
                catch (System.Exception e)
                {
                    Logger.Error("unknown exception happened in repush loop", e);
                }
                finally
                {

                    if (transaction != null)
                    {
                        this.repush_transactions.Remove(transaction);
                    }
                }
            }
        }

        public void RePush(TransactionCapsule transaction)
        {
            if (ContainsTransaction(transaction))
                return;

            try
            {
                PushTransaction(transaction);
            }
            catch (System.Exception e)
            {
                if (e is ValidateSignatureException
                    || e is ContractValidateException
                    || e is ContractExeException
                    || e is AccountResourceInsufficientException
                    || e is VMIllegalException)
                {
                    Logger.Debug(e.Message);
                }
                else if (e is DupTransactionException)
                {
                    Logger.Debug("pending manager: dup trans");
                }
                else if (e is TaposException)
                {
                    Logger.Debug("pending manager: tapos exception");
                }
                else if (e is TooBigTransactionException)
                {
                    Logger.Debug("too big transaction");
                }
                else if (e is TransactionExpirationException)
                {
                    Logger.Debug("expiration transaction");
                }
                else if (e is ReceiptCheckErrorException)
                {
                    Logger.Debug("outOfSlotTime transaction");
                }
                else if (e is TooBigTransactionResultException)
                {
                    Logger.Debug("too big transaction result");
                }
            }
        }

        private bool IsMultSignTransaction(Transaction transaction)
        {
            bool result = false;

            switch (transaction.RawData.Contract[0].Type)
            {
                case ContractType.AccountPermissionUpdateContract:
                    {
                        result = true;
                    }
                    break;
                default:
                    {
                        result = false;
                    }
                    break;
            }

            return result;
        }

        private bool ContainsTransaction(TransactionCapsule transaction)
        {
            if (this.transaction_cache != null)
            {
                return this.transaction_cache.Contains(transaction.Id.Hash);
            }

            return this.transaction_store.Contains(transaction.Id.Hash);
        }

        private bool ProcessPenddingTransaction(TransactionCapsule transaction,
                                                long when,
                                                BlockCapsule block,
                                                HashSet<string> accounts,
                                                long postponed_tx_Count,
                                                bool is_pendding_transaction)
        {
            if (Helper.CurrentTimeMillis() - when
                > Parameter.ChainParameters.BLOCK_PRODUCED_INTERVAL * 0.5 * Args.Instance.Node.BlockProducedTimeout / 100)
            {
                Logger.Warning("Processing transaction time exceeds the 50% producing time。");
                return false;
            }

            if ((block.Instance.CalculateSize() + transaction.Instance.CalculateSize() + 3) > Parameter.ChainParameters.BLOCK_SIZE)
            {
                postponed_tx_Count++;
                return true;
            }

            Contract contract = transaction.Instance.RawData.Contract[0];
            byte[] owner = TransactionCapsule.GetOwner(contract);
            string owner_address = owner.ToHexString();

            if (accounts.Contains(owner_address))
            {
                return true;
            }
            else
            {
                if (IsMultSignTransaction(transaction.Instance))
                {
                    accounts.Add(owner_address);
                }
            }

            if (this.owner_addresses.Contains(owner_address))
            {
                transaction.IsVerified = false;
            }

            try
            {
                ISession temp_session = this.revoking_store.BuildSession();

                Manager.Instance.FastSyncCallback.PreExecuteTrans();
                ProcessTransaction(transaction, block);
                Manager.Instance.FastSyncCallback.ExecuteTransFinish();

                temp_session.Merge();
                block.AddTransaction(transaction);
                if (is_pendding_transaction)
                {
                    this.pending_transactions.TryTake(out _);
                }
            }
            catch (ContractExeException e)
            {
                Logger.Info("contract not processed during execute");
                Logger.Debug(e.Message);
            }
            catch (ContractValidateException e)
            {
                Logger.Info("contract not processed during validate");
                Logger.Debug(e.Message);
            }
            catch (TaposException e)
            {
                Logger.Info("contract not processed during TaposException");
                Logger.Debug(e.Message);
            }
            catch (DupTransactionException e)
            {
                Logger.Info("contract not processed during DupTransactionException");
                Logger.Debug(e.Message);
            }
            catch (TooBigTransactionException e)
            {
                Logger.Info("contract not processed during TooBigTransactionException");
                Logger.Debug(e.Message);
            }
            catch (TooBigTransactionResultException e)
            {
                Logger.Info("contract not processed during TooBigTransactionResultException");
                Logger.Debug(e.Message);
            }
            catch (TransactionExpirationException e)
            {
                Logger.Info("contract not processed during TransactionExpirationException");
                Logger.Debug(e.Message);
            }
            catch (AccountResourceInsufficientException e)
            {
                Logger.Info("contract not processed during AccountResourceInsufficientException");
                Logger.Debug(e.Message);
            }
            catch (ValidateSignatureException e)
            {
                Logger.Info("contract not processed during ValidateSignatureException");
                Logger.Debug(e.Message);
            }
            catch (ReceiptCheckErrorException e)
            {
                Logger.Info("OutOfSlotTime exception : " + e.Message);
                Logger.Debug(e.Message);
            }
            catch (VMIllegalException e)
            {
                Logger.Warning(e.Message);
            }

            return true;
        }

        private void ProcessMaintenance(BlockCapsule block)
        {
            this.proposal_controller.ProcessProposals();
            this.witness_controller.UpdateWitness();
            this.dynamic_properties_store.UpdateNextMaintenanceTime(block.Timestamp);
            this.fork_controller.Reset();
        }

        private void PostContractTrigger(TransactionTrace trace, bool remove)
        {
            // TODO : EventPluginLoader is not Implementation
            //if (IsEventPluginLoaded
            //    && (EventPluginLoader.getInstance().isContractEventTriggerEnable()
            //        || EventPluginLoader.getInstance().isContractLogTriggerEnable()))
            //{
            //    for (ContractTrigger trigger : trace.getRuntimeResult().getTriggerList())
            //    {
            //        ContractTriggerCapsule contractEventTriggerCapsule = new ContractTriggerCapsule(trigger);
            //        contractEventTriggerCapsule.getContractTrigger().setRemoved(remove);
            //        contractEventTriggerCapsule.setLatestSolidifiedBlockNumber(latestSolidifiedBlockNumber);
            //        if (!triggerCapsuleQueue.offer(contractEventTriggerCapsule))
            //        {
            //            logger.info("too many tigger, lost contract log trigger: {}", trigger.getTransactionId());
            //        }
            //    }
            //}
        }

        private void ReorgContractTrigger()
        {
            // TODO : EventPluginLoader is not Implementation
            //if (IsEventPluginLoaded &&
            //    (EventPluginLoader.getInstance().isContractEventTriggerEnable()
            //        || EventPluginLoader.getInstance().isContractLogTriggerEnable()))
            //{
            //    Logger.Info("switchfork occured, post reorgContractTrigger");
            //    try
            //    {
            //        BlockCapsule oldHeadBlock = getBlockById(
            //            getDynamicPropertiesStore().getLatestBlockHeaderHash());
            //        for (TransactionCapsule trx : oldHeadBlock.getTransactions())
            //        {
            //            postContractTrigger(trx.getTrxTrace(), true);
            //        }
            //    }
            //    catch (BadItemException | ItemNotFoundException e) {
            //        logger.error("block header hash not exists or bad: {}",
            //            getDynamicPropertiesStore().getLatestBlockHeaderHash());
            //    }
            //    }
            //}
        }

        private void PostBlockTrigger(BlockCapsule block)
        {
            // TODO : EventPluginLoader is not Implementation
            //if (IsEventPluginLoaded && EventPluginLoader.getInstance().isBlockLogTriggerEnable())
            //{
            //    BlockLogTriggerCapsule blockLogTriggerCapsule = new BlockLogTriggerCapsule(block);
            //    blockLogTriggerCapsule.setLatestSolidifiedBlockNumber(latestSolidifiedBlockNumber);
            //    boolean result = triggerCapsuleQueue.offer(blockLogTriggerCapsule);
            //    if (!result)
            //    {
            //        logger.info("too many trigger, lost block trigger: {}", block.getBlockId());
            //    }
            //}

            //for (TransactionCapsule e : block.getTransactions())
            //{
            //    postTransactionTrigger(e, block);
            //}
        }

        private void ApplyBlock(BlockCapsule block)
        {
            ProcessBlock(block);

            this.block_store.Put(block.Id.Hash, block);
            this.block_index_store.Put(block.Id);
            this.fork_controller.Update(block);

            if (Helper.CurrentTimeMillis() - block.Timestamp >= 60_000)
            {
                this.revoking_store.MaxFlushCount = SnapshotManager.DEFAULT_MAX_FLUSH_COUNT;
            }
            else
            {
                this.revoking_store.MaxFlushCount = SnapshotManager.DEFAULT_MIN_FLUSH_COUNT;
            }
        }

        private void SwitchFork(BlockCapsule new_head)
        {
            KeyValuePair<List<KhaosBlock>, List<KhaosBlock>> block_tree;

            try
            {
                block_tree = this.khaos_database.GetBranch(new_head.Id, this.dynamic_properties_store.GetLatestBlockHeaderHash());
            }
            catch (NonCommonBlockException e)
            {
                Logger.Info(
                    "there is not the most recent common ancestor, need to remove all blocks in the fork chain.");

                BlockCapsule tmp = new_head;
                while (tmp != null)
                {
                    this.khaos_database.RemoveBlock(tmp.Id);
                    tmp = this.khaos_database.GetBlock(tmp.ParentId);
                }

                throw e;
            }

            if (block_tree.Value.IsNotNullOrEmpty())
            {
                while (!this.dynamic_properties_store.GetLatestBlockHeaderHash().Equals(block_tree.Value.LastOrDefault().ParentHash))
                {
                    ReorgContractTrigger();
                    EraseBlock();
                }
            }

            if (block_tree.Key.IsNotNullOrEmpty())
            {
                List<KhaosBlock> first = new List<KhaosBlock>(block_tree.Key);
                first.Reverse();

                foreach (KhaosBlock item in first)
                {
                    System.Exception exception = null;

                    try
                    {
                        using (ISession tmpSession = this.revoking_store.BuildSession())
                        {
                            ApplyBlock(item.Block);
                            tmpSession.Commit();
                        }
                    }
                    catch (System.Exception e)
                    {
                        if (e is AccountResourceInsufficientException
                            || e is ValidateSignatureException
                            || e is ContractValidateException
                            || e is ContractExeException
                            || e is TaposException
                            || e is DupTransactionException
                            || e is TransactionExpirationException
                            || e is ReceiptCheckErrorException
                            || e is TooBigTransactionException
                            || e is TooBigTransactionResultException
                            || e is ValidateScheduleException
                            || e is VMIllegalException
                            || e is BadBlockException)
                        {
                            Logger.Warning(e.Message);
                            exception = e;
                        }

                        throw e;
                    }
                    finally
                    {
                        if (exception != null)
                        {
                            Logger.Warning("switch back because exception thrown while switching forks. " + exception.Message);
                            first.ForEach(khaos_block => this.khaos_database.RemoveBlock(khaos_block.Block.Id));
                            this.khaos_database.SetHead(block_tree.Value.Last());

                            while (!this.dynamic_properties_store.GetLatestBlockHeaderHash().Equals(block_tree.Value.Last().ParentHash))
                            {
                                EraseBlock();
                            }

                            List<KhaosBlock> second = new List<KhaosBlock>(block_tree.Value);
                            second.Reverse();

                            foreach (KhaosBlock khaosBlock in second)
                            {
                                try
                                {
                                    using (ISession tmpSession = this.revoking_store.BuildSession())
                                    {
                                        ApplyBlock(khaosBlock.Block);
                                        tmpSession.Commit();
                                    }
                                }

                                catch (System.Exception e)
                                {
                                    if (e is AccountResourceInsufficientException
                                        || e is ValidateSignatureException
                                        || e is ContractValidateException
                                        || e is ContractExeException
                                        || e is TaposException
                                        || e is DupTransactionException
                                        || e is TransactionExpirationException
                                        || e is TooBigTransactionException
                                        || e is ValidateScheduleException
                                        )
                                    {
                                        Logger.Warning(e.Message);
                                    }
                                    else
                                    {
                                        throw e;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void UpdateSignedWitness(BlockCapsule block)
        {
            WitnessCapsule witness = this.witness_store.GetUnchecked(block.Instance.BlockHeader.RawData.WitnessAddress.ToByteArray());
            witness.TotalProduced = witness.TotalProduced + 1;
            witness.LatestBlockNum = block.Num;
            witness.LatestSlotNum = this.witness_controller.GetAbsSlotAtTime(block.Timestamp);

            WitnessCapsule wit = this.witness_controller.GetWitnessesByAddress(block.WitnessAddress);
            if (wit != null)
            {
                wit.TotalProduced = witness.TotalProduced + 1;
                wit.LatestBlockNum = block.Num;
                wit.LatestSlotNum = this.witness_controller.GetAbsSlotAtTime(block.Timestamp);
            }

            this.witness_store.Put(witness.Address.ToByteArray(), witness);

            try
            {
                AdjustAllowance(witness.Address.ToByteArray(), this.dynamic_properties_store.GetWitnessPayPerBlock());
            }
            catch (BalanceInsufficientException e)
            {
                Logger.Warning(e.Message);
            }

            Logger.Debug(
                string.Format("updateSignedWitness. witness address:{0}, blockNum:{1}, totalProduced:{2}",
                              witness.ToHexString(),
                              block.Num,
                              witness.TotalProduced));
        }

        private void UpdateLatestSolidifiedBlock()
        {
            List<long> numbers = this.witness_controller.GetActiveWitnesses()
                                                        .Select(address => this.witness_controller.GetWitnessesByAddress(address).LatestBlockNum)
                                                        .ToList();
            numbers.Sort();

            long size = this.witness_controller.GetActiveWitnesses().Count;
            int solidified_position = (int)(size * (1 - Parameter.ChainParameters.SOLIDIFIED_THRESHOLD * 1.0 / 100));
            if (solidified_position < 0)
            {
                Logger.Warning(
                    string.Format("updateLatestSolidifiedBlock error, solidifiedPosition:{0},wits.size:{1}",
                                  solidified_position,
                                  size));
                return;
            }
            long latest_block_num = numbers[solidified_position];
            if (latest_block_num < this.dynamic_properties_store.GetLatestSolidifiedBlockNum())
            {
                Logger.Warning("latestSolidifiedBlockNum = 0,LatestBlockNum:" + numbers);
                return;
            }

            this.dynamic_properties_store.PutLatestSolidifiedBlockNum(latest_block_num);
            this.latest_solidified_block_number = latest_block_num;

            Logger.Info("update solid block, num = " + latest_block_num);
        }

        private void UpdateTransHashCache(BlockCapsule block)
        {
            foreach (TransactionCapsule tx in block.Transactions)
            {
                this.transaction_id_cache.Add(tx.Id.ToString(), true, new CacheItemPolicy());
            }
        }

        private void UpdateMaintenanceState(bool is_need_maintenance)
        {
            if (is_need_maintenance)
            {
                this.dynamic_properties_store.PutStateFlag(1);
            }
            else
            {
                this.dynamic_properties_store.PutStateFlag(0);
            }
        }

        private void UpdateRecentBlock(BlockCapsule block)
        {
            this.recent_block_store.Put(ArrayUtil.SubArray(BitConverter.GetBytes(block.Num), 6, 8),
                                        new BytesCapsule(ArrayUtil.SubArray(block.Id.Hash, 8, 16)));
        }

        private void UpdateDynamicProperties(BlockCapsule block)
        {
            long slot = 1;
            if (block.Num != 1)
            {
                slot = this.witness_controller.GetSlotAtTime(block.Timestamp);
            }
            for (int i = 1; i < slot; ++i)
            {
                if (!this.witness_controller.GetScheduleWitness(i).Equals(block.WitnessAddress))
                {
                    WitnessCapsule witness = this.witness_store.GetUnchecked(this.witness_controller.GetScheduleWitness(i).ToByteArray());
                    witness.TotalMissed = witness.TotalMissed + 1;
                    this.witness_store.Put(witness.CreateDatabaseKey(), witness);
                    Logger.Info(
                        string.Format("{0} miss a block. totalMissed = {1}",
                                      witness.ToHexString(),
                                      witness.TotalMissed));
                }

                this.dynamic_properties_store.ApplyBlock(false);
            }

            this.dynamic_properties_store.ApplyBlock(true);

            if (slot <= 0)
            {
                Logger.Warning("missedBlocks [" + slot + "] is illegal");
            }

            Logger.Info("update head, num = " + block.Num);
            this.dynamic_properties_store.PutLatestBlockHeaderHash(ByteString.CopyFrom(block.Id.Hash));
            this.dynamic_properties_store.PutLatestBlockHeaderNumber(block.Num);
            this.dynamic_properties_store.PutLatestBlockHeaderTimestamp(block.Timestamp);
            this.revoking_store.MaxSize =
                (int)(this.dynamic_properties_store.GetLatestBlockHeaderNumber()
                        - this.dynamic_properties_store.GetLatestSolidifiedBlockNum() + 1);

            this.khaos_database.SetMaxCapacity((int)
                (this.dynamic_properties_store.GetLatestBlockHeaderNumber() - this.dynamic_properties_store.GetLatestSolidifiedBlockNum() + 1));
        }

        private void FilterOwnerAddress(TransactionCapsule transactionCapsule, HashSet<string> result)
        {
            Contract contract = transactionCapsule.Instance.RawData.Contract[0];
            byte[] owner = TransactionCapsule.GetOwner(contract);
            string owner_address = owner.ToHexString();
            if (this.owner_addresses.Contains(owner_address))
            {
                result.Add(owner_address);
            }
        }

        private void CloseStore(IDB database)
        {
            Logger.Info("begin to close " + database.Name + " ********");
            try
            {
                database.Close();
            }
            catch (System.Exception e)
            {
                Logger.Info("failed to close  " + database.Name + ". " + e);
            }
            finally
            {
                Logger.Info("******** end to close " + database.Name + " ********");
            }
        }
        #endregion


        #region External Method
        public void Init()
        {
            this.revoking_store.Disable();
            this.revoking_store.Check();

            InitGenesis();

            try
            {
                this.khaos_database.Start(GetBlockById(this.dynamic_properties_store.GetLatestBlockHeaderHash()));
            }
            catch (ItemNotFoundException e)
            {
                Logger.Error(
                    string.Format(
                            "Can not find Dynamic highest block from DB! \nnumber={0} \nhash={1}",
                            this.dynamic_properties_store.GetLatestBlockHeaderNumber(),
                            this.dynamic_properties_store.GetLatestBlockHeaderHash()));

                Logger.Error(
                    string.Format(
                            "Please delete database directory({0}) and restart",
                            Args.Instance.GetOutputDirectory()));

                Environment.Exit(1);
            }
            catch (BadItemException e)
            {
                Logger.Error("DB data broken!");
                Logger.Error(
                    string.Format(
                            "Please delete database directory({0}) and restart",
                            Args.Instance.GetOutputDirectory()));

                Environment.Exit(1);
            }
            this.fork_controller.Init(this);

            if (Args.Instance.Storage.NeedToUpdateAsset && IsNeedToUpdateAsset)
            {
                new AssetUpdateHelper(this).DoWork();
            }

            //for test only
            this.dynamic_properties_store.UpdateDynamicStoreByConfig();

            InitCacheTransactions();
            this.revoking_store.Enable();

            new Thread(new ThreadStart(RePushLoop)).Start();

            // TODO : EventPluginLoader is not Implementation
            //if (Args.Instance.IsEventSubscribe)
            //{
            //    StartEventSubscribing();
            //    Thread triggerCapsuleProcessThread = new Thread(triggerCapsuleProcessLoop);
            //    triggerCapsuleProcessThread.start();
            //}
        }

        public bool IsNeedMaintenance(long block_time)
        {
            return this.dynamic_properties_store.GetNextMaintenanceTime() <= block_time;
        }

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

        public void AdjustAllowance(byte[] account_address, long amount)
        {
            AccountCapsule account = this.account_store.GetUnchecked(account_address);
            if (amount == 0)
            {
                return;
            }

            if (amount < 0 && account.Allowance < -amount)
            {
                throw new BalanceInsufficientException(
                    account_address.ToHexString() + " insufficient balance");
            }
            account.Allowance = account.Allowance + amount;
            this.account_store.Put(account.CreateDatabaseKey(), account);
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

        public BlockCapsule GetBlockByNum(long num)
        {
            return GetBlockById(GetBlockIdByNum(num));
        }

        public BlockId GetBlockIdByNum(long num)
        {
            return this.block_index_store.Get(num);
        }

        public AssetIssueStore GetAssetIssueStoreFinal()
        {
            if (DynamicProperties.GetAllowSameTokenName() == 0)
                return this.asset_issue_store;
            else
                return this.asset_issue_v2_store;
        }

        public long GetSyncBeginNumber()
        {
            Logger.Info("headNumber:" + this.dynamic_properties_store.GetLatestBlockHeaderNumber());
            Logger.Info("syncBeginNumber:" + (this.dynamic_properties_store.GetLatestBlockHeaderNumber() - this.revoking_store.Size));
            Logger.Info("solidBlockNumber:" + this.dynamic_properties_store.GetLatestSolidifiedBlockNum());

            return this.dynamic_properties_store.GetLatestBlockHeaderNumber() - this.revoking_store.Size;
        }

        public List<BlockId> GetBlockChainHashesOnFork(BlockId hash)
        {
            KeyValuePair<List<KhaosBlock>, List<KhaosBlock>> branch =
                this.khaos_database.GetBranch(this.dynamic_properties_store.GetLatestBlockHeaderHash(), hash);

            List<KhaosBlock> blocks = branch.Value;

            if (blocks.IsNullOrEmpty())
            {
                Logger.Info("empty branch " + hash);
                return new List<BlockId>();
            }

            List<BlockId> result = blocks.Select(block => block.Block.Id).ToList();
            result.Add(blocks.Last().Block.ParentId);

            return result;
        }

        public HashSet<Node> ReadNeighbours()
        {
            return this.peer_store.Get(Encoding.UTF8.GetBytes("neighbours"));
        }

        public void ClearAndWriteNeighbours(HashSet<Node> nodes)
        {
            this.peer_store.Put(Encoding.UTF8.GetBytes("neighbours"), nodes);
        }

        public bool LastHeadBlockIsMaintenance()
        {
            return DynamicProperties.GetStateFlag() == 1;
        }

        public bool ContainBlock(SHA256Hash hash)
        {
            try
            {
                return this.khaos_database.ContainInMiniStore(hash)
                    || this.block_store.Get(hash.Hash) != null;
            }
            catch (ArgumentException e)
            {
                return false;
            }
            catch (ItemNotFoundException)
            {
                return false;
            }

        }

        public bool ContainBlockInMainChain(BlockId blockId)
        {
            try
            {
                return this.block_store.Get(blockId.Hash) != null;
            }
            catch (ItemNotFoundException e)
            {
                return false;
            }
        }

        public void PutExchangeCapsule(ExchangeCapsule exchange)
        {
            if (this.dynamic_properties_store.GetAllowSameTokenName() == 0)
            {
                this.exchange_store.Put(exchange.CreateDatabaseKey(), exchange);

                ExchangeCapsule exchange_v2 = new ExchangeCapsule(exchange.Data);
                exchange_v2.ResetTokenWithID(this);
                this.exchange_v2_store.Put(exchange_v2.CreateDatabaseKey(), exchange_v2);
            }
            else
            {
                this.exchange_v2_store.Put(exchange.CreateDatabaseKey(), exchange);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public BlockCapsule GenerateBlock(WitnessCapsule witness,
                                          long when,
                                          byte[] privatekey,
                                          bool is_maintenance_before,
                                          bool need_check_witness_permission)
        {
            if (!this.witness_controller.ValidateWitnessSchedule(witness.Address, when))
            {
                Logger.Info("It's not my turn, "
                    + "and the first block after the maintenance period has just been processed.");

                Logger.Info(
                    string.Format("when:{0},lastHeadBlockIsMaintenanceBefore:{1},lastHeadBlockIsMaintenanceAfter:{2}",
                                  when,
                                  is_maintenance_before,
                                  LastHeadBlockIsMaintenance()));

                return null;
            }

            long timestamp = this.dynamic_properties_store.GetLatestBlockHeaderTimestamp();
            long number = this.dynamic_properties_store.GetLatestBlockHeaderNumber();
            SHA256Hash prev_hash = this.dynamic_properties_store.GetLatestBlockHeaderHash();

            if (when < timestamp)
            {
                throw new ArgumentException("Generate block timestamp is invalid.");
            }

            long postponed_tx_count = 0;

            BlockCapsule block = new BlockCapsule(number + 1, prev_hash, when, witness.Address);
            block.IsGenerateMyself = true;
            this.session.Reset();
            this.session.SetValue(this.revoking_store.BuildSession());

            Manager.Instance.FastSyncCallback.PreExecute(block);

            if (need_check_witness_permission
                && !this.witness_service.ValidateWitnessPermission(witness.Address))
            {
                Logger.Warning("Witness permission is wrong");
                return null;
            }

            HashSet<string> accounts = new HashSet<string>();
            foreach (var transaction in this.pending_transactions)
            {
                if (!ProcessPenddingTransaction(transaction,
                                                when,
                                                block,
                                                accounts,
                                                postponed_tx_count,
                                                true))
                {
                    break;
                }
            }

            while (this.repush_transactions.Count > 0)
            {
                if (this.repush_transactions.TryDequeue(out TransactionCapsule transaction))
                {
                    if (!ProcessPenddingTransaction(transaction,
                                when,
                                block,
                                accounts,
                                postponed_tx_count,
                                true))
                    {
                        break;
                    }
                }
            }

            Manager.Instance.FastSyncCallback.ExecuteGenerateFinish();
            this.session.Reset();

            if (postponed_tx_count > 0)
            {
                Logger.Info(
                    string.Format("{0} transactions over the block size limit", postponed_tx_count));
            }

            Logger.Info(
                "postponedTrxCount[" + postponed_tx_count + "],TrxLeft[" + this.pending_transactions.Count
                    + "],repushTrxCount[" + this.repush_transactions.Count + "]");

            block.SetMerkleTree();
            block.Sign(privatekey);

            try
            {
                PushBlock(block);
                return block;
            }
            catch (TaposException e)
            {
                Logger.Info("contract not processed during TaposException");
            }
            catch (TooBigTransactionException e)
            {
                Logger.Info("contract not processed during TooBigTransactionException");
            }
            catch (DupTransactionException e)
            {
                Logger.Info("contract not processed during DupTransactionException");
            }
            catch (TransactionExpirationException e)
            {
                Logger.Info("contract not processed during TransactionExpirationException");
            }
            catch (BadNumberBlockException e)
            {
                Logger.Info("generate block using wrong number");
            }
            catch (BadBlockException e)
            {
                Logger.Info("block exception");
            }
            catch (NonCommonBlockException e)
            {
                Logger.Info("non common exception");
            }
            catch (ReceiptCheckErrorException e)
            {
                Logger.Info("OutOfSlotTime exception : " + e.Message);
                Logger.Debug(e.Message);
            }
            catch (VMIllegalException e)
            {
                Logger.Warning(e.Message);
            }
            catch (TooBigTransactionResultException e)
            {
                Logger.Info("contract not processed during TooBigTransactionResultException");
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void PushBlock(BlockCapsule block)
        {
            long start = Helper.CurrentTimeMillis();

            try
            {
                PendingManager pm = new PendingManager(this);

                if (!block.IsGenerateMyself)
                {
                    if (!block.ValidateSignature(this))
                    {
                        Logger.Warning("The signature is not validated.");
                        throw new BadBlockException("The signature is not validated");
                    }

                    if (!block.CalcMerkleRoot().Equals(block.MerkleRoot))
                    {
                        Logger.Warning(
                            string.Format(
                                "The merkle root doesn't match, Calc result is "
                                + block.CalcMerkleRoot()
                                + " , the headers is "
                                + block.MerkleRoot));

                        throw new BadBlockException("The merkle hash is not validated");
                    }
                }

                if (this.witness_service != null)
                {
                    this.witness_service.CheckDupWitness(block);
                }

                BlockCapsule new_block = this.khaos_database.Push(block);

                if (this.dynamic_properties_store.GetLatestBlockHeaderHash() == null)
                {
                    if (new_block.Num != 0)
                        return;
                }
                else
                {
                    if (new_block.Num <= this.dynamic_properties_store.GetLatestBlockHeaderNumber())
                        return;

                    if (!new_block.ParentId.Equals(this.dynamic_properties_store.GetLatestBlockHeaderHash()))
                    {
                        Logger.Warning(
                            string.Format("switch fork! new head num = {0}, blockid = {1}",
                                          new_block.Num,
                                          new_block.Id.ToString()));

                        Logger.Warning(
                            "******** before switchFork ******* push block: "
                                + block.ToString()
                                + ", new block:"
                                + new_block.ToString()
                                + ", dynamic head num: "
                                + this.dynamic_properties_store.GetLatestBlockHeaderNumber()
                                + ", dynamic head hash: "
                                + this.dynamic_properties_store.GetLatestBlockHeaderHash()
                                + ", dynamic head timestamp: "
                                + this.dynamic_properties_store.GetLatestBlockHeaderTimestamp()
                                + ", khaosDb head: "
                                + this.khaos_database.GetHead().ToString()
                                + ", khaosDb miniStore size: "
                                + this.khaos_database.MiniStore.Size
                                + ", khaosDb unlinkMiniStore size: "
                                + this.khaos_database.MiniUnlinkedStore.Size);

                        SwitchFork(new_block);
                        Logger.Info("save block: " + new_block);

                        Logger.Warning(
                            "******** after switchFork ******* push block: "
                                + block.ToString()
                                + ", new block:"
                                + new_block.ToString()
                                + ", dynamic head num: "
                                + this.dynamic_properties_store.GetLatestBlockHeaderNumber()
                                + ", dynamic head hash: "
                                + this.dynamic_properties_store.GetLatestBlockHeaderHash()
                                + ", dynamic head timestamp: "
                                + this.dynamic_properties_store.GetLatestBlockHeaderTimestamp()
                                + ", khaosDb head: "
                                + this.khaos_database.GetHead()
                                + ", khaosDb miniStore size: "
                                + this.khaos_database.MiniStore.Size
                                + ", khaosDb unlinkMiniStore size: "
                                + this.khaos_database.MiniUnlinkedStore.Size);

                        return;
                    }

                    try
                    {
                        using (ISession tmpSession = this.revoking_store.BuildSession())
                        {
                            ApplyBlock(new_block);
                            tmpSession.Commit();
                            PostBlockTrigger(new_block);
                        }
                    }
                    catch (System.Exception e)
                    {
                        Logger.Error(e.Message);
                        this.khaos_database.RemoveBlock(block.Id);
                        throw e;
                    }
                }

                Logger.Info("save block: " + new_block);
            }
            catch
            {
            }

            lock (this.push_transactions)
            {
                if (this.owner_addresses.IsNotNullOrEmpty())
                {
                    HashSet<string> results = new HashSet<string>();
                    foreach (TransactionCapsule tx in this.repush_transactions)
                    {
                        FilterOwnerAddress(tx, results);
                    }

                    foreach (TransactionCapsule tx in this.push_transactions)
                    {
                        FilterOwnerAddress(tx, results);
                    }
                    this.owner_addresses.Clear();

                    foreach (string result in results)
                    {
                        this.owner_addresses.Add(result);
                    }
                }
            }

            Logger.Info(
                string.Format("pushBlock block number:{0}, cost/txs:{1}/{2}",
                              block.Num,
                              Helper.CurrentTimeMillis() - start,
                              block.Transactions.Count));
        }

        public bool PushTransaction(TransactionCapsule transaction)
        {

            lock (this.push_transactions)
            {
                this.push_transactions.Enqueue(transaction);
            }

            try
            {
                if (!transaction.ValidateSignature(this))
                {
                    throw new ValidateSignatureException("trans sig validate failed");
                }

                lock (this)
                {
                    if (!session.IsValid())
                    {
                        session.SetValue(this.revoking_store.BuildSession());
                    }

                    try
                    {
                        using (ISession temp_session = this.revoking_store.BuildSession())
                        {
                            ProcessTransaction(transaction, null);
                            this.pending_transactions.Add(transaction);
                            temp_session.Merge();
                        }
                    }
                    catch (System.Exception e)
                    {
                        Logger.Error(e.Message);
                    }
                }
            }
            finally
            {
                this.push_transactions.Remove(transaction);
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void EraseBlock()
        {
            this.session.Reset();

            try
            {
                BlockCapsule old_head_block = GetBlockById(this.dynamic_properties_store.GetLatestBlockHeaderHash());

                Logger.Info("begin to erase block :" + old_head_block);
                this.khaos_database.Pop();
                this.revoking_store.FastPop();
                Logger.Info("end to erase block:" + old_head_block);

                foreach (var transaction in old_head_block.Transactions)
                {
                    this.pop_transactions.Add(transaction);
                }
            }
            catch (ItemNotFoundException e)
            {
                Logger.Warning(e.Message);
            }
            catch (ArgumentException e)
            {
                Logger.Warning(e.Message);
            }
        }

        public void ProcessBlock(BlockCapsule block)
        {
            if (!this.witness_controller.ValidateWitnessSchedule(block))
            {
                throw new ValidateScheduleException("validateWitnessSchedule error");
            }

            this.dynamic_properties_store.PutBlockEnergyUsage(0);

            if (!block.IsGenerateMyself)
            {
                try
                {
                    PreValidateTransactionSign(block);
                }
                catch (ThreadInterruptedException e)
                {
                    Logger.Error("parallel check sign interrupted exception! block info : " + block.ToString());
                    Thread.CurrentThread.Interrupt();
                }
            }

            try
            {
                Manager.Instance.FastSyncCallback.PreExecute(block);

                foreach (TransactionCapsule tx in block.Transactions)
                {
                    tx.BlockNum = block.Num;
                    if (block.IsGenerateMyself)
                    {
                        tx.IsVerified = true;
                    }

                    Manager.Instance.FastSyncCallback.PreExecuteTrans();
                    ProcessTransaction(tx, block);
                    Manager.Instance.FastSyncCallback.ExecuteTransFinish();
                }
                Manager.Instance.FastSyncCallback.ExecutePushFinish();
            }
            finally
            {
                Manager.Instance.FastSyncCallback.ExceptionFinish();
            }

            bool need_maintenance = IsNeedMaintenance(block.Timestamp);
            if (need_maintenance)
            {
                if (block.Num == 1)
                {
                    this.dynamic_properties_store.UpdateNextMaintenanceTime(block.Timestamp);
                }
                else
                {
                    ProcessMaintenance(block);
                }
            }
            if (this.dynamic_properties_store.GetAllowAdaptiveEnergy() == 1)
            {
                EnergyProcessor processor = new EnergyProcessor(this);
                processor.UpdateTotalEnergyAverageUsage();
                processor.UpdateAdaptiveTotalEnergyLimit();
            }
            UpdateSignedWitness(block);
            UpdateLatestSolidifiedBlock();
            UpdateTransHashCache(block);
            UpdateMaintenanceState(need_maintenance);
            UpdateRecentBlock(block);
            UpdateDynamicProperties(block);
        }

        public bool ProcessTransaction(TransactionCapsule transaction, BlockCapsule block)
        {
            if (transaction == null)
            {
                return false;
            }

            ValidateTapos(transaction);
            ValidateCommon(transaction);

            if (transaction.Instance.RawData.Contract.Count != 1)
            {
                throw new ContractSizeNotEqualToOneException("act size should be exactly 1, this is extend feature");
            }

            ValidateDup(transaction);

            if (!transaction.ValidateSignature(this))
            {
                throw new ValidateSignatureException("trans sig validate failed");
            }

            TransactionTrace trace = new TransactionTrace(transaction, this);
            transaction.TransactionTrace = trace;

            ConsumeBandwidth(transaction, trace);
            ConsumeMultiSignFee(transaction, trace);

            Common.Runtime.Config.VMConfig.InitVmHardFork();
            Common.Runtime.Config.VMConfig.InitAllowMultiSign(this.dynamic_properties_store.GetAllowMultiSign());
            Common.Runtime.Config.VMConfig.InitAllowTvmTransferTrc10(this.dynamic_properties_store.GetAllowTvmTransferTrc10());
            Common.Runtime.Config.VMConfig.InitAllowTvmConstantinople(this.dynamic_properties_store.GetAllowTvmConstantinople());
            trace.Init(block, IsEventPluginLoaded);
            trace.CheckIsConstant();
            trace.Execute();

            if (block != null)
            {
                trace.SetResult();
                if (!block.Instance.BlockHeader.WitnessSignature.IsEmpty)
                {
                    if (trace.CheckNeedRetry())
                    {
                        string tx_id = transaction.Id.Hash.ToHexString();
                        Logger.Info("Retry for tx id : " + tx_id);
                        trace.Init(block, IsEventPluginLoaded);
                        trace.CheckIsConstant();
                        trace.Execute();
                        trace.SetResult();
                        Logger.Info(
                            string.Format("Retry result for tx id: {0}, tx resultCode in receipt: {1}",
                                          tx_id,
                                          trace.Receipt.Result));
                    }
                    trace.Check();
                }
            }

            trace.Finalization();
            if (block != null && this.dynamic_properties_store.SupportVM())
            {
                transaction.SetResultCode(trace.Receipt.Result);
            }

            this.transaction_store.Put(transaction.Id.Hash, transaction);
            this.transaction_cache?.Put(transaction.Id.Hash, new BytesCapsule(BitConverter.GetBytes(transaction.BlockNum)));

            TransactionInfoCapsule transaction_info = TransactionInfoCapsule.BuildInstance(transaction, block, trace);
            this.transaction_history_store.Put(transaction.Id.Hash, transaction_info);

            PostContractTrigger(trace, false);
            if (IsMultSignTransaction(transaction.Instance))
            {
                this.owner_addresses.Add(TransactionCapsule.GetOwner(transaction.Instance.RawData.Contract[0]).ToHexString());
            }

            return true;
        }

        public void PreValidateTransactionSign(BlockCapsule block)
        {
            Logger.Info("PreValidate Transaction Sign, size:" + block.Transactions.Count + ",block num:" + block.Num);
            int tx_size = block.Transactions.Count;
            if (tx_size <= 0)
            {
                return;
            }

            CountdownEvent cde = new CountdownEvent(tx_size);
            List<Task<bool>> tasks = new List<Task<bool>>(tx_size);

            foreach (TransactionCapsule tx in block.Transactions)
            {
                tasks.Add(
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            tx.ValidateSignature(this);
                        }
                        catch (System.Exception e)
                        {
                            throw e;
                        }
                        finally
                        {
                            cde.Signal();
                        }

                        return true;
                    }));
            }

            cde.Wait();

            foreach (Task<bool> result in tasks)
            {
                if (!result.Result)
                {
                    throw new ValidateSignatureException();
                }
            }
        }

        public void ValidateTapos(TransactionCapsule transaction)
        {
            byte[] ref_hash = transaction.Instance.RawData.RefBlockHash.ToByteArray();
            byte[] ref_bytes = transaction.Instance.RawData.RefBlockBytes.ToByteArray();

            try
            {
                byte[] hash = this.recent_block_store.Get(ref_bytes).Data;
                if (!hash.SequenceEqual(ref_hash))
                {
                    string msg = string.Format("Tapos failed, different block hash, {0}, {1} , recent block {2}, solid block {3} head block {4}",
                                                BitConverter.ToInt64(ref_bytes, 0),
                                                ref_hash.ToHexString(),
                                                hash.ToHexString(),
                                                SolidBlockId.GetString(),
                                                HeadBlockId.GetString());

                    Logger.Info(msg);
                    throw new TaposException(msg);
                }
            }
            catch (ItemNotFoundException e)
            {
                string msg = string.Format("Tapos failed, block not found, ref block {0}, {1} , solid block {2} head block {3}",
                                            BitConverter.ToInt64(ref_bytes, 0),
                                            ref_hash.ToHexString(),
                                            SolidBlockId.GetString(),
                                            HeadBlockId.GetString());

                Logger.Info(msg);
                throw new TaposException(msg);
            }
        }

        public void ValidateCommon(TransactionCapsule transaction)
        {
            if (transaction.Data.Length > DefineParameter.TRANSACTION_MAX_BYTE_SIZE)
            {
                throw new TooBigTransactionException(
                    "too big transaction, the size is " + transaction.Data.Length + " bytes");
            }
            long head_block_time = GetHeadBlockTimestamp();
            if (transaction.Expiration <= head_block_time ||
                transaction.Expiration > head_block_time + DefineParameter.MAXIMUM_TIME_UNTIL_EXPIRATION)
            {
                throw new TransactionExpirationException(
                    "transaction expiration, transaction expiration time is " + transaction.Expiration
                        + ", but headBlockTime is " + head_block_time);
            }
        }

        public void ValidateDup(TransactionCapsule transaction)
        {
            if (ContainsTransaction(transaction))
            {
                Logger.Debug(transaction.Id.Hash.ToHexString());
                throw new DupTransactionException("dup trans");
            }
        }

        public void ConsumeBandwidth(TransactionCapsule transaction, TransactionTrace trace)
        {
            BandwidthProcessor processor = new BandwidthProcessor(this);
            processor.Consume(transaction, trace);
        }

        public void ConsumeMultiSignFee(TransactionCapsule transaction, TransactionTrace trace)
        {
            if (transaction.Instance.Signature.Count > 1)
            {
                long fee = this.dynamic_properties_store.GetMultiSignFee();

                foreach (Contract contract in transaction.Instance.RawData.Contract)
                {
                    byte[] address = TransactionCapsule.GetOwner(contract);
                    AccountCapsule account = this.account_store.Get(address);
                    try
                    {
                        AdjustBalance(account, -fee);
                        AdjustBalance(this.account_store.GetBlackHole().CreateDatabaseKey(), +fee);
                    }
                    catch (BalanceInsufficientException)
                    {
                        throw new AccountResourceInsufficientException(
                            "Account Insufficient  balance[" + fee + "] to MultiSign");
                    }
                }

                trace.Receipt.MultiSignFee = fee;
            }
        }

        public void CloseAll()
        {
            Logger.Info("------------------ Begin to close store ------------------");
            CloseStore(this.account_store);
            CloseStore(this.block_store);
            CloseStore(this.block_index_store);
            CloseStore(this.account_id_index_store);
            CloseStore(this.account_index_store);
            CloseStore(this.witness_store);
            CloseStore(this.witness_schedule_store);
            CloseStore(this.asset_issue_store);
            CloseStore(this.dynamic_properties_store);
            CloseStore(this.transaction_store);
            CloseStore(this.code_store);
            CloseStore(this.contract_store);
            CloseStore(this.storage_row_store);
            CloseStore(this.exchange_store);
            CloseStore(this.peer_store);
            CloseStore(this.proposal_store);
            CloseStore(this.recent_block_store);
            CloseStore(this.transaction_history_store);
            CloseStore(this.votes_store);
            CloseStore(this.delegated_resource_store);
            CloseStore(this.delegate_resource_Account_index_store);
            CloseStore(this.asset_issue_v2_store);
            CloseStore(this.exchange_v2_store);
            Logger.Info("------------------ End to close store ------------------");
        }
        #endregion
    }
}
