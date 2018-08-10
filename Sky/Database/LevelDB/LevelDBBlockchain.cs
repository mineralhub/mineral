using System;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using Sky;
using Sky.Core;
using System.Text;

namespace Sky.Database.LevelDB
{
    public class LevelDBBlockchain : Blockchain
    {
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

        public override Block GenesisBlock => _genesisBlock;
        public override int CurrentBlockHeight => _currentBlockHeight;
        public override UInt256 CurrentBlockHash => _currentBlockHash;
        public override int CurrentHeaderHeight => _currentHeaderHeight;
        public override UInt256 CurrentHeaderHash => _currentHeaderHash;

        public LevelDBBlockchain(string path, Block genesisBlock)
        {
            _genesisBlock = genesisBlock;

            Version version;
            Slice value;
            ReadOptions options = new ReadOptions { FillCache = false };
            _db = DB.Open(path, new Options { CreateIfMissing = true });
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
                _headerIndices.Add(genesisBlock.Hash);
                _currentBlockHash = genesisBlock.Hash;
                _currentHeaderHash = genesisBlock.Hash;
                Persist(genesisBlock);
                _db.Put(WriteOptions.Default, SliceBuilder.Begin(DataEntryPrefix.SYS_Version), Assembly.GetExecutingAssembly().GetName().Version.ToString());
            }

            _threadPersistence = new Thread(PersistBlocksLoop);
            _threadPersistence.Name = $"{nameof(LevelDBBlockchain)}.{nameof(PersistBlocksLoop)}";
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

        public override bool AddBlock(Block block)
        {
            lock (_blockCache)
            {
                if (!_blockCache.ContainsKey(block.Hash))
                    _blockCache.Add(block.Hash, block);
            }

            lock (_headerIndices)
            {
                if (_headerIndices.Count <= block.Height - 1)
                    return false;

                if (_headerIndices.Count == block.Height)
                {
                    if (!block.Verify())
                        return false;
                    WriteBatch batch = new WriteBatch();
                    OnAddHeader(block.Header, batch);
                    _db.Write(WriteOptions.Default, batch);
                }
                if (block.Height < _headerIndices.Count)
                    _newBlockEvent.Set();
            }
            return true;
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
            Block block = new Block(value.ToArray(), sizeof(long));
            if (block.Transactions.Count == 0)
                return null;
            return block;
        }

        public override Block GetBlock(int height)
        {
            UInt256 hash = GetBlockHash(height);
            if (hash == null)
                return null;
            return GetBlock(hash);
        }

        public override Transaction GetTransaction(UInt256 hash, out int height)
        {
            return GetTransaction(ReadOptions.Default, hash, out height);
        }

        private Transaction GetTransaction(ReadOptions options, UInt256 hash, out int height)
        {
            Slice value;
            if (_db.TryGet(options, SliceBuilder.Begin(DataEntryPrefix.DATA_Transaction).Add(hash), out value))
            {
                byte[] data = value.ToArray();
                height = data.ToInt32(0);
                return Transaction.DeserializeFrom(data, sizeof(uint));
            }
            else
            {
                height = -1;
                return null;
            }
        }

        public override bool IsDoubleSpend(Transaction tx)
        {
            if (tx.Inputs.Count == 0)
                return false;

            ReadOptions options = new ReadOptions();
            using (options.Snapshot = _db.GetSnapshot())
            {
                foreach (var group in tx.Inputs.GroupBy(p => p.PrevHash))
                {
                    UnspentCoinState state = _db.TryGet<UnspentCoinState>(options, DataEntryPrefix.ST_Coin, group.Key);
                    if (state == null)
                        return true;
                    if (group.Any(p => p.PrevIndex >= state.Items.Count || state.Items[p.PrevIndex].HasFlag(CoinState.Spent)))
                        return true;
                }
            }
            return false;
        }

        public override AccountState GetAccountState(UInt160 addressHash)
        {
            return _db.TryGet<AccountState>(ReadOptions.Default, DataEntryPrefix.ST_Account, addressHash);
        }

        public override List<DelegatorState> GetDelegateStateAll()
        {
            return new List<DelegatorState>(_db.Find<DelegatorState>(ReadOptions.Default, DataEntryPrefix.ST_Delegator));
        }

