using System;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using Mineral;
using Mineral.Core;
using System.Text;
using Mineral.Database.LevelDB;
using Mineral.Database.CacheStorage;
using Mineral.Core.DPos;
using Mineral.Wallets;

namespace Mineral.Database.LevelDB
{
    public class LevelDBBlockchain : Blockchain
    {
        private string _path;
        private DB _db;

        private List<UInt256> _headerIndices = new List<UInt256>();
        private uint _storedHeaderCount = 0;

        private Dictionary<UInt256, Block> _blockCache = new Dictionary<UInt256, Block>();
        private Dictionary<UInt256, BlockHeader> _headerCache = new Dictionary<UInt256, BlockHeader>();

        private AutoResetEvent _newBlockEvent = new AutoResetEvent(false);
        private Thread _threadPersistence = null;

        private bool _disposed = false;

        private Block _genesisBlock;
        private int _currentBlockHeight;
        private UInt256 _currentBlockHash;
        private int _currentHeaderHeight;
        private UInt256 _currentHeaderHash;
        private DPos _dpos = new DPos();

        public override Block GenesisBlock => _genesisBlock;
        public override int CurrentBlockHeight => _currentBlockHeight;
        public override UInt256 CurrentBlockHash => _currentBlockHash;
        public override int CurrentHeaderHeight => _currentHeaderHeight;
        public override UInt256 CurrentHeaderHash => _currentHeaderHash;

        public LevelDBBlockchain(string path, Block genesisBlock)
        {
            _path = path;
            _genesisBlock = genesisBlock;
        }

        public override Storage storage
        {
            get
            {
                return Storage.NewStorage(_db);
            }
        }

        public override void Run()
        {
            Version version;
            Slice value;
            ReadOptions options = new ReadOptions { FillCache = false };
            _db = DB.Open(_path, new Options { CreateIfMissing = true });
            if (_db.TryGet(ReadOptions.Default, SliceBuilder.Begin(DataEntryPrefix.SYS_Version), out value) && Version.TryParse(value.ToString(), out version))
            {
                value = _db.Get(options, SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentBlock));
                _currentBlockHash = new UInt256(value.ToArray().Take(32).ToArray());
                _currentBlockHeight = value.ToArray().ToInt32(32);
                _currentHeaderHash = _currentBlockHash;
                _currentHeaderHeight = _currentBlockHeight;

                if (_db.TryGet(options, SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentHeader), out value))
                {
                    _currentHeaderHash = new UInt256(value.ToArray().Take(32).ToArray());
                    _currentHeaderHeight = value.ToArray().ToInt32(32);
                }

                IEnumerable<UInt256> headerHashes = _db.Find(options, SliceBuilder.Begin(DataEntryPrefix.IX_HeaderHashList), (k, v) =>
                {
                    using (MemoryStream ms = new MemoryStream(v.ToArray(), false))
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        return new
                        {
                            Index = k.ToArray().ToUInt32(1),
                            Hashes = br.ReadSerializableArray<UInt256>()
                        };
                    }
                }).OrderBy(p => p.Index).SelectMany(p => p.Hashes).ToArray();

                foreach (UInt256 headerHash in headerHashes)
                {
                    _headerIndices.Add(headerHash);
                    ++_storedHeaderCount;
                }

