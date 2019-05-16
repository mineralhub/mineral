using Mineral.Core2.State;
using Mineral.Core2.Transactions;
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

namespace Mineral.Core2
{
    public partial class BlockChain : IDisposable
    {
        #region Field
        private Proof _proof = null;
        private Block _genesisBlock = null;
        private static BlockChain _instance = null;

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


        #region Property
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
        public uint CurrentHeaderHeight { get { return _manager.CacheBlocks.HeaderHeight; } }
        public UInt256 CurrentHeaderHash { get { return _manager.CacheBlocks.HeaderHash; } }
        public uint CurrentBlockHeight { get { return _currentBlockHeight; } }
        public UInt256 CurrentBlockHash { get { return _currentBlockHash; } }
        public uint CacheBlockCapacity { get { return _manager.CacheBlocks.Capacity; } set { _manager.CacheBlocks.Capacity = value; } }
        #endregion


        #region Constructor
        private BlockChain() { }
        #endregion


        #region Event Method
        private void PersistBlocksLoop()
        {
            LinkedList<Block> blocks = new LinkedList<Block>();

            while (!_disposed)
            {
                uint height = CurrentBlockHeight + 1;
                for (Block block = _manager.CacheBlocks.GetBlock(height); block != null; height++)
                {
                    blocks.AddLast(block);
                    block = _manager.CacheBlocks.GetBlock(height);
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

            using (Storage snapshot = _manager.Chain.SnapShot)
            {
                Fixed8 fee = block.Transactions.Sum(p => p.Fee);
                snapshot.Block.Add(block.Header.Hash, block, fee);

                foreach (Transaction tx in block.Transactions)
                {
                    snapshot.Transaction.Add(tx.Hash, block.Header.Height, tx);
                    if (_genesisBlock != block && !tx.VerifyBlockChain(_manager.Chain.Storage))
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
            _manager.Chain.PutCurrentHeader(batch, block.Header);
            _manager.Chain.PutCurrentBlock(batch, block);
            _manager.Chain.BatchWrite(WriteOptions.Default, batch);

            _currentBlockHeight = block.Header.Height;
            _currentBlockHash = block.Header.Hash;

            Logger.Debug("persist block : " + block.Height);
        }
        #endregion


        #region External Method
        public bool Initialize(Block genesisBlock)
        {
            bool result = _manager.Initialize(genesisBlock);
            if (result)
            {
                _genesisBlock = genesisBlock;

                if (_manager.Chain.TryGetCurrentBlock(out _currentBlockHash, out _currentBlockHeight))
                {
                    _proof.SetTurnTable(_manager.Chain.GetCurrentTurnTable());
                }
                else
                {
                    Persist(genesisBlock);
                    _proof.Update(this);
                }

                _storeHeaderCount = _manager.CacheBlocks.HeaderCount;

                _threadPersist = new Thread(PersistBlocksLoop)
                {
                    IsBackground = true,
                    Name = "Mineral.BlockChain.PersistBlocksLoop"
                };
                _threadPersist.Start();
            }

            return result;
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
                    bw.WriteSerializableArray(_manager.CacheBlocks.GetBlcokHashs(_storeHeaderCount, _storeHeaderCount + 2000));
                    bw.Flush();
                    _manager.Chain.PutHeaderHashList(batch, (int)_storeHeaderCount, ms.ToArray());
                }
                _storeHeaderCount += 2000;
            }

            if (_storeHeaderCount > oStoreHeaderCount)
            {
                _manager.Chain.BatchWrite(WriteOptions.Default, batch);
            }
        }

        public ERROR_BLOCK AddBlock(Block block)
        {
            if (!_manager.CacheBlocks.AddHeaderHash(block.Height, block.Hash))
                return ERROR_BLOCK.ERROR_EXIST_HEIGHT;

            var err = _manager.CacheBlocks.AddBlock(block);
            if (err != ERROR_BLOCK.NO_ERROR)
                return err;
            return err;
        }

        public ERROR_BLOCK AddBlockDirectly(Block block)
        {
            if (!_manager.CacheBlocks.AddHeaderHash(block.Height, block.Hash))
                return ERROR_BLOCK.ERROR_HEIGHT;

            var err = _manager.CacheBlocks.AddBlock(block);
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
            Storage snapshot = _manager.Chain.SnapShot;
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
                                OtherSignTransactionState state = _manager.Chain.Storage.OtherSign.GetAndChange(signTx.TxHashes[i]);
                                state.Sign(signTx.Owner.Signature);
                                if (state.RemainSign.Count == 0)
                                {
                                    TransactionState txState = _manager.Chain.Storage.Transaction.Get(state.TxHash);
                                    if (txState != null)
                                    {
                                        var osign = txState.Transaction.Data as OtherSignTransaction;
                                        foreach (var to in osign.To)
                                            _manager.Chain.Storage.Account.GetAndChange(to.Key).AddBalance(to.Value);
                                        var trigger = _manager.Chain.Storage.BlockTrigger.GetAndChange(signTx.Reference[i].ExpirationBlockHeight);
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
            _manager.Chain.PutTurnTable(state);
        }

        public void OnPersistCompleted(Block block)
        {
            RemoveTransactionPool(block.Transactions);
            PersistCompleted?.Invoke(this, block);
        }

        public void Dispose()
        {
            _disposed = true;
            _manager.Chain.Dispose();
        }
        #endregion
    }
}