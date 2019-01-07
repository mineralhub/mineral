using Mineral.Core.Transactions;
using Mineral.Database.BlockChain;
using Mineral.Database.LevelDB;
using Mineral.Utils;
using Mineral.Wallets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mineral.Core
{
    public partial class BlockChain : IDisposable
    {
        #region Definition
        public enum ERROR_BLOCK
        {
            NO_ERROR = 0,
            ERROR = 1,
            ERROR_HEIGHT = 2,
            ERROR_HASH = 3,
        };
        #endregion


        #region Fields
        private static BlockChain instance = null;
        private Proof proof = null;
        private Block genesisBlock = null;

        private AutoResetEvent newBlockEvent = new AutoResetEvent(false);
        public event EventHandler<Block> PersistCompleted;
        public object PersistLock { get; } = new object();
        public object PoolLock { get; } = new object();

        private int currentBlockHeight = 0;
        private UInt256 currentBlockHash = UInt256.Zero;
        private int currentHeaderHeight = 0;
        private UInt256 currentHeaderHash = UInt256.Zero;

        private CacheChain cacheChain = new CacheChain();
        private Dictionary<UInt256, Block> waitPersistBlocks = new Dictionary<UInt256, Block>();
        private Dictionary<UInt256, BlockHeader> cacheHeaders = new Dictionary<UInt256, BlockHeader>();

        protected Dictionary<UInt256, Transaction> rxPool = new Dictionary<UInt256, Transaction>();
        protected Dictionary<UInt256, Transaction> txPool = new Dictionary<UInt256, Transaction>();

        private uint storeHeaderCount = 0;
        private bool disposed = false;
        #endregion


        #region Properties
        public static BlockChain Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new BlockChain();
                    instance.proof = new DPos.DPos();
                }
                return instance;
            }
        }

        public Proof Proof { get { return this.proof; } }

        public int CurrentBlockHeight { get { return this.currentBlockHeight; } }
        public UInt256 CurrentBlockHash { get { return this.currentBlockHash; } }
        public int CurrentHeaderHeight { get { return this.currentHeaderHeight; } }
        public UInt256 CurrentHeaderHash { get { return this.currentHeaderHash; } }
        public Block GenesisBlock { get { return this.genesisBlock; } }
        #endregion


        #region Event Method
        private void PersistBlocksLoop()
        {
            while (!this.disposed)
            {
                this.newBlockEvent.WaitOne();

                while (!this.disposed)
                {
                    if (!this.cacheChain.HeaderIndices.TryGetValue(this.currentBlockHeight + 1, out UInt256 hash))
                        break;

                    Block block;
                    lock (this.waitPersistBlocks)
                    {
                        if (!this.waitPersistBlocks.TryGetValue(hash, out block))
                            break;
                    }

                    // Compare block's previous hash is CurrentBlockHash
                    if (block.Header.PrevHash != CurrentBlockHash)
                        break;

                    lock (PersistLock)
                    {
                        Persist(block);
                        if (0 >= this.proof.RemainUpdate(block.Height))
                            this.proof.Update(this);
                        OnPersistCompleted(block);
                    }
                    lock (this.waitPersistBlocks)
                    {
                        this.waitPersistBlocks.Remove(hash);
                    }
                }
            }
        }
        #endregion


        #region Internal Method
        private void AddHeader(BlockHeader header)
        {
            WriteBatch batch = new WriteBatch();

            this.cacheChain.AddHeaderIndex(header.Height, header.Hash);
            while (this.storeHeaderCount <= header.Height - 2000)
            {
                using (MemoryStream ms = new MemoryStream())
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.WriteSerializableArray(this.cacheChain.HeaderIndices.Values.Skip((int)this.storeHeaderCount).Take(2000));
                    bw.Flush();
                    this.manager.PutHeaderHashList(batch, (int)this.storeHeaderCount, ms.ToArray());
                }
                this.storeHeaderCount += 2000;
            }

            this.manager.PutBlock(batch, header, 0L);
            this.manager.PutCurrentHeader(batch, header);
            this.manager.BatchWrite(WriteOptions.Default, batch);

            this.currentHeaderHeight = header.Height;
            this.currentHeaderHash = header.Hash;
        }

        private void Persist(Block block)
        {
            WriteBatch batch = new WriteBatch();

            long fee = block.Transactions.Sum(p => p.Fee).Value;
            this.manager.PutBlock(batch, block, fee);

            foreach (Transaction tx in block.Transactions)
            {
                this.manager.PutTransaction(batch, block, tx);
                if (block != GenesisBlock && !tx.VerifyBlockchain(this.manager.Storage))
                {
                    if (Fixed8.Zero < tx.Fee)
                        this.manager.Storage.GetAccountState(tx.From).AddBalance(-tx.Fee);

                    byte[] eCodeBytes = BitConverter.GetBytes((Int64)tx.Data.TxResult).Take(8).ToArray();
                    this.manager.PutTransactionResult(batch, tx);
#if DEBUG
                    Logger.Debug("verified == false transaction. " + tx.ToJson());
#endif
                    continue;
                }

                AccountState from = this.manager.Storage.GetAccountState(tx.From);
                if (Fixed8.Zero < tx.Fee)
                    from.AddBalance(-tx.Fee);

                switch (tx.Data)
                {
                    case TransferTransaction transTx:
                        {
                            Fixed8 totalAmount = transTx.To.Sum(p => p.Value);
                            from.AddBalance(-totalAmount);
                            foreach (var i in transTx.To)
                                this.manager.Storage.GetAccountState(i.Key).AddBalance(i.Value);
                        }
                        break;
                    case VoteTransaction voteTx:
                        {
                            from.LastVoteTxID = tx.Hash;
                            this.manager.Storage.Downvote(from.Votes);
                            this.manager.Storage.Vote(voteTx);
                            from.SetVote(voteTx.Votes);
                        }
                        break;
                    case RegisterDelegateTransaction registerDelegateTx:
                        {
                            this.manager.Storage.AddDelegate(registerDelegateTx.From, registerDelegateTx.Name);
                        }
                        break;
                    case OtherSignTransaction osignTx:
                        {
                            Fixed8 totalAmount = osignTx.To.Sum(p => p.Value);
                            from.AddBalance(-totalAmount);
                            this.manager.Storage.GetBlockTriggers(osignTx.ExpirationBlockHeight).TransactionHashes.Add(osignTx.Owner.Hash);
                            this.manager.Storage.AddOtherSignTxs(osignTx.Owner.Hash, osignTx.Others);
                        }
                        break;
                    case SignTransaction signTx:
                        {
                            OtherSignTransactionState osignState = this.manager.Storage.GetOtherSignTxs(signTx.SignTxHash);
                            if (osignState != null && osignState.Sign(signTx.Owner.Signature) && osignState.RemainSign.Count == 0)
                            {
                                OtherSignTransaction osignTx = this.manager.Storage.GetTransaction(osignState.TxHash).Data as OtherSignTransaction;
                                foreach (var i in osignTx.To)
                                    this.manager.Storage.GetAccountState(i.Key).AddBalance(i.Value);
                                BlockTriggerState state = this.manager.Storage.GetBlockTriggers(signTx.Reference.ExpirationBlockHeight);
                                state.TransactionHashes.Remove(signTx.SignTxHash);
                            }
                        }
                        break;

                    case LockTransaction lockTx:
                        {
                            from.LastLockTxID = tx.Hash;
                            from.AddBalance(-lockTx.LockValue);
                            from.AddLock(lockTx.LockValue);
                        }
                        break;

                    case UnlockTransaction unlockTx:
                        {
                            from.LastLockTxID = tx.Hash;
                            Fixed8 lockValue = from.LockBalance;
                            from.AddBalance(lockValue);
                            from.AddLock(-lockValue);
                        }
                        break;
                    case SupplyTransaction rewardTx:
                        {
                            from.AddBalance(rewardTx.Supply);
                        }
                        break;
                }
            }

            BlockTriggerState blockTrigger = this.manager.Storage.TryBlockTriggers(block.Height);
            if (blockTrigger != null)
            {
                foreach (UInt256 txhash in blockTrigger.TransactionHashes)
                {
                    Transaction tx = this.manager.Storage.GetTransaction(txhash);
                    switch (tx.Data)
                    {
                        case OtherSignTransaction osignTx:
                            {
                                this.manager.Storage.GetAccountState(osignTx.From).AddBalance(osignTx.To.Sum(p => p.Value));
                            }
                            break;
                    }
                }
            }

            if (0 < block.Height)
            {
                AccountState producer = this.manager.Storage.GetAccountState(WalletAccount.ToAddressHash(block.Header.Signature.Pubkey));
                producer.AddBalance(Config.Instance.BlockReward);
            }

            this.manager.Storage.commit(batch, block.Height);
            this.manager.PutCurrentBlock(block);
            this.manager.BatchWrite(WriteOptions.Default, batch);

            this.currentBlockHeight = block.Header.Height;
            this.currentBlockHash = block.Header.Hash;

            this.cacheChain.AddBlock(block);

            Logger.Debug("persist block : " + block.Height);
        }
        #endregion


        #region External Method
        public void Initialize(string path, Block genesisBlock)
        {
            Version version;
            Slice value;    
            ReadOptions options = new ReadOptions { FillCache = false };

            this.genesisBlock = genesisBlock;
            this.manager = new LevelDBBlockChain(path);
            if (this.manager.TryGetVersion(out version))
            {
                int blockHeight = 0;
                UInt256 blockHash = UInt256.Zero;
                IEnumerable<UInt256> headerHashs;

                if (this.manager.TryGetCurrentBlock(out this.currentBlockHash, out this.currentBlockHeight))
                {
                    headerHashs = this.manager.GetHeaderHashList();

                    int height = 0;
                    foreach (UInt256 headerHash in headerHashs)
                    {
                        this.cacheChain.AddHeaderIndex(height++, headerHash);
                        ++this.storeHeaderCount;
                    }

                    if (!this.manager.TryGetCurrentHeader(out this.currentHeaderHash, out this.currentHeaderHeight))
                    {
                        this.currentHeaderHash = this.currentBlockHash;
                        this.currentHeaderHeight = this.currentBlockHeight;
                    }

                    if (this.storeHeaderCount == 0)
                    {
                        foreach (BlockHeader blockHeader in this.manager.GetBlockHeaderList())
                            this.cacheChain.AddHeaderIndex(blockHeader.Height, blockHeader.Hash);
                    }
                    else if (this.storeHeaderCount <= this.currentHeaderHeight)
                    {
                        for (UInt256 hash = this.currentHeaderHash; hash != this.cacheChain.HeaderIndices[(int)this.storeHeaderCount - 1];)
                        {
                            BlockHeader header = this.manager.GetBlockHeader(hash);
                            this.cacheChain.AddHeaderIndex(header.Height, header.Hash);
                            hash = header.PrevHash;
                        }
                    }
                    this.proof.SetTurnTable(this.manager.GetCurrentTurnTable());
                }
            }
            else
            {
                this.cacheChain.AddHeaderIndex(genesisBlock.Height, genesisBlock.Hash);
                this.currentBlockHash = genesisBlock.Hash;
                this.currentHeaderHash = genesisBlock.Hash;
                Persist(genesisBlock);
                this.manager.PutVersion(Assembly.GetExecutingAssembly().GetName().Version);
                this.proof.Update(this);
            }

            Task.Run(() => PersistBlocksLoop());
        }

        public ERROR_BLOCK AddBlock(Block block)
        {
            lock (this.waitPersistBlocks)
            {
                if (!this.waitPersistBlocks.ContainsKey(block.Hash))
                    this.waitPersistBlocks.Add(block.Hash, block);
            }

            lock (PoolLock)
            {
                foreach (var tx in block.Transactions)
                {
                    if (this.rxPool.ContainsKey(tx.Hash))
                        continue;
                    if (this.txPool.ContainsKey(tx.Hash))
                        continue;
                    this.txPool.Add(tx.Hash, tx);
                }
            }

            int height = this.cacheChain.HeaderHeight;
            if (height + 1 == block.Height)
            {
                if (!block.Verify())
                    return ERROR_BLOCK.ERROR;
                WriteBatch batch = new WriteBatch();
                AddHeader(block.Header);
                this.cacheChain.AddBlock(block);
                this.newBlockEvent.Set();
            }
            else
                return ERROR_BLOCK.ERROR_HEIGHT;

            return ERROR_BLOCK.NO_ERROR;
        }

        public bool AddBlockDirectly(Block block)
        {
            if (block.Height != CurrentBlockHeight + 1)
                return false;

            int height = this.cacheChain.HeaderHeight;
            if (height + 1 == block.Height)
            {
                AddHeader(block.Header);
                lock (PersistLock)
                {
                    Persist(block);
                    if (0 >= this.proof.RemainUpdate(block.Height))
                        this.proof.Update(this);
                    OnPersistCompleted(block);
                }
                this.cacheChain.AddBlock(block);
                return true;
            }
            return false;
        }

        // TODO : clean
        public bool VerityBlock(Block block)
        {
            Storage snapshot = this.manager.SnapShot;
            List<Transaction> errList = new List<Transaction>();

            foreach (Transaction tx in block.Transactions)
            {
                if (block != GenesisBlock && (!tx.Verify() || !tx.VerifyBlockchain(snapshot)))
                {
                    errList.Add(tx);
                    lock (PoolLock)
                    {
                        this.txPool.Remove(tx.Hash);
                    }
                    continue;
                }

                AccountState from = snapshot.GetAccountState(tx.From);
                if (Fixed8.Zero < tx.Fee)
                    from.AddBalance(-tx.Fee);

                switch (tx.Data)
                {
                    case TransferTransaction transTx:
                        {
                            Fixed8 totalAmount = transTx.To.Sum(p => p.Value);
                            from.AddBalance(-totalAmount);
                            foreach (var i in transTx.To)
                                snapshot.GetAccountState(i.Key).AddBalance(i.Value);
                        }
                        break;
                    case VoteTransaction voteTx:
                        {
                            from.LastVoteTxID = tx.Hash;
                            snapshot.Downvote(from.Votes);
                            snapshot.Vote(voteTx);
                            from.SetVote(voteTx.Votes);
                        }
                        break;
                    case RegisterDelegateTransaction registerDelegateTx:
                        {
                            snapshot.AddDelegate(registerDelegateTx.From, registerDelegateTx.Name);
                        }
                        break;
                    case OtherSignTransaction osignTx:
                        {
                            Fixed8 totalAmount = osignTx.To.Sum(p => p.Value);
                            from.AddBalance(-totalAmount);
                            snapshot.GetBlockTriggers(osignTx.ExpirationBlockHeight).TransactionHashes.Add(osignTx.Owner.Hash);
                            snapshot.AddOtherSignTxs(osignTx.Owner.Hash, osignTx.Others);
                        }
                        break;
                    case SignTransaction signTx:
                        {
                            OtherSignTransactionState osignState = snapshot.GetOtherSignTxs(signTx.SignTxHash);
                            if (osignState != null && osignState.Sign(signTx.Owner.Signature) && osignState.RemainSign.Count == 0)
                            {
                                OtherSignTransaction osignTx = snapshot.GetTransaction(osignState.TxHash).Data as OtherSignTransaction;
                                foreach (var i in osignTx.To)
                                    snapshot.GetAccountState(i.Key).AddBalance(i.Value);
                                BlockTriggerState state = snapshot.GetBlockTriggers(signTx.Reference.ExpirationBlockHeight);
                                state.TransactionHashes.Remove(signTx.SignTxHash);
                            }
                        }
                        break;
                    case LockTransaction lockTx:
                        {
                            from.LastLockTxID = tx.Hash;
                            from.AddBalance(-lockTx.LockValue);
                            from.AddLock(lockTx.LockValue);
                        }
                        break;
                    case UnlockTransaction unlockTx:
                        {
                            from.LastLockTxID = tx.Hash;
                            Fixed8 lockValue = from.LockBalance;
                            from.AddBalance(lockValue);
                            from.AddLock(-lockValue);
                        }
                        break;
                    case SupplyTransaction rewardTx:
                        {
                            from.AddBalance(rewardTx.Supply);
                        }
                        break;
                }
            }

            BlockTriggerState blockTrigger = snapshot.TryBlockTriggers(block.Height);
            if (blockTrigger != null)
            {
                foreach (UInt256 txhash in blockTrigger.TransactionHashes)
                {
                    Transaction tx = snapshot.GetTransaction(txhash);
                    switch (tx.Data)
                    {
                        case OtherSignTransaction osignTx:
                            {
                                snapshot.GetAccountState(osignTx.From).AddBalance(osignTx.To.Sum(p => p.Value));
                            }
                            break;
                    }
                }
            }
            snapshot.Dispose();
            if (errList.Count == 0)
                return true;
            var list = block.Transactions.Except(errList);
            block.Transactions.Clear();
            block.Transactions.AddRange(list);
            return false;
        }

        public void PersistTurnTable(List<UInt160> addrs, int height)
        {
            WriteBatch batch = new WriteBatch();
            TurnTableState state = new TurnTableState();
            state.SetTurnTable(addrs, height);
            this.manager.PutTurnTable(state);
        }

        public void OnPersistCompleted(Block block)
        {
            RemoveTransactionPool(block.Transactions);
            PersistCompleted?.Invoke(this, block);
        }

        public void SetCacheBlockCapacity(int capacity) { cacheChain.SetCapacity(capacity); }

        public void Dispose()
        {
            this.disposed = true;
            this.newBlockEvent.Set();
            this.newBlockEvent.Dispose();
            this.manager.Dispose();
        }
        #endregion

    }
}