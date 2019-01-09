using Mineral.Core;
using Mineral.Core.Transactions;
using Mineral.Database.LevelDB;
using Mineral.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("Mineral.UnitTests")]
namespace Mineral.Database.BlockChain
{
    internal class LevelDBBlockChain : BaseLevelDB, IDisposable
    {
        private Storage _storage = null;

        public LevelDBBlockChain(string path)
            : base(path)
        {
            NewStorage();
        }

        #region Properties
        public Storage Storage { get { return _storage; } }
        public Storage SnapShot
        {
            get
            {
                return Storage.NewStorage(_db, new ReadOptions() { Snapshot = _db.GetSnapshot() });
            }
        }
        #endregion


        #region Storage
        public void NewStorage()
        {
            _storage = Storage.NewStorage(_db);
        }
        #endregion


        #region Batch - Put
        public void PutCurrentHeader(WriteBatch batch, BlockHeader header)
        {
            batch.Put(SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentHeader), SliceBuilder.Begin().Add(header.Hash).Add(header.Height));
        }

        public void PutCurrentBlock(WriteBatch batch, Block block)
        {
            batch.Put(SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentBlock), SliceBuilder.Begin().Add(block.Hash).Add(block.Header.Height));
        }

        public void PutHeaderHashList(WriteBatch batch, int count, byte[] hash)
        {
            batch.Put(SliceBuilder.Begin(DataEntryPrefix.IX_HeaderHashList).Add(count), hash);
        }

        public void PutBlock(WriteBatch batch, BlockHeader header, long fee)
        {
            batch.Put(SliceBuilder.Begin(DataEntryPrefix.DATA_Block).Add(header.Hash), SliceBuilder.Begin().Add(fee).Add(header.ToArray()));
        }

        public void PutBlock(WriteBatch batch, Block block, long fee)
        {
            batch.Put(SliceBuilder.Begin(DataEntryPrefix.DATA_Block).Add(block.Hash), SliceBuilder.Begin().Add(fee).Add(block.Trim()));
        }

        public void PutTransaction(WriteBatch batch, Block block, Transaction tx)
        {
            batch.Put(SliceBuilder.Begin(DataEntryPrefix.DATA_Transaction).Add(tx.Hash), SliceBuilder.Begin().Add(block.Header.Height).Add(tx.ToArray()));
        }

        public void PutTransactionResult(WriteBatch batch, Transaction tx)
        {
            byte[] txRes = BitConverter.GetBytes((Int64)tx.Data.TxResult).Take(8).ToArray();
            batch.Put(SliceBuilder.Begin(DataEntryPrefix.DATA_TxResult).Add(tx.Hash), SliceBuilder.Begin().Add(txRes));
        }

        public void PutCurrentTurnTalbe(WriteBatch batch, byte[] data)
        {
            batch.Put(SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentTurnTable), data);
        }

        public void PutTurnTable(WriteBatch batch, TurnTableState state)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                state.Serialize(bw);
                bw.Flush();
                byte[] data = ms.ToArray();
                batch.Put(SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentTurnTable), data);
                batch.Put(SliceBuilder.Begin(DataEntryPrefix.ST_TurnTable).Add(state.turnTableHeight), data);
            }
        }

        public void BatchWrite(WriteOptions option, WriteBatch batch)
        {
            _db.Write(option, batch);
        }
        #endregion


        #region TryGet
        public bool TryGetCurrentHeader(out UInt256 headerHash, out int headerHeight)
        {
            Slice value;
            bool result = _db.TryGet(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentHeader), out value);
            if (result)
            {
                headerHash = new UInt256(value.ToArray().Take(32).ToArray());
                headerHeight = value.ToArray().ToInt32(32);
            }
            else
            {
                headerHash = new UInt256();
                headerHeight = 0;
            }

            return result;
        }

        public bool TryGetCurrentBlock(out UInt256 blockHash, out int blockHeight)
        {
            Slice value;
            bool result = _db.TryGet(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentBlock), out value);
            if (result)
            {
                blockHash = new UInt256(value.ToArray().Take(32).ToArray());
                blockHeight = value.ToArray().ToInt32(32);
            }
            else
            {
                blockHash = new UInt256();
                blockHeight = 0;
            }

            return result;
        }

        public bool TryGetBlockHeader(UInt256 headerHash, out BlockHeader blockHeader)
        {
            Slice value;
            bool result = _db.TryGet(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.DATA_Block).Add(headerHash), out value);
            if (result)
                blockHeader = BlockHeader.FromArray(value.ToArray(), sizeof(long));
            else
                blockHeader = null;

            return result;
        }

        public bool TryGetBlock(UInt256 blockHash, out Block block)
        {
            Slice value;
            bool result = _db.TryGet(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.DATA_Block).Add(blockHash), out value);
            if (result)
                block = Block.FromTrimmedData(value.ToArray(), sizeof(long), p => Storage.GetTransaction(p));
            else
                block = null;

            return result;
        }

        public bool TryGetTransaction(UInt256 txHash, out Transaction tx)
        {
            Slice value;
            bool result = _db.TryGet(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.DATA_Transaction).Add(txHash), out value);
            if (result)
                tx = Transaction.DeserializeFrom(value.ToArray().Skip(sizeof(int)).ToArray());
            else
                tx = null;

            return result;
        }

        public bool TryGetTransactionResult(UInt256 txHash, out MINERAL_ERROR_CODES code)
        {
            Slice value;
            bool result = _db.TryGet(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.DATA_TxResult).Add(txHash), out value);
            if (result)
                code = (MINERAL_ERROR_CODES)value.ToInt64();
            else
                code = MINERAL_ERROR_CODES.NO_ERROR;

            return result;
        }

        public bool TryGetCurrentTurnTable(out TurnTableState state)
        {
            state = new TurnTableState();
            Slice value;
            bool result = _db.TryGet(ReadOptions.Default, SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentTurnTable), out value);
            if (result)
            {
                using (MemoryStream ms = new MemoryStream(value.ToArray(), false))
                using (BinaryReader br = new BinaryReader(ms))
                {
                    state.Deserialize(br);
                }
            }
            else
            {
                state = null;
            }

            return result;
        }

        public bool TryGetTurnTable(int height, out TurnTableState state)
        {
            state = new TurnTableState();
            Slice value;
            bool result = _db.TryGet(ReadOptions.Default, SliceBuilder.Begin(DataEntryPrefix.ST_TurnTable).Add(height), out value);
            if (result)
            {
                using (MemoryStream ms = new MemoryStream(value.ToArray(), false))
                using (BinaryReader br = new BinaryReader(ms))
                {
                    state.Deserialize(br);
                }
            }
            else
            {
                state = null;
            }

            return result;
        }
        #endregion


        #region Get
        public UInt256 GetCurrentHeaderHash()
        {
            Slice value = _db.Get(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentHeader));
            return new UInt256(value.ToArray().Take(32).ToArray());
        }

        public int GetCurrentHeaderHeight()
        {
            Slice value = _db.Get(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentHeader));
            return value.ToArray().ToInt32(32);
        }

        public UInt256 GetCurrentBlockHash()
        {
            Slice value = _db.Get(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentBlock));
            return new UInt256(value.ToArray().Take(32).ToArray());
        }

        public int GetCurrentBlockHeight()
        {
            Slice value = _db.Get(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentBlock));
            return value.ToArray().ToInt32(32);
        }

        public BlockHeader GetBlockHeader(UInt256 blockHash)
        {
            return BlockHeader.FromArray(_db.Get(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.DATA_Block).Add(blockHash)).ToArray(), sizeof(long));
        }

        public Block GetBlock(UInt256 blockHash)
        {
            return Block.FromTrimmedData(_db.Get(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.DATA_Block).Add(blockHash)).ToArray(), sizeof(long), p => Storage.GetTransaction(p));
        }

        public IEnumerable<UInt256> GetHeaderHashList()
        {
            IEnumerable<UInt256> headerHashes = _db.Find(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.IX_HeaderHashList), (k, v) =>
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

            return headerHashes;
        }

        public IEnumerable<BlockHeader> GetBlockHeaderList()
        {
            return _db.Find(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.DATA_Block), (k, v) =>
                                    BlockHeader.FromArray(v.ToArray(), sizeof(long))).OrderBy(p => p.Height).ToArray();
        }

        public IEnumerable<UInt256> GetBlockHeaderHashList()
        {
            return GetBlockHeaderList().Select(x => x.Hash);
        }

        public TurnTableState GetCurrentTurnTable()
        {
            TurnTableState table = new TurnTableState();
            Slice value = _db.Get(ReadOptions.Default, SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentTurnTable)).ToArray();

            using (MemoryStream ms = new MemoryStream(value.ToArray(), false))
            using (BinaryReader br = new BinaryReader(ms))
            {
                table.Deserialize(br);
            }
            return table;
        }

        public TurnTableState GetTurnTable(int height)
        {
            TurnTableState table = new TurnTableState();

            Slice value = _db.Get(ReadOptions.Default, SliceBuilder.Begin(DataEntryPrefix.ST_TurnTable).Add(height));
            using (MemoryStream ms = new MemoryStream(value.ToArray(), false))
            using (BinaryReader br = new BinaryReader(ms))
            {
                table.Deserialize(br);
            }

            return table;
        }

        public IEnumerable<int> GetTurnTableHeightList(int height)
        {
            return _db.Find(ReadOptions.Default, SliceBuilder.Begin(DataEntryPrefix.ST_TurnTable), (k, v) =>
            {
                return k.ToArray().ToInt32(1);
            }).Where(p => p <= height).ToArray();
        }

        public IEnumerable<DelegateState> GetDelegateStateAll()
        {
            return _db.Find<DelegateState>(ReadOptions.Default, DataEntryPrefix.ST_Delegate);
        }
        #endregion


        #region Put
        public void PutCurrentHeader(BlockHeader header)
        {
            _db.Put(WriteOptions.Default, SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentHeader), SliceBuilder.Begin().Add(header.Hash).Add(header.Height));
        }

        public void PutCurrentBlock(Block block)
        {
            _db.Put(WriteOptions.Default, SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentBlock), SliceBuilder.Begin().Add(block.Hash).Add(block.Height));
        }

        public void PutBlock(Block block)
        {
            _db.Put(WriteOptions.Default, SliceBuilder.Begin(DataEntryPrefix.DATA_Block).Add(block.Hash), SliceBuilder.Begin().Add(0L).Add(block.ToArray()));
        }

        public void PutHeaderHashList(int prefix_count, IEnumerable<UInt256> headerHashList)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.WriteSerializableArray(headerHashList);
                bw.Flush();
                _db.Put(WriteOptions.Default, SliceBuilder.Begin(DataEntryPrefix.IX_HeaderHashList).Add(prefix_count), ms.ToArray());
            }
        }

        public void PutTurnTable(TurnTableState state)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                state.Serialize(bw);
                bw.Flush();
                byte[] data = ms.ToArray();
                _db.Put(WriteOptions.Default, SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentTurnTable), data);
                _db.Put(WriteOptions.Default, SliceBuilder.Begin(DataEntryPrefix.ST_TurnTable).Add(state.turnTableHeight), data);
            }
        }
        #endregion
    }
}