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
        private static BlockChain _instance = null;
        private Proof _proof = null;
        private Block _genesisBlock = null;

        private AutoResetEvent _newBlockEvent = new AutoResetEvent(false);
        public event EventHandler<Block> PersistCompleted;
        public object PersistLock { get; } = new object();
        public object PoolLock { get; } = new object();

        private int _currentBlockHeight = 0;
        private UInt256 _currentBlockHash = UInt256.Zero;
        private int _currentHeaderHeight = 0;
        private UInt256 _currentHeaderHash = UInt256.Zero;

        private CacheChain _cacheChain = new CacheChain();
        private Dictionary<UInt256, Block> _waitPersistBlocks = new Dictionary<UInt256, Block>();
        private Dictionary<UInt256, BlockHeader> _cacheHeaders = new Dictionary<UInt256, BlockHeader>();

        protected Dictionary<UInt256, Transaction> _rxPool = new Dictionary<UInt256, Transaction>();
        protected Dictionary<UInt256, Transaction> _txPool = new Dictionary<UInt256, Transaction>();

        private uint _storeHeaderCount = 0;
        private bool _disposed = false;
        private Thread _persistThread;
        #endregion


        #region Properties
        public static BlockChain Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BlockChain();
                    _instance._proof = new DPos.DPos();
                }
                return _instance;
            }
        }

        public Proof Proof { get { return _proof; } }

        public int CurrentBlockHeight { get { return _currentBlockHeight; } }
        public UInt256 CurrentBlockHash { get { return _currentBlockHash; } }
        public int CurrentHeaderHeight { get { return _currentHeaderHeight; } }
        public UInt256 CurrentHeaderHash { get { return _currentHeaderHash; } }
        public Block GenesisBlock { get { return _genesisBlock; } }
        #endregion


        #region Event Method
        private void PersistBlocksLoop()
        {
            while (!_disposed)
            {
                _newBlockEvent.WaitOne();

                while (!_disposed)
                {
                    if (!_cacheChain.HeaderIndices.TryGetValue(_currentBlockHeight + 1, out UInt256 hash))
                        break;

                    Block block;
                    lock (_waitPersistBlocks)
                    {
                        if (!_waitPersistBlocks.TryGetValue(hash, out block))
                            break;
                    }

                    // Compare block's previous hash is CurrentBlockHash
                    if (block.Header.PrevHash != CurrentBlockHash)
                        break;

                    lock (PersistLock)
                    {
                        Persist(block);
                        if (0 >= _proof.RemainUpdate(block.Height))
                            _proof.Update(this);
                        OnPersistCompleted(block);
                    }
                    lock (_waitPersistBlocks)
                    {
                        _waitPersistBlocks.Remove(hash);
                    }
                }
            }
        }
        #endregion


        #region Internal Method
        private void AddHeader(BlockHeader header)
        {
            WriteBatch batch = new WriteBatch();

            _cacheChain.AddHeaderIndex(header.Height, header.Hash);
            while (_storeHeaderCount <= header.Height - 2000)
            {
                using (MemoryStream ms = new MemoryStream())
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.WriteSerializableArray(_cacheChain.HeaderIndices.Values.Skip((int)_storeHeaderCount).Take(2000));
                    bw.Flush();
                    _manager.PutHeaderHashList(batch, (int)_storeHeaderCount, ms.ToArray());
                }
                _storeHeaderCount += 2000;
            }

            _manager.PutBlock(batch, header, 0L);
            _manager.PutCurrentHeader(batch, header);
            _manager.BatchWrite(WriteOptions.Default, batch);

            _currentHeaderHeight = header.Height;
            _currentHeaderHash = header.Hash;
        }

        private void Persist(Block block)
        {
            WriteBatch batch = new WriteBatch();

            long fee = block.Transactions.Sum(p => p.Fee).Value;
            _manager.PutBlock(batch, block, fee);

            foreach (Transaction tx in block.Transactions)
            {
                _manager.PutTransaction(batch, block, tx);
                if (block != GenesisBlock && !tx.VerifyBlockchain(_manager.Storage))
                {
                    if (Fixed8.Zero < tx.Fee)
                        _manager.Storage.GetAccountState(tx.From).AddBalance(-tx.Fee);

                    byte[] eCodeBytes = BitConverter.GetBytes((Int64)tx.Data.TxResult).Take(8).ToArray();
                    _manager.PutTransactionResult(batch, tx);
#if DEBUG
                    Logger.Debug("verified == false transaction. " + tx.ToJson());
#endif
                    continue;
                }

                AccountState from = _manager.Storage.GetAccountState(tx.From);
                if (Fixed8.Zero < tx.Fee)
                    from.AddBalance(-tx.Fee);

                switch (tx.Data)
                {
                    case TransferTransaction transTx:
                        {
                            Fixed8 totalAmount = transTx.To.Sum(p => p.Value);
                            from.AddBalance(-totalAmount);
                            foreach (var i in transTx.To)
                                _manager.Storage.GetAccountState(i.Key).AddBalance(i.Value);
                        }
                        break;
                    case VoteTransaction voteTx:
                        {
                            from.LastVoteTxID = tx.Hash;
                            _manager.Storage.Downvote(from.Votes);
                            _manager.Storage.Vote(voteTx);
                            from.SetVote(voteTx.Votes);
                        }
                        break;
                    case RegisterDelegateTransaction registerDelegateTx:
                        {
                            _manager.Storage.AddDelegate(registerDelegateTx.From, registerDelegateTx.Name);
                        }
                        break;
                    case OtherSignTransaction osignTx:
                        {
                            Fixed8 totalAmount = osignTx.To.Sum(p => p.Value);
                            from.AddBalance(-totalAmount);
                            _manager.Storage.GetBlockTriggers(osignTx.ExpirationBlockHeight).TransactionHashes.Add(osignTx.Owner.Hash);
                            _manager.Storage.AddOtherSignTxs(osignTx.Owner.Hash, osignTx.Others);
                        }
                        break;
                    case SignTransaction signTx:
                        {
                            OtherSignTransactionState osignState = _manager.Storage.GetOtherSignTxs(signTx.SignTxHash);
                            if (osignState != null && osignState.Sign(signTx.Owner.Signature) && osignState.RemainSign.Count == 0)
                            {
                                OtherSignTransaction osignTx = _manager.Storage.GetTransaction(osignState.TxHash).Data as OtherSignTransaction;
                                foreach (var i in osignTx.To)
                                    _manager.Storage.GetAccountState(i.Key).AddBalance(i.Value);
                                BlockTriggerState state = _manager.Storage.GetBlockTriggers(signTx.Reference.ExpirationBlockHeight);
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

            BlockTriggerState blockTrigger = _manager.Storage.TryBlockTriggers(block.Height);
            if (blockTrigger != null)
            {
                foreach (UInt256 txhash in blockTrigger.TransactionHashes)
                {
                    Transaction tx = _manager.Storage.GetTransaction(txhash);
                    switch (tx.Data)
                    {
                        case OtherSignTransaction osignTx:
                            {
                                _manager.Storage.GetAccountState(osignTx.From).AddBalance(osignTx.To.Sum(p => p.Value));
                            }
                            break;
                    }
                }
            }

            if (0 < block.Height)
            {
                AccountState producer = _manager.Storage.GetAccountState(WalletAccount.ToAddressHash(block.Header.Signature.Pubkey));
                producer.AddBalance(Config.Instance.BlockReward);
            }

            _manager.Storage.commit(batch, block.Height);
            _manager.PutCurrentBlock(block);
            _manager.BatchWrite(WriteOptions.Default, batch);

            _currentBlockHeight = block.Header.Height;
            _currentBlockHash = block.Header.Hash;

            _cacheChain.AddBlock(block);

            Logger.Debug("persist block : " + block.Height);
        }
        #endregion


        #region External Method
        public void Initialize(string path, Block genesisBlock)
        {
            Version version;
            Slice value;    

            _genesisBlock = genesisBlock;
            _manager = new LevelDBBlockChain(path);
            if (_manager.TryGetVersion(out version))
            {
                UInt256 blockHash = UInt256.Zero;
                IEnumerable<UInt256> headerHashs;

                if (_manager.TryGetCurrentBlock(out _currentBlockHash, out _currentBlockHeight))
                {
                    headerHashs = _manager.GetHeaderHashList();

                    int height = 0;
                    foreach (UInt256 headerHash in headerHashs)
                    {
                        _cacheChain.AddHeaderIndex(height++, headerHash);
                        ++_storeHeaderCount;
                    }

                    if (!_manager.TryGetCurrentHeader(out _currentHeaderHash, out _currentHeaderHeight))
                    {
                        _currentHeaderHash = _currentBlockHash;
                        _currentHeaderHeight = _currentBlockHeight;
                    }

                    if (_storeHeaderCount == 0)
                    {
                        foreach (BlockHeader blockHeader in _manager.GetBlockHeaderList())
                            _cacheChain.AddHeaderIndex(blockHeader.Height, blockHeader.Hash);
                    }
                    else if (_storeHeaderCount <= _currentHeaderHeight)
                    {
                        for (UInt256 hash = _currentHeaderHash; hash != _cacheChain.HeaderIndices[(int)_storeHeaderCount - 1];)
                        {
                            BlockHeader header = _manager.GetBlockHeader(hash);
                            _cacheChain.AddHeaderIndex(header.Height, header.Hash);
                            hash = header.PrevHash;
                        }
                    }
                    _proof.SetTurnTable(_manager.GetCurrentTurnTable());
                }
            }
            else
            {
                _cacheChain.AddHeaderIndex(genesisBlock.Height, genesisBlock.Hash);
                _currentBlockHash = genesisBlock.Hash;
                _currentHeaderHash = genesisBlock.Hash;
                Persist(genesisBlock);
                _manager.PutVersion(Assembly.GetExecutingAssembly().GetName().Version);
                _proof.Update(this);
            }

            _persistThread = new Thread(PersistBlocksLoop)
            {
                IsBackground = true,
                Name = "Mineral.BlockChain.PersistBlocksLoop"
            };
            _persistThread.Start();
        }

        public ERROR_BLOCK AddBlock(Block block)
        {
            lock (_waitPersistBlocks)
            {
                if (!_waitPersistBlocks.ContainsKey(block.Hash))
                    _waitPersistBlocks.Add(block.Hash, block);
            }

            lock (PoolLock)
            {
                foreach (var tx in block.Transactions)
                {
                    if (_rxPool.ContainsKey(tx.Hash))
                        continue;
                    if (_txPool.ContainsKey(tx.Hash))
                        continue;
                    _txPool.Add(tx.Hash, tx);
                }
            }

            int height = _cacheChain.HeaderHeight;
            if (height + 1 == block.Height)
            {
                if (!block.Verify())
                    return ERROR_BLOCK.ERROR;
                WriteBatch batch = new WriteBatch();
                AddHeader(block.Header);
                _cacheChain.AddBlock(block);
                _newBlockEvent.Set();
            }
            else
                return ERROR_BLOCK.ERROR_HEIGHT;

            return ERROR_BLOCK.NO_ERROR;
        }

        public bool AddBlockDirectly(Block block)
        {
            if (block.Height != CurrentBlockHeight + 1)
                return false;

            int height = _cacheChain.HeaderHeight;
            if (height + 1 == block.Height)
            {
                AddHeader(block.Header);
                lock (PersistLock)
                {
                    Persist(block);
                    if (0 >= _proof.RemainUpdate(block.Height))
                        _proof.Update(this);
                    OnPersistCompleted(block);
                }
                _cacheChain.AddBlock(block);
                return true;
            }
            return false;
        }

        // TODO : clean
        public bool VerityBlock(Block block)
        {
            Storage snapshot = _manager.SnapShot;
            List<Transaction> errList = new List<Transaction>();

            foreach (Transaction tx in block.Transactions)
            {
                if (block != GenesisBlock && (!tx.Verify() || !tx.VerifyBlockchain(snapshot)))
                {
                    errList.Add(tx);
                    lock (PoolLock)
                    {
                        _txPool.Remove(tx.Hash);
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
            _manager.PutTurnTable(state);
        }

        public void OnPersistCompleted(Block block)
        {
            RemoveTransactionPool(block.Transactions);
            PersistCompleted?.Invoke(this, block);
        }

        public void SetCacheBlockCapacity(int capacity) { _cacheChain.SetCapacity(capacity); }

        public void Dispose()
        {
            _disposed = true;
            _newBlockEvent.Set();
            _newBlockEvent.Dispose();
            _manager.Dispose();
        }
        #endregion

    }
}