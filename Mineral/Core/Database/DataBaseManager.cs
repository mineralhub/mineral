using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Mineral.Common.Overlay.Discover.Node;
using Mineral.Common.Utils;
using Mineral.Core.Capsule;
using Mineral.Core.Config;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Database.Fast.Callback;
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
    public class DataBaseManager
    {
        #region Field
        private static DataBaseManager instance = null;

        private IRevokingDatabase revoking_store = null;
        private KhaosDatabase khaos_database = null;
        private BlockStore block_store = null;
        private RecentBlockStore recent_block_store = null;
        private BlockIndexStore block_index_store = null;
        private TransactionStore transaction_store = null;
        private TransactionCache transaction_cache = null;
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
        private PeerStore peer_store = new PeerStore();

        private ForkController fork_controller = ForkController.Instance;
        private WitnessController witness_controller = null;
        private WitnessService witness_service = null;
        private BlockCapsule genesis_block = null;

        private FastSyncCallBack fast_sync_call_back = null;
        private SessionOptional session = SessionOptional.Instance;
        private BlockingCollection<TransactionCapsule> pending_transactions = new BlockingCollection<TransactionCapsule>();
        private BlockingCollection<TransactionCapsule> pop_Transactions = new BlockingCollection<TransactionCapsule>();
        private ConcurrentQueue<TransactionCapsule> repush_transactions = new ConcurrentQueue<TransactionCapsule>();

        private HashSet<string> owner_addresses = new HashSet<string>();
        #endregion


        #region Property
        public static DataBaseManager Instance
        {
            get { return Instance ?? new DataBaseManager(); }
        }

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

        public BlockingCollection<TransactionCapsule> PendingTransactions
        {
            get { return this.pending_transactions; }
        }

        public BlockingCollection<TransactionCapsule> PopTransactions
        {
            get { return this.pop_Transactions; }
        }

        public ConcurrentQueue<TransactionCapsule> RePushTransactions
        {
            get { return this.repush_transactions; }
        }

        public SessionOptional Session
        {
            get { return this.session; }
        }

        public bool IsEventPluginLoaded { get; set; }
        #endregion


        #region Constructor
        private DataBaseManager()
        {
            this.khaos_database = new KhaosDatabase("block_KDB");

            this.block_store = new BlockStore("block");
            this.block_index_store = new BlockIndexStore("block-index");
            this.transaction_store = new TransactionStore(this.block_store, this.khaos_database, "transaction");
            this.account_store = new AccountStore(this, "account");
            this.witness_store = new WitnessStore("witness");
            this.witness_schedule_store = new WitnessScheduleStore("siwtness_schedule");
            this.votes_store = new VotesStore("votes");
            this.proposal_store = new ProposalStore("proposal");
            this.asset_issue_store = new AssetIssueStore("asset-issue");
            this.code_store = new CodeStore("code");
            this.contract_store = new ContractStore("contract");
            this.storage_row_store = new StorageRowStore("storage-row");
            this.dynamic_properties_store = new DynamicPropertiesStore("properties");

            this.witness_controller = new WitnessController(this);
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
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
        public BlockCapsule GenerateBlock(WitnessCapsule witness, long when, byte[] privateKey, bool is_maintenance_before, bool need_check_witness_permission)
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

            long postponed_tx_Count = 0;

            BlockCapsule block = new BlockCapsule(number + 1, prev_hash, when, witness.Address);
            block.IsGenerateMyself = true;
            this.session.Reset();
            this.session.SetValue(this.revoking_store.BuildSession());

            this.fast_sync_call_back.PreExecute(block);

            if (need_check_witness_permission
                && !this.witness_service.ValidateWitnessPermission(witness.Address))
            {
                Logger.Warning("Witness permission is wrong");
                return null;
            }

            HashSet<string> accounts = new HashSet<string>();

            foreach (TransactionCapsule tx in this.pending_transactions)
            {
                if (DateTime.UtcNow.Millisecond - when
                    > Parameter.ChainParameters.BLOCK_PRODUCED_INTERVAL * 0.5 * Args.Instance.Node.BlockProducedTimeout / 100)
                {
                    Logger.Warning("Processing transaction time exceeds the 50% producing time。");
                    break;
                }

                if ((block.Instance.CalculateSize() + tx.Instance.CalculateSize() + 3) > Parameter.ChainParameters.BLOCK_SIZE)
                {
                    postponed_tx_Count++;
                    continue;
                }

                Contract contract = tx.Instance.RawData.Contract[0];
                byte[] owner = TransactionCapsule.GetOwner(contract);
                string owner_address = owner.ToHexString();
                if (accounts.Contains(owner_address))
                {
                    continue;
                }
                else
                {
                    if (IsMultSignTransaction(tx.Instance))
                    {
                        accounts.Add(owner_address);
                    }
                }

                if (this.owner_addresses.Contains(owner_address))
                {
                    tx.IsVerified = false;
                }

                try
                {
                    ISession temp_session = this.revoking_store.BuildSession();

                    this.fast_sync_call_back.PreExecuteTrans();
                    ProcessTransaction(tx, block);
                    this.fast_sync_call_back.ExecuteTransFinish();

                    temp_session.Merge();
                    block.AddTransaction(tx);
                    if (is_from_pending)
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
                catch (ReceiptCheckErrException e)
                {
                    Logger.Info("OutOfSlotTime exception: {}", e.getMessage());
                    Logger.Debug(e.Message);
                }
                catch (VMIllegalException e)
                {
                    Logger.Warning(e.Message);
                }
            }



            foreach

            Iterator<TransactionCapsule> iterator = pendingTransactions.iterator();
            while (iterator.hasNext() || repushTransactions.size() > 0)
            {
                if (iterator.hasNext())
                {
                    fromPending = true;
                    trx = (TransactionCapsule)iterator.next();
                }
                else
                {
                    trx = this.repush_transactions.poll();
                }

                if (DateTime.now().getMillis() - when
                    > ChainConstant.BLOCK_PRODUCED_INTERVAL * 0.5
                    * Args.getInstance().getBlockProducedTimeOut()
                    / 100)
                {
                    Logger.Warning("Processing transaction time exceeds the 50% producing time。");
                    break;
                }

                // check the block size
                if ((block.getInstance().getSerializedSize() + trx.getSerializedSize() + 3)
                    > ChainConstant.BLOCK_SIZE)
                {
                    postponed_tx_Count++;
                    continue;
                }

                //
                Contract contract = trx.getInstance().getRawData().getContract(0);
                byte[] owner = TransactionCapsule.getOwner(contract);
                String ownerAddress = ByteArray.toHexString(owner);
                if (accountSet.contains(ownerAddress))
                {
                    continue;
                }
                else
                {
                    if (isMultSignTransaction(trx.getInstance()))
                    {
                        accountSet.add(ownerAddress);
                    }
                }
                if (ownerAddressSet.contains(ownerAddress))
                {
                    trx.setVerified(false);
                }
                // apply transaction
                try (ISession tmpSeesion = revokingStore.buildSession()) {
                    fastSyncCallBack.preExeTrans();
                    processTransaction(trx, block);
                    fastSyncCallBack.exeTransFinish();
                    tmpSeesion.merge();
                    // push into block
                    block.addTransaction(trx);
                    if (fromPending)
                    {
                        iterator.remove();
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
                catch (ReceiptCheckErrException e)
                {
                    Logger.Info("OutOfSlotTime exception: {}", e.getMessage());
                    Logger.Debug(e.Message);
                }
                catch (VMIllegalException e)
                {
                    Logger.Warning(e.Message);
                }
            } // end of while

            fastSyncCallBack.executeGenerateFinish();

            session.reset();
            if (postponed_tx_Count > 0)
            {
                Logger.info("{} transactions over the block size limit", postponed_tx_Count);
            }

            Logger.info(
                "postponedTrxCount[" + postponed_tx_Count + "],TrxLeft[" + pendingTransactions.size()
                    + "],repushTrxCount[" + repushTransactions.size() + "]");

            block.setMerkleRoot();
            block.sign(privateKey);

            try
            {
                this.pushBlock(block);
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
            catch (ReceiptCheckErrException e)
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
                Logger.info("contract not processed during TooBigTransactionResultException");
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
                                + block.MerkleRoot);

                        throw new BadBlockException("The merkle hash is not validated");
                    }
                }

                if (this.witnessService != null)
                {
                    witnessService.checkDupWitness(block);
                }

                BlockCapsule newBlock = this.khaosDb.push(block);

                // DB don't need lower block
                if (getDynamicPropertiesStore().getLatestBlockHeaderHash() == null)
                {
                    if (newBlock.getNum() != 0)
                    {
                        return;
                    }
                }
                else
                {
                    if (newBlock.getNum() <= getDynamicPropertiesStore().getLatestBlockHeaderNumber())
                    {
                        return;
                    }

                    // switch fork
                    if (!newBlock
                        .getParentHash()
                        .equals(getDynamicPropertiesStore().getLatestBlockHeaderHash()))
                    {
                        Logger.warn(
                            "switch fork! new head num = {}, blockid = {}",
                            newBlock.getNum(),
                            newBlock.getBlockId());

                        Logger.warn(
                            "******** before switchFork ******* push block: "
                                + block.toString()
                                + ", new block:"
                                + newBlock.toString()
                                + ", dynamic head num: "
                                + dynamicPropertiesStore.getLatestBlockHeaderNumber()
                                + ", dynamic head hash: "
                                + dynamicPropertiesStore.getLatestBlockHeaderHash()
                                + ", dynamic head timestamp: "
                                + dynamicPropertiesStore.getLatestBlockHeaderTimestamp()
                                + ", khaosDb head: "
                                + khaosDb.getHead()
                                + ", khaosDb miniStore size: "
                                + khaosDb.getMiniStore().size()
                                + ", khaosDb unlinkMiniStore size: "
                                + khaosDb.getMiniUnlinkedStore().size());

                        switchFork(newBlock);
                        Logger.info("save block: " + newBlock);

                        Logger.warn(
                            "******** after switchFork ******* push block: "
                                + block.toString()
                                + ", new block:"
                                + newBlock.toString()
                                + ", dynamic head num: "
                                + dynamicPropertiesStore.getLatestBlockHeaderNumber()
                                + ", dynamic head hash: "
                                + dynamicPropertiesStore.getLatestBlockHeaderHash()
                                + ", dynamic head timestamp: "
                                + dynamicPropertiesStore.getLatestBlockHeaderTimestamp()
                                + ", khaosDb head: "
                                + khaosDb.getHead()
                                + ", khaosDb miniStore size: "
                                + khaosDb.getMiniStore().size()
                                + ", khaosDb unlinkMiniStore size: "
                                + khaosDb.getMiniUnlinkedStore().size());

                        return;
                    }
                    try (ISession tmpSession = revokingStore.buildSession()) {

                        applyBlock(newBlock);
                        tmpSession.commit();
                        // if event subscribe is enabled, post block trigger to queue
                        postBlockTrigger(newBlock);
                    } catch (Throwable throwable)
                    {
                        Logger.error(throwable.getMessage(), throwable);
                        khaosDb.removeBlk(block.getBlockId());
                        throw throwable;
                    }
                }
                Logger.info("save block: " + newBlock);
            }
            //clear ownerAddressSet
            synchronized(pushTransactionQueue)
        {
                if (CollectionUtils.isNotEmpty(ownerAddressSet))
                {
                    Set<String> result = new HashSet<>();
                    for (TransactionCapsule transactionCapsule : repushTransactions)
                    {
                        filterOwnerAddress(transactionCapsule, result);
                    }
                    for (TransactionCapsule transactionCapsule : pushTransactionQueue)
                    {
                        filterOwnerAddress(transactionCapsule, result);
                    }
                    ownerAddressSet.clear();
                    ownerAddressSet.addAll(result);
                }
            }
            Logger.info("pushBlock block number:{}, cost/txs:{}/{}",
                    block.getNum(),
                    System.currentTimeMillis() - start,
                    block.getTransactions().size());
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

            TransactionInfoCapsule transactionInfo = TransactionInfoCapsule
                .buildInstance(transaction, block, trace);

            transactionHistoryStore.put(transaction.getTransactionId().getBytes(), transactionInfo);

            // if event subscribe is enabled, post contract triggers to queue
            postContractTrigger(trace, false);
            Contract contract = transaction.getInstance().getRawData().getContract(0);
            if (isMultSignTransaction(transaction.getInstance()))
            {
                ownerAddressSet.add(ByteArray.toHexString(TransactionCapsule.getOwner(contract)));
            }

            return true;
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
        #endregion
    }
}