                if (_storedHeaderCount == 0)
                {
                    BlockHeader[] headers = _db.Find(options, SliceBuilder.Begin(DataEntryPrefix.DATA_Block), (k, v) =>
                                                     BlockHeader.FromArray(v.ToArray(), sizeof(long))).OrderBy(p => p.Height).ToArray();
                    for (int i = 0; i < headers.Length; ++i)
                        _headerIndices.Add(headers[i].Hash);
                }
                else if (_storedHeaderCount <= _currentHeaderHeight)
                {
                    for (UInt256 hash = _currentHeaderHash; hash != _headerIndices[(int)_storedHeaderCount - 1];)
                    {
                        BlockHeader header = BlockHeader.FromArray(_db.Get(options, SliceBuilder.Begin(DataEntryPrefix.DATA_Block).Add(hash)).ToArray(), sizeof(long));
                        _headerIndices.Insert((int)_storedHeaderCount, hash);
                        hash = header.PrevHash;
                    }
                }
                Slice slice = _db.Get(ReadOptions.Default, SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentTurnTable));
                TurnTableState table = new TurnTableState();
                using (MemoryStream ms = new MemoryStream(slice.ToArray(), false))
                using (BinaryReader br = new BinaryReader(ms))
                {
                    table.Deserialize(br);
                }
                _dpos.TurnTable.SetTable(table.addrs);
                _dpos.TurnTable.SetUpdateHeight(table.turnTableHeight);
            }
            else
            {
                WriteBatch batch = new WriteBatch();
                using (Iterator it = _db.NewIterator(options))
                {
                    for (it.SeekToFirst(); it.Valid(); it.Next())
                        batch.Delete(it.Key());
                }
                _db.Write(WriteOptions.Default, batch);
                _headerIndices.Add(_genesisBlock.Hash);
                _currentBlockHash = _genesisBlock.Hash;
                _currentHeaderHash = _genesisBlock.Hash;
                Persist(_genesisBlock);
                _db.Put(WriteOptions.Default, SliceBuilder.Begin(DataEntryPrefix.SYS_Version), Assembly.GetExecutingAssembly().GetName().Version.ToString());
                UpdateTurnTable();
            }

            _threadPersistence = new Thread(PersistBlocksLoop)
            {
                Name = $"{nameof(LevelDBBlockchain)}.{nameof(PersistBlocksLoop)}"
            };
            _threadPersistence.Start();
        }

        public override void Dispose()
        {
            _disposed = true;
            _newBlockEvent.Set();
            if (_threadPersistence.ThreadState.HasFlag(ThreadState.Unstarted))
                _threadPersistence.Join();
            _newBlockEvent.Dispose();
            if (_db != null)
            {
                _db.Dispose();
                _db = null;
            }
        }

        public override BLOCK_ERROR AddBlock(Block block)
        {
            lock (_blockCache)
            {
                if (!_blockCache.ContainsKey(block.Hash))
                    _blockCache.Add(block.Hash, block);
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

            lock (_headerIndices)
            {
                if (_headerIndices.Count <= block.Height - 1)
                    return BLOCK_ERROR.E_ERROR_HEIGHT;

                if (_headerIndices.Count == block.Height)
                {
                    if (!block.Verify())
                        return BLOCK_ERROR.E_ERROR;
#if DEBUG
                    Logger.Log("Added Height: " + block.Height);
#endif
                    WriteBatch batch = new WriteBatch();
                    OnAddHeader(block.Header, batch);
                    _db.Write(WriteOptions.Default, batch);
                }
                if (block.Height < _headerIndices.Count)
                    _newBlockEvent.Set();
            }
            return BLOCK_ERROR.E_NO_ERROR;
        }

        public override bool AddBlockDirectly(Block block)
        {
            while (_headerIndices.Count > CurrentBlockHeight + 1)
                Thread.Sleep(10);

            if (block.Height != CurrentBlockHeight + 1)
                return false;

            if (block.Height == _headerIndices.Count)
            {
                WriteBatch batch = new WriteBatch();
                OnAddHeader(block.Header, batch);
                _db.Write(WriteOptions.Default, batch);
                lock (PersistLock)
                {
                    Persist(block);
                    if (0 >= _dpos.TurnTable.RemainUpdate(block.Height))
                        UpdateTurnTable();
                    OnPersistCompleted(block);
                }
                return true;
            }
            return false;
        }

        public override BlockHeader GetHeader(UInt256 hash)
        {
            lock (_headerCache)
            {
                if (_headerCache.TryGetValue(hash, out BlockHeader header))
                    return header;
            }
            Slice value;
            if (!_db.TryGet(ReadOptions.Default, SliceBuilder.Begin(DataEntryPrefix.DATA_Block).Add(hash), out value))
                return null;
            return BlockHeader.FromArray(value.ToArray(), sizeof(long));
        }

        public override BlockHeader GetHeader(int height)
        {
            UInt256 hash = GetBlockHash(height);
            if (hash == null)
                return null;
            return GetHeader(hash);
        }

        public override BlockHeader GetNextHeader(UInt256 hash)
        {
            BlockHeader header = GetHeader(hash);
            if (header == null)
                return null;

            return GetHeader(header.Height + 1);
        }

        public override bool ContainsBlock(UInt256 hash)
        {
            return GetHeader(hash)?.Height <= _currentBlockHeight;
        }

        public override Block GetBlock(UInt256 hash)
        {
            Slice value;
            if (!_db.TryGet(ReadOptions.Default, SliceBuilder.Begin(DataEntryPrefix.DATA_Block).Add(hash), out value))
                return null;
            return Block.FromTrimmedData(value.ToArray(), sizeof(long), p => storage.GetTransaction(p));
        }

        public override Block GetBlock(int height)
        {
            UInt256 hash = GetBlockHash(height);
            if (hash == null)
                return null;
            return GetBlock(hash);
        }

        public override Block GetNextBlock(UInt256 hash)
        {
            Block block = GetBlock(hash);
            if (block == null)
                return null;

            return GetBlock(block.Height + 1);
        }

        public override List<DelegateState> GetDelegateStateAll()
        {
            return new List<DelegateState>(_db.Find<DelegateState>(ReadOptions.Default, DataEntryPrefix.ST_Delegate));
        }

        public override List<DelegateState> GetDelegateStateMakers()
        {
            throw new NotImplementedException();
        }

        private UInt256 GetBlockHash(int height)
        {
            lock (_headerIndices)
            {
                if (_headerIndices.Count <= height)
                    return null;
                return _headerIndices[height];
            }
        }

        public override void NormalizeTransactions(ref List<Transaction> txs)
        {
            if (txs.Count == 0)
                return;
            lock (_blockCache)
            {
                foreach (Block block in _blockCache.Values)
                {
                    int counter = txs.Count;
                    while (counter > 0)
                    {
                        counter--;
                        Transaction tx = txs.ElementAt(0);
                        txs.RemoveAt(0);
                        if (block.Transactions.Find((p) => { return p.Hash == tx.Hash; }) == null)
                            txs.Add(tx);
                    }
                }
            }
        }

        private void OnAddHeader(BlockHeader header, WriteBatch batch)
        {
            _headerIndices.Add(header.Hash);
            while (_storedHeaderCount <= header.Height - 2000)
            {
                using (MemoryStream ms = new MemoryStream())
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.WriteSerializableArray(_headerIndices.Skip((int)_storedHeaderCount).Take(2000));
                    bw.Flush();
                    batch.Put(SliceBuilder.Begin(DataEntryPrefix.IX_HeaderHashList).Add(_storedHeaderCount), ms.ToArray());
                }
                _storedHeaderCount += 2000;
            }
            batch.Put(SliceBuilder.Begin(DataEntryPrefix.DATA_Block).Add(header.Hash), SliceBuilder.Begin().Add(0L).Add(header.ToArray()));
            batch.Put(SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentHeader), SliceBuilder.Begin().Add(header.Hash).Add(header.Height));

            _currentHeaderHeight = _headerIndices.Count - 1;
            _currentHeaderHash = header.Hash;
        }

        public override bool VerityBlock(Block block)
        {
            Snapshot _snap = _db.GetSnapshot();
            Storage storage = Storage.NewStorage(_db, new ReadOptions() { Snapshot = _snap });

            List<Transaction> errList = new List<Transaction>();

            foreach (Transaction tx in block.Transactions)
            {
                if (block != GenesisBlock && (!tx.Verify() || !tx.VerifyBlockchain(storage)))
                {
                    errList.Add(tx);
                    lock (PoolLock)
                    {
                        _txPool.Remove(tx.Hash);
                    }
                    continue;
                }

                AccountState from = storage.GetAccountState(tx.From);
                if (Fixed8.Zero < tx.Fee)
                    from.AddBalance(-tx.Fee);

                switch (tx.Data)
                {
                    case TransferTransaction transTx:
                        {
                            Fixed8 totalAmount = transTx.To.Sum(p => p.Value);
                            from.AddBalance(-totalAmount);
                            foreach (var i in transTx.To)
                                storage.GetAccountState(i.Key).AddBalance(i.Value);
                        }
                        break;
                    case VoteTransaction voteTx:
                        {
                            from.LastVoteTxID = tx.Hash;
                            storage.Downvote(from.Votes);
                            storage.Vote(voteTx);
                            from.SetVote(voteTx.Votes);
                        }
                        break;
                    case RegisterDelegateTransaction registerDelegateTx:
                        {
                            storage.AddDelegate(registerDelegateTx.From, registerDelegateTx.Name);
                        }
                        break;
                    case OtherSignTransaction osignTx:
                        {
                            Fixed8 totalAmount = osignTx.To.Sum(p => p.Value);
                            from.AddBalance(-totalAmount);
                            storage.GetBlockTriggers(osignTx.ExpirationBlockHeight).TransactionHashes.Add(osignTx.Owner.Hash);
                            storage.AddOtherSignTxs(osignTx.Owner.Hash, osignTx.Others);
                        }
                        break;
                    case SignTransaction signTx:
                        {
                            OtherSignTransactionState osignState = storage.GetOtherSignTxs(signTx.SignTxHash);
                            if (osignState != null && osignState.Sign(signTx.Owner.Signature) && osignState.RemainSign.Count == 0)
                            {
                                OtherSignTransaction osignTx = storage.GetTransaction(osignState.TxHash).Data as OtherSignTransaction;
                                foreach (var i in osignTx.To)
                                    storage.GetAccountState(i.Key).AddBalance(i.Value);
                                BlockTriggerState state = storage.GetBlockTriggers(signTx.Reference.ExpirationBlockHeight);
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

            BlockTriggerState blockTrigger = storage.TryBlockTriggers(block.Height);
            if (blockTrigger != null)
            {
                foreach (UInt256 txhash in blockTrigger.TransactionHashes)
                {
                    Transaction tx = storage.GetTransaction(txhash);
                    switch (tx.Data)
                    {
                        case OtherSignTransaction osignTx:
                            {
                                storage.GetAccountState(osignTx.From).AddBalance(osignTx.To.Sum(p => p.Value));
                            }
                            break;
                    }
                }
            }
            storage.Dispose();
            _snap.Dispose();
            if (errList.Count == 0)
                return true;
            var list = block.Transactions.Except(errList);
            block.Transactions.Clear();
            block.Transactions.AddRange(list);
            return false;
        }

        private void Persist(Block block)
        {
            WriteBatch batch = new WriteBatch();
            Storage storage = Storage.NewStorage(_db);

            long fee = block.Transactions.Sum(p => p.Fee).Value;
            batch.Put(SliceBuilder.Begin(DataEntryPrefix.DATA_Block).Add(block.Hash), SliceBuilder.Begin().Add(fee).Add(block.Trim()));

            foreach (Transaction tx in block.Transactions)
            {
                batch.Put(SliceBuilder.Begin(DataEntryPrefix.DATA_Transaction).Add(tx.Hash), SliceBuilder.Begin().Add(block.Header.Height).Add(tx.ToArray()));
                if (block != GenesisBlock && !tx.VerifyBlockchain(storage))
                {
                    if (Fixed8.Zero < tx.Fee)
                        storage.GetAccountState(tx.From).AddBalance(-tx.Fee);

                    byte[] eCodeBytes = BitConverter.GetBytes((Int64)tx.Data.TxResult).Take(8).ToArray();
                    batch.Put(SliceBuilder.Begin(DataEntryPrefix.DATA_TxResult).Add(tx.Hash), SliceBuilder.Begin().Add(eCodeBytes));
#if DEBUG
                    Logger.Log("verified == false transaction. " + tx.ToJson());
#endif
                    continue;
                }

                AccountState from = storage.GetAccountState(tx.From);
                if (Fixed8.Zero < tx.Fee)
                    from.AddBalance(-tx.Fee);

                switch (tx.Data)
                {
                    case TransferTransaction transTx:
                        {
                            Fixed8 totalAmount = transTx.To.Sum(p => p.Value);
                            from.AddBalance(-totalAmount);
                            foreach (var i in transTx.To)
                                storage.GetAccountState(i.Key).AddBalance(i.Value);
                        }
                        break;
                    case VoteTransaction voteTx:
                        {
                            from.LastVoteTxID = tx.Hash;
                            storage.Downvote(from.Votes);
                            storage.Vote(voteTx);
                            from.SetVote(voteTx.Votes);
                        }
                        break;
                    case RegisterDelegateTransaction registerDelegateTx:
                        {
                            storage.AddDelegate(registerDelegateTx.From, registerDelegateTx.Name);
                        }
                        break;
                    case OtherSignTransaction osignTx:
                        {
                            Fixed8 totalAmount = osignTx.To.Sum(p => p.Value);
                            from.AddBalance(-totalAmount);
                            storage.GetBlockTriggers(osignTx.ExpirationBlockHeight).TransactionHashes.Add(osignTx.Owner.Hash);
                            storage.AddOtherSignTxs(osignTx.Owner.Hash, osignTx.Others);
                        }
                        break;
                    case SignTransaction signTx:
                        {
                            OtherSignTransactionState osignState = storage.GetOtherSignTxs(signTx.SignTxHash);
                            if (osignState != null && osignState.Sign(signTx.Owner.Signature) && osignState.RemainSign.Count == 0)
                            {
                                OtherSignTransaction osignTx = storage.GetTransaction(osignState.TxHash).Data as OtherSignTransaction;
                                foreach (var i in osignTx.To)
                                    storage.GetAccountState(i.Key).AddBalance(i.Value);
                                BlockTriggerState state = storage.GetBlockTriggers(signTx.Reference.ExpirationBlockHeight);
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

            BlockTriggerState blockTrigger = storage.TryBlockTriggers(block.Height);
            if (blockTrigger != null)
            {
                foreach (UInt256 txhash in blockTrigger.TransactionHashes)
                {
                    Transaction tx = storage.GetTransaction(txhash);
                    switch (tx.Data)
                    {
                        case OtherSignTransaction osignTx:
                            {
                                storage.GetAccountState(osignTx.From).AddBalance(osignTx.To.Sum(p => p.Value));
                            }
                            break;
                    }
                }
            }

            if (0 < block.Height)
            {
                AccountState producer = storage.GetAccountState(WalletAccount.ToAddressHash(block.Header.Signature.Pubkey));
                producer.AddBalance(Config.Instance.BlockReward);
            }
            storage.commit(batch, block.Height);

            batch.Put(SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentBlock), SliceBuilder.Begin().Add(block.Hash).Add(block.Header.Height));
            _db.Write(WriteOptions.Default, batch);
            _currentBlockHeight = block.Header.Height;
            _currentBlockHash = block.Header.Hash;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("persist block : " + block.Height);
#if FALSE
            sb.AppendLine(block.ToJson().ToString());
#endif
            Logger.Log(sb.ToString());
        }

        private void PersistBlocksLoop()
        {
            while (!_disposed)
            {
                _newBlockEvent.WaitOne();

                while (!_disposed)
                {
                    UInt256 hash;
                    lock (_headerIndices)
                    {
                        if (_headerIndices.Count <= _currentBlockHeight + 1)
                            break;
                        hash = _headerIndices[_currentBlockHeight + 1];
                    }
                    Block block;
                    lock (_blockCache)
                    {
                        if (!_blockCache.TryGetValue(hash, out block))
                            break;
                    }

                    // Compare block's previous hash is CurrentBlockHash

                    if (block.Header.PrevHash != CurrentBlockHash)
                        break;

                    lock (PersistLock)
                    {
                        Persist(block);
                        OnPersistCompleted(block);
                    }
                    lock (_blockCache)
                    {
                        _blockCache.Remove(hash);
                    }
                }
            }
        }

        public override void PersistTurnTable(List<UInt160> addrs, int height)
        {
            WriteBatch batch = new WriteBatch();
            TurnTableState state = new TurnTableState();
            state.SetTurnTable(addrs, height);
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                state.Serialize(bw);
                bw.Flush();
                byte[] data = ms.ToArray();
                batch.Put(SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentTurnTable), data);
                batch.Put(SliceBuilder.Begin(DataEntryPrefix.ST_TurnTable).Add(height), data);
            }
            _db.Write(WriteOptions.Default, batch);
        }

        public override TurnTableState GetTurnTable(int turnTableHeight)
        {
            IEnumerable<UInt32> _Heights = _db.Find(ReadOptions.Default, SliceBuilder.Begin(DataEntryPrefix.ST_TurnTable), (k, v) =>
            {
                return k.ToArray().ToUInt32(1);
            }).Where(p => p <= turnTableHeight).ToArray();

            List<UInt32> Heights = new List<uint>();
            foreach (UInt32 n in _Heights)
                Heights.Add(n);
            Heights.Sort((a, b) => { return a > b ? -1 : a < b ? 1 : 0; });
            Slice value = _db.Get(ReadOptions.Default, SliceBuilder.Begin(DataEntryPrefix.ST_TurnTable).Add(Heights.Count > 0 ? Heights.First() : 0));
            TurnTableState state = new TurnTableState();
            using (MemoryStream ms = new MemoryStream(value.ToArray(), false))
            using (BinaryReader br = new BinaryReader(ms))
            {
                state.Deserialize(br);
            }
            return state;
        }

        public override UInt160 GetTurn()
        {
            // create time?
            var time = _dpos.CalcBlockTime(_genesisBlock.Header.Timestamp, Blockchain.Instance.CurrentBlockHeight + 1);
            if (DateTime.UtcNow.ToTimestamp() < time)
                return UInt160.Zero;
            return _dpos.TurnTable.GetTurn(Blockchain.Instance.CurrentBlockHeight + 1);
        }

        public override void UpdateTurnTable()
        {
            int currentHeight = Blockchain.Instance.CurrentBlockHeight;
            UpdateTurnTable(Blockchain.Instance.GetBlock(currentHeight - currentHeight % Config.Instance.RoundBlock));
        }

        void UpdateTurnTable(Block block)
        {
            // calculate turn table
            List<DelegateState> delegates = Blockchain.Instance.GetDelegateStateAll();
            delegates.Sort((x, y) =>
            {
                var valueX = x.Votes.Sum(p => p.Value).Value;
                var valueY = y.Votes.Sum(p => p.Value).Value;
                if (valueX == valueY)
                {
                    if (x.AddressHash < y.AddressHash)
                        return -1;
                    else
                        return 1;
                }
                else if (valueX < valueY)
                    return -1;
                return 1;
            });

            int delegateRange = Config.Instance.MaxDelegate < delegates.Count ? Config.Instance.MaxDelegate : delegates.Count;
            List<UInt160> addrs = new List<UInt160>();
            for (int i = 0; i < delegateRange; ++i)
                addrs.Add(delegates[i].AddressHash);

            _dpos.TurnTable.SetTable(addrs);
            _dpos.TurnTable.SetUpdateHeight(block.Height);

            PersistTurnTable(addrs, block.Height);
        }
    }
}
