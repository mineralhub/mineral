using Mineral.Core.State;
using Mineral.Core.Transactions;
using Mineral.Cryptography;
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
    public enum ERROR_BLOCK
    {
        NO_ERROR = 0,
        ERROR = 1,
        ERROR_HEIGHT = 2,
        ERROR_HASH = 3,
    };

    public partial class BlockChain : IDisposable
    {
        #region Fields
        private Proof _proof = null;
        private Block _genesisBlock = null;
        private static BlockChain _instance = null;
        private CacheChain _cacheChain = null;

        private bool _disposed = false;
        private Thread _threadPersist = null;
        public object PoolLock { get; } = new object();
        public object PersistLock { get; } = new object();
        public event EventHandler<Block> PersistCompleted;
        private uint _storeHeaderCount = 0;
        private uint _currentBlockHeight = 0;
        private UInt256 _currentBlockHash = UInt256.Zero;
        private const int _defaultCacheCapacity = 200000;
        protected Dictionary<UInt256, Transaction> _rxPool = new Dictionary<UInt256, Transaction>();
        protected Dictionary<UInt256, Transaction> _txPool = new Dictionary<UInt256, Transaction>();
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
        public Block GenesisBlock { get { return _genesisBlock; } }
        public uint CurrentHeaderHeight { get { return _cacheChain.HeaderHeight; } }
        public UInt256 CurrentHeaderHash { get { return _cacheChain.HeaderHash; } }
        public uint CurrentBlockHeight { get { return _currentBlockHeight; } }
        public UInt256 CurrentBlockHash { get { return _currentBlockHash; } }
        public uint CacheBlockCapacity { get { return _cacheChain.Capacity; } set { _cacheChain.Capacity = value; } }
        #endregion


        #region Event Method
        private void PersistBlocksLoop()
        {
            LinkedList<Block> blocks = new LinkedList<Block>();

            while (!_disposed)
            {
                uint height = CurrentBlockHeight + 1;
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
            AddHeader(block.Header);

            using (Storage snapshot = _dbManager.SnapShot)
            {
                Fixed8 fee = block.Transactions.Sum(p => p.Fee);
                snapshot.Block.Add(block.Header.Hash, block, fee);

                foreach (Transaction tx in block.Transactions)
                {
                    snapshot.Transaction.Add(tx.Hash, block.Header.Height, tx);
                    if (_genesisBlock != block && !tx.VerifyBlockChain(_dbManager.Storage))
                    {
                        if (Fixed8.Zero < tx.Fee)
                        {
                            snapshot.Account.GetAndChange(tx.From).AddBalance(-tx.Fee);
                        }
                        snapshot.TransactionResult.Add(tx.Hash, tx.TxResult);
#if DEBUG
                        Logger.Debug("verified == false transaction. " + tx.ToJson());
#endif
                        continue;
                    }

                    AccountState from = snapshot.Account.GetAndChange(tx.From);
                    if (Fixed8.Zero < tx.Fee)
                        from.AddBalance(-tx.Fee);

                    switch (tx.Data)
                    {
                        case TransferTransaction transTx:
                            {
                                Fixed8 totalAmount = transTx.To.Sum(p => p.Value);
                                from.AddBalance(-totalAmount);
                                foreach (var i in transTx.To)
                                {
                                    snapshot.Account.GetAndChange(i.Key).AddBalance(i.Value);
                                }
                            }
                            break;
                        case VoteTransaction voteTx:
                            {
                                from.LastVoteTxID = tx.Hash;
                                snapshot.Delegate.Downvote(from.Votes);
                                snapshot.Delegate.Vote(voteTx);
                                from.SetVote(voteTx.Votes);
                            }
                            break;
                        case RegisterDelegateTransaction registerDelegateTx:
                            {
                                snapshot.Delegate.Add(registerDelegateTx.From, registerDelegateTx.Name);
                            }
                            break;
                        case OtherSignTransaction osignTx:
                            {
                                Fixed8 totalAmount = osignTx.To.Sum(p => p.Value);
                                from.AddBalance(-totalAmount);
                                snapshot.BlockTrigger.GetAndChange(osignTx.ExpirationBlockHeight).TransactionHashes.Add(osignTx.Owner.Hash);
                                snapshot.OtherSign.Add(osignTx.Owner.Hash, osignTx.Others);
                            }
                            break;
                        case SignTransaction signTx:
                            {
                                for (int i = 0; i < signTx.TxHashes.Count; ++i)
                                {
                                    OtherSignTransactionState state = snapshot.OtherSign.GetAndChange(signTx.TxHashes[i]);
                                    state.Sign(signTx.Owner.Signature);
                                    if (state.RemainSign.Count == 0)
                                    {
                                        TransactionState txState = snapshot.Transaction.Get(state.TxHash);
                                        if (txState != null)
                                        {
                                            var osign = txState.Transaction.Data as OtherSignTransaction;
                                            foreach (var to in osign.To)
                                                snapshot.Account.GetAndChange(to.Key).AddBalance(to.Value);
                                            var trigger = snapshot.BlockTrigger.GetAndChange(signTx.Reference[i].ExpirationBlockHeight);
                                            trigger.TransactionHashes.Remove(signTx.TxHashes[i]);
                                        }
                                    }
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

                BlockTriggerState blockTrigger = snapshot.BlockTrigger.Get(block.Height);
                if (blockTrigger != null)
                {
                    foreach (UInt256 txhash in blockTrigger.TransactionHashes)
                    {
                        TransactionState txState = snapshot.Transaction.Get(txhash);
                        switch (txState.Transaction.Data)
                        {
                            case OtherSignTransaction osignTx:
                                {
                                    snapshot.Account.GetAndChange(osignTx.From).AddBalance(osignTx.To.Sum(p => p.Value));
                                }
                                break;
                        }
                    }
                }

                if (0 < block.Height)
                {
                    AccountState producer = snapshot.Account.GetAndChange(WalletAccount.ToAddressHash(block.Header.Signature.Pubkey));
                    producer.AddBalance(Config.Instance.BlockReward);
                }

                snapshot.Commit(block.Height);
            }

            WriteBatch batch = new WriteBatch();
            _dbManager.PutCurrentHeader(batch, block.Header);
            _dbManager.PutCurrentBlock(batch, block);
            _dbManager.BatchWrite(WriteOptions.Default, batch);

            _currentBlockHeight = block.Header.Height;
            _currentBlockHash = block.Header.Hash;

            Logger.Debug("persist block : " + block.Height);
        }
        #endregion


        #region External Method
        public void Initialize(string path, Block genesisBlock)
        {
            _genesisBlock = genesisBlock;
            _dbManager = new LevelDBBlockChain(path);

            Version version;
            if (_dbManager.TryGetVersion(out version))
            {
                UInt256 blockHash = UInt256.Zero;
                IEnumerable<UInt256> headerHashs;

                if (_dbManager.TryGetCurrentBlock(out _currentBlockHash, out _currentBlockHeight))
                {
                    _cacheChain = new CacheChain((uint)(_currentBlockHeight * 1.1F));
                    headerHashs = _dbManager.GetHeaderHashList();
                    foreach (UInt256 headerHash in headerHashs)
                    {
                        _cacheChain.AddHeaderHash(_storeHeaderCount++, headerHash);
                    }

                    if (_storeHeaderCount == 0)
                    {
                        foreach (BlockHeader blockHeader in _dbManager.GetBlockHeaderList())
                            _cacheChain.AddHeaderHash(blockHeader.Height, blockHeader.Hash);
                    }
                    else if (_storeHeaderCount <= _currentBlockHeight)
                    {
                        UInt256 hash = _currentBlockHash;
                        Dictionary<uint, UInt256> headers = new Dictionary<uint, UInt256>();

                        while (hash != _cacheChain.GetBlockHash((uint)_cacheChain.HeaderCount - 1))
                        {
                            BlockState blockState = _dbManager.Storage.Block.Get(hash);
                            if (blockState != null)
                            {
                                headers.Add(blockState.Header.Height, blockState.Header.Hash);
                                hash = blockState.Header.PrevHash;
                            }
                        }

                        foreach (var header in headers.OrderBy(x => x.Key))
                            _cacheChain.AddHeaderHash(header.Key, header.Value);
                    }
                    _proof.SetTurnTable(_dbManager.GetCurrentTurnTable());
                }
            }
            else
            {
                _cacheChain = new CacheChain(_defaultCacheCapacity);
                _cacheChain.AddHeaderHash(genesisBlock.Height, genesisBlock.Hash);
                _currentBlockHash = genesisBlock.Hash;
                Persist(genesisBlock);
                _dbManager.PutVersion(Assembly.GetExecutingAssembly().GetName().Version);
                _proof.Update(this);
            }

            _threadPersist = new Thread(PersistBlocksLoop)
            {
                IsBackground = true,
                Name = "Mineral.BlockChain.PersistBlocksLoop"
            };
            _threadPersist.Start();
        }

        public void AddHeader(BlockHeader header)
        {
            WriteBatch batch = new WriteBatch();
            uint oStoreHeaderCount = _storeHeaderCount;

            while (2000 <= header.Height - _storeHeaderCount)
            {
                using (MemoryStream ms = new MemoryStream())
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.WriteSerializableArray(_cacheChain.GetBlcokHashs(_storeHeaderCount, _storeHeaderCount + 2000));
                    bw.Flush();
                    _dbManager.PutHeaderHashList(batch, (int)_storeHeaderCount, ms.ToArray());
                }
                _storeHeaderCount += 2000;
            }

            if (_storeHeaderCount > oStoreHeaderCount)
            {
                _dbManager.BatchWrite(WriteOptions.Default, batch);
            }
        }

        public ERROR_BLOCK AddBlock(Block block)
        {
            if (!_cacheChain.AddHeaderHash(block.Height, block.Hash))
                return ERROR_BLOCK.ERROR_HEIGHT;

            var err = _cacheChain.AddBlock(block);
            if (err != ERROR_BLOCK.NO_ERROR)
                return err;
            return err;
        }

        public ERROR_BLOCK AddBlockDirectly(Block block)
        {
            if (!_cacheChain.AddHeaderHash(block.Height, block.Hash))
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
            Storage snapshot = _dbManager.SnapShot;
            List<Transaction> errList = new List<Transaction>();

            foreach (Transaction tx in block.Transactions)
            {
                if (block != GenesisBlock && (!tx.Verify() || !tx.VerifyBlockChain(snapshot)))
                {
                    errList.Add(tx);
                    lock (PoolLock)
                    {
                        _txPool.Remove(tx.Hash);
                    }
                    continue;
                }

                AccountState from = snapshot.Account.GetAndChange(tx.From);
                if (Fixed8.Zero < tx.Fee)
                    from.AddBalance(-tx.Fee);

                switch (tx.Data)
                {
                    case TransferTransaction transTx:
                        {
                            Fixed8 totalAmount = transTx.To.Sum(p => p.Value);
                            from.AddBalance(-totalAmount);
                            foreach (var i in transTx.To)
                                snapshot.Account.GetAndChange(i.Key).AddBalance(i.Value);
                        }
                        break;
                    case VoteTransaction voteTx:
                        {
                            from.LastVoteTxID = tx.Hash;
                            snapshot.Delegate.Downvote(from.Votes);
                            snapshot.Delegate.Vote(voteTx);
                            from.SetVote(voteTx.Votes);
                        }
                        break;
                    case RegisterDelegateTransaction registerDelegateTx:
                        {
                            snapshot.Delegate.Add(registerDelegateTx.From, registerDelegateTx.Name);
                        }
                        break;
                    case OtherSignTransaction osignTx:
                        {
                            Fixed8 totalAmount = osignTx.To.Sum(p => p.Value);
                            from.AddBalance(-totalAmount);
                            snapshot.BlockTrigger.GetAndChange(osignTx.ExpirationBlockHeight).TransactionHashes.Add(osignTx.Owner.Hash);
                            snapshot.OtherSign.Add(osignTx.Owner.Hash, osignTx.Others);
                        }
                        break;
                    case SignTransaction signTx:
                        {
                            for (int i = 0; i < signTx.TxHashes.Count; ++i)
                            {
                                OtherSignTransactionState state = _dbManager.Storage.OtherSign.GetAndChange(signTx.TxHashes[i]);
                                state.Sign(signTx.Owner.Signature);
                                if (state.RemainSign.Count == 0)
                                {
                                    TransactionState txState = _dbManager.Storage.Transaction.Get(state.TxHash);
                                    if (txState != null)
                                    {
                                        var osign = txState.Transaction.Data as OtherSignTransaction;
                                        foreach (var to in osign.To)
                                            _dbManager.Storage.Account.GetAndChange(to.Key).AddBalance(to.Value);
                                        var trigger = _dbManager.Storage.BlockTrigger.GetAndChange(signTx.Reference[i].ExpirationBlockHeight);
                                        trigger.TransactionHashes.Remove(signTx.TxHashes[i]);
                                    }
                                }
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

            BlockTriggerState blockTrigger = snapshot.BlockTrigger.Get(block.Height);
            if (blockTrigger != null)
            {
                foreach (UInt256 txhash in blockTrigger.TransactionHashes)
                {
                    TransactionState txState = snapshot.Transaction.Get(txhash);
                    switch (txState.Transaction.Data)
                    {
                        case OtherSignTransaction osignTx:
                            {
                                snapshot.Account.GetAndChange(osignTx.From).AddBalance(osignTx.To.Sum(p => p.Value));
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

        public void PersistTurnTable(List<UInt160> addrs, uint height)
        {
            WriteBatch batch = new WriteBatch();
            TurnTableState state = new TurnTableState();
            state.SetTurnTable(addrs, height);
            _dbManager.PutTurnTable(state);
        }

        public void OnPersistCompleted(Block block)
        {
            RemoveTransactionPool(block.Transactions);
            PersistCompleted?.Invoke(this, block);
        }

        public void Dispose()
        {
            _disposed = true;
            _dbManager.Dispose();
        }
        #endregion

    }
}