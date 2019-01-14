using Mineral.Core.Transactions;
using Mineral.Database.BlockChain;
using Mineral.Database.LevelDB;
using Mineral.Utils;
using Mineral.Wallets;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mineral.Core
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

    public partial class BlockChain : IDisposable
    {
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

        private CacheChain _cacheChain = new CacheChain();

        protected Dictionary<UInt256, Transaction> _rxPool = new Dictionary<UInt256, Transaction>();
        protected Dictionary<UInt256, Transaction> _txPool = new Dictionary<UInt256, Transaction>();

        private uint _storeHeaderCount = 0;
        private bool _disposed = false;
        private Thread _threadPersist;
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

        public int CurrentHeaderHeight { get { return _cacheChain.HeaderHeight; } }
        public UInt256 CurrentHeaderHash { get { return _cacheChain.HeaderHash; } }
        public int CurrentBlockHeight { get { return _currentBlockHeight; } }
        public UInt256 CurrentBlockHash { get { return _currentBlockHash; } }
        public Block GenesisBlock { get { return _genesisBlock; } }
        #endregion


        #region Event Method
        private void PersistBlocksLoop()
        {
            LinkedList<Block> blocks = new LinkedList<Block>();

            while (!_disposed)
            {
                int height = CurrentBlockHeight + 1;
                for (Block block = _cacheChain.GetBlock(height); block != null; height++)
                {
                    blocks.AddLast(block);
                    block = _cacheChain.GetBlock(height);
                }

                if (blocks.Count == 0)
                {
                    Thread.Sleep(30);
                    continue;
                }

                while (blocks.Count > 0)
                {
                    Block block = blocks.First();
                    blocks.RemoveFirst();

                    if (!block.Verify()) 
                        continue;

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

                    if (block.Header.PrevHash != _currentBlockHash) 
                        break;

                    lock (PersistLock)
                    {
                        Persist(block);
                        if (0 >= _proof.RemainUpdate(block.Height))
                            _proof.Update(this);
                        OnPersistCompleted(block);
                    }
                }
            }
        }
        #endregion


        #region Internal Method
        private void Persist(Block block)
        {
            WriteBatch batch = new WriteBatch();
            while (_storeHeaderCount <= block.Header.Height - 2000)
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

            long fee = block.Transactions.Sum(p => p.Fee).Value;
            _manager.PutBlock(batch, block, fee);

            foreach (Transaction tx in block.Transactions)
            {
                _manager.PutTransaction(batch, block, tx);
                if (_genesisBlock != block && !tx.VerifyBlockchain(_manager.Storage))
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
            _manager.PutCurrentHeader(batch, block.Header);
            _manager.PutCurrentBlock(batch, block);
            _manager.BatchWrite(WriteOptions.Default, batch);

            _currentBlockHeight = block.Header.Height;
            _currentBlockHash = block.Header.Hash;

            Logger.Debug("persist block : " + block.Height);
        }
        #endregion


        #region External Method
        public void Initialize(string path, Block genesisBlock)
        {
            _genesisBlock = genesisBlock;
            _manager = new LevelDBBlockChain(path);

            Version version;
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

                    if (_storeHeaderCount == 0)
                    {
                        foreach (BlockHeader blockHeader in _manager.GetBlockHeaderList())
                            _cacheChain.AddHeaderIndex(blockHeader.Height, blockHeader.Hash);
                    }
                    else if (_storeHeaderCount <= _currentBlockHeight)
                    {
                        for (UInt256 hash = _currentBlockHash; hash != _cacheChain.HeaderIndices[(int)_storeHeaderCount - 1];)
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
                _currentBlockHash = genesisBlock.Hash;
                Persist(genesisBlock);
                _manager.PutVersion(Assembly.GetExecutingAssembly().GetName().Version);
                _proof.Update(this);
            }

            _threadPersist = new Thread(PersistBlocksLoop)
            {
                IsBackground = true,
                Name = "Mineral.BlockChain.PersistBlocksLoop"
            };
            _threadPersist.Start();
        }

        public ERROR_BLOCK AddBlock(Block block)
        {
            if (!_cacheChain.AddHeaderIndex(block.Height, block.Hash))
                return ERROR_BLOCK.ERROR_HEIGHT;

            var err = _cacheChain.AddBlock(block);
            if (err != ERROR_BLOCK.NO_ERROR)
                return err;
            return err;
        }

        public ERROR_BLOCK AddBlockDirectly(Block block)
        {
            if (!_cacheChain.AddHeaderIndex(block.Height, block.Hash))
                return ERROR_BLOCK.ERROR_HEIGHT;

            var err = _cacheChain.AddBlock(block);
            if (err != ERROR_BLOCK.NO_ERROR)
                return err;

            lock (PersistLock)
            {
                Persist(block);
                if (0 >= _proof.RemainUpdate(block.Height))
                    _proof.Update(this);
                OnPersistCompleted(block);
            }
            return err;
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