        public override List<DelegatorState> GetDelegateStateMakers()
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

        private void Persist(Block block)
        {
            WriteBatch batch = new WriteBatch();
            DbCache<UInt160, AccountState> accounts = new DbCache<UInt160, AccountState>(_db, DataEntryPrefix.ST_Account);
            DbCache<UInt160, DelegatorState> delegators = new DbCache<UInt160, DelegatorState>(_db, DataEntryPrefix.ST_Delegator);
            DbCache<UInt256, UnspentCoinState> unspentCoins = new DbCache<UInt256, UnspentCoinState>(_db, DataEntryPrefix.ST_Coin);
            DbCache<UInt256, SpentCoinState> spentCoins = new DbCache<UInt256, SpentCoinState>(_db, DataEntryPrefix.ST_SpentCoin);

            long fee = block.Transactions.Sum(p => p.Fee).Value;
            batch.Put(SliceBuilder.Begin(DataEntryPrefix.DATA_Block).Add(block.Hash), SliceBuilder.Begin().Add(fee).Add(block.ToArray()));
            foreach (Transaction tx in block.Transactions)
            {
                batch.Put(SliceBuilder.Begin(DataEntryPrefix.DATA_Transaction).Add(tx.Hash), SliceBuilder.Begin().Add(block.Header.Height).Add(tx.ToArray()));
                foreach (TransactionOutput output in tx.Outputs)
                {
                    AccountState account = accounts.GetAndChange(output.AddressHash, () => new AccountState(output.AddressHash));
                    account.AddBalance(output.Value);
                }

                int txHeight;
                foreach (TransactionInput input in tx.Inputs)
                {
                    Transaction prevtx = GetTransaction(input.PrevHash, out txHeight);
                    TransactionOutput prevOutput = prevtx.Outputs[input.PrevIndex];
                    AccountState account = accounts.GetAndChange(prevOutput.AddressHash, () => new AccountState(prevOutput.AddressHash));
                    account.AddBalance(-prevOutput.Value);
                }

                switch (tx.Type)
                {
                    case eTransactionType.RewardTransaction:
                        break;
                    case eTransactionType.DataTransaction:
                        {
                            //DataTransaction data = tx as DataTransaction;
                            //data.Data
                        }
                        break;
                    case eTransactionType.VoteTransaction:
                        {
                            VoteTransaction voteTx = tx.Data as VoteTransaction;
                            AccountState account = accounts.GetAndChange(voteTx.Sender, () => new AccountState(voteTx.Sender));
                            foreach (var v in voteTx.Votes)
                            {
                                DelegatorState delegator = delegators.TryGet(v.Key);
                                if (delegator.Votes.ContainsKey(voteTx.Sender))
                                    delegator.Votes[voteTx.Sender] = v.Value;
                                else
                                    delegator.Votes.Add(voteTx.Sender, v.Value);
                            }
                            account.SetVote(voteTx.Votes);
                        }
                        break;
                    case eTransactionType.RegisterDelegateTransaction:
                        {
                            RegisterDelegateTransaction delegateTx = tx.Data as RegisterDelegateTransaction;
                            delegators.Add(delegateTx.Sender, new DelegatorState(delegateTx.Sender, delegateTx.NameBytes));
                        }
                        break;
                }
            }

            accounts.DeleteWhere((k, v) => !v.IsFrozen && v.Balance <= Fixed8.Zero && v.Votes == null);
            accounts.Commit(batch);
            delegators.DeleteWhere((k, v) => v.AddressHash == null);
            delegators.Commit(batch);
            batch.Put(SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentBlock), SliceBuilder.Begin().Add(block.Hash).Add(block.Header.Height));
            _db.Write(WriteOptions.Default, batch);
            _currentBlockHeight = block.Header.Height;
            _currentBlockHash = block.Header.Hash;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("persist block : " + block.Height);
            sb.AppendLine(block.ToJson().ToString());
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
                    Persist(block);
                    OnPersistCompleted(block);
                    lock (_blockCache)
                    {
                        _blockCache.Remove(hash);
                    }
                }
            }
        }
    }
}
