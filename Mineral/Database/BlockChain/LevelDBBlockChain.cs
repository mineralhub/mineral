using Mineral.Core;
using Mineral.Core.State;
using Mineral.Core.Transactions;
using Mineral.Database.LevelDB;
using Mineral.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

        public void PutBlock(WriteBatch batch, Block block, long fee)
        {
            BlockState blockState = new BlockState(block, Fixed8.FromLongValue(fee));
            batch.Put(SliceBuilder.Begin(DataEntryPrefix.DATA_Block).Add(block.Hash), SliceBuilder.Begin().Add(blockState.ToArray()));
        }

        public void PutTransaction(WriteBatch batch, Block block, Transaction tx)
        {
            TransactionState txState = new TransactionState(block.Header.Height, tx);
            batch.Put(SliceBuilder.Begin(DataEntryPrefix.DATA_Transaction).Add(tx.Hash), SliceBuilder.Begin().Add(txState.ToArray()));
        }

        public void PutTransactionResult(WriteBatch batch, Transaction tx)
        {
            TransactionResultState txResultState = new TransactionResultState(tx.Data.TxResult);
            batch.Put(SliceBuilder.Begin(DataEntryPrefix.DATA_TransactionResult).Add(tx.Hash), SliceBuilder.Begin().Add(txResultState.ToArray()));
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
        public bool TryGetCurrentHeader(out UInt256 headerHash, out uint headerHeight)
        {
            Slice value;
            bool result = _db.TryGet(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentHeader), out value);
            if (result)
            {
                headerHash = new UInt256(value.ToArray().Take(32).ToArray());
                headerHeight = value.ToArray().ToUInt32(32);
            }
            else
            {
                headerHash = new UInt256();
                headerHeight = 0;
            }

            return result;
        }

        public bool TryGetCurrentBlock(out UInt256 blockHash, out uint blockHeight)
        {
            Slice value;
            bool result = _db.TryGet(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentBlock), out value);
            if (result)
            {
                blockHash = new UInt256(value.ToArray().Take(32).ToArray());
                blockHeight = value.ToArray().ToUInt32(32);
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
            {
                BlockState blockState = BlockState.DeserializeFrom(value.ToArray());
                blockHeader = blockState.Header;
            }
            else
            {
                blockHeader = null;
            }

            return result;
        }

        public bool TryGetBlock(UInt256 blockHash, out Block block)
        {
            Slice value;
            bool result = _db.TryGet(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.DATA_Block).Add(blockHash), out value);
            if (result)
            {
                BlockState blockState = BlockState.DeserializeFrom(value.ToArray());
                block = blockState.GetBlock(p => Storage.Transaction.Get(p));
            }
            else
            {
                block = null;
            }
            return result;
        }

        public bool TryGetTransaction(UInt256 txHash, out Transaction tx)
        {
            Slice value;
            bool result = _db.TryGet(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.DATA_Transaction).Add(txHash), out value);
            if (result)
            {
                TransactionState txState = TransactionState.DeserializeFrom(value.ToArray());
                tx = txState.Transaction;
            }
            else
            {
                tx = null;
            }
            return result;
        }

        public bool TryGetTransactionResult(UInt256 txHash, out MINERAL_ERROR_CODES code)
        {
            Slice value;
            bool result = _db.TryGet(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.DATA_TransactionResult).Add(txHash), out value);
            if (result)
            {
                TransactionResultState txResultState = TransactionResultState.DeserializeFrom(value.ToArray());
                code = txResultState.TxResult;
            }
            else
            {
                code = MINERAL_ERROR_CODES.NO_ERROR;
            }

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

        public bool TryGetTurnTable(uint height, out TurnTableState state)
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
            UInt256 hash = UInt256.Zero;
            try
            {
                Slice value = _db.Get(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentHeader));
                hash = new UInt256(value.ToArray().Take(32).ToArray());
            }
            catch (Exception e)
            {
                Logger.Warning("[Warning] " + MethodBase.GetCurrentMethod().Name + " : " + e.Message);
                hash = UInt256.Zero;
            }
            return hash;
        }

        public uint GetCurrentHeaderHeight()
        {
            uint height = uint.MaxValue;
            try
            {
                Slice value = _db.Get(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentHeader));
                height = value.ToArray().ToUInt32(32);
            }
            catch (Exception e)
            {
                Logger.Warning("[Warning] " + MethodBase.GetCurrentMethod().Name + " : " + e.Message);
                height = uint.MaxValue;
            }
            return height;
        }

        public UInt256 GetCurrentBlockHash()
        {
            UInt256 hash = UInt256.Zero;
            try
            {
                Slice value = _db.Get(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentBlock));
                hash = new UInt256(value.ToArray().Take(32).ToArray());
            }
            catch (Exception e)
            {
                Logger.Warning("[Warning] " + MethodBase.GetCurrentMethod().Name + " : " + e.Message);
                hash = UInt256.Zero;
            }
            return hash;
        }

        public uint GetCurrentBlockHeight()
        {
            uint height = uint.MaxValue;
            try
            {
                Slice value = _db.Get(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentBlock));
                height = value.ToArray().ToUInt32(32);
            }
            catch (Exception e)
            {
                Logger.Warning("[Warning] " + MethodBase.GetCurrentMethod().Name + " : " + e.Message);
                height = uint.MaxValue;
            }
            return height;
        }

        public BlockHeader GetBlockHeader(UInt256 blockHash)
        {
            BlockHeader header = null;
            try
            {
                Slice value = _db.Get(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.DATA_Block).Add(blockHash));
                header = BlockState.DeserializeFrom(value.ToArray()).Header;
            }
            catch (Exception e)
            {
                Logger.Warning("[Warning] " + MethodBase.GetCurrentMethod().Name + " : " + e.Message);
                header = null;
            }
            return header;
        }

        public Block GetBlock(UInt256 blockHash)
        {
            Block block = null;
            try
            {
                Slice value = _db.Get(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.DATA_Block).Add(blockHash));
                block = BlockState.DeserializeFrom(value.ToArray()).GetBlock(p => Storage.Transaction.Get(p));
            }
            catch (Exception e)
            {
                Logger.Warning("[Warning] " + MethodBase.GetCurrentMethod().Name + " : " + e.Message);
                block = null;
            }
            return block;
        }

        public IEnumerable<UInt256> GetHeaderHashList()
        {
            IEnumerable<UInt256> headerHashes = null;
            try
            {
                headerHashes = _db.Find(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.IX_HeaderHashList), (k, v) =>
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
            }
            catch (Exception e)
            {
                Logger.Warning("[Warning] " + MethodBase.GetCurrentMethod().Name + " : " + e.Message);
                headerHashes = null;
            }
            return headerHashes;
        }

        public IEnumerable<BlockHeader> GetBlockHeaders(uint start, uint end)
        {
            IEnumerable<BlockHeader> headers = null;
            try
            {
                headers = _db.Find(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.DATA_Block), (k, v) =>
                {
                    BlockState blockState = BlockState.DeserializeFrom(v.ToArray());
                    return blockState.Header;
                })
                    .Where(x => x.Height >= start && x.Height <= end)
                    .OrderBy(p => p.Height).ToArray();
            }
            catch (Exception e)
            {
                Logger.Warning("[Warning] " + MethodBase.GetCurrentMethod().Name + " : " + e.Message);
                headers = null;
            }
            return headers;
        }

        public IEnumerable<BlockHeader> GetBlockHeaderList()
        {
            IEnumerable<BlockHeader> headers = null;
            try
            {
                headers = _db.Find(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.DATA_Block), (k, v) =>
                {
                    BlockState blockState = BlockState.DeserializeFrom(v.ToArray());
                    return blockState.Header;
                }).OrderBy(p => p.Height).ToArray();
            }
            catch (Exception e)
            {
                Logger.Warning("[Warning] " + MethodBase.GetCurrentMethod().Name + " : " + e.Message);
                headers = null;
            }
            return headers;
        }

        public IEnumerable<UInt256> GetBlockHeaderHashList()
        {
            IEnumerable<UInt256> headerHashs = null;
            try
            {
                headerHashs = GetBlockHeaderList().Select(x => x.Hash);
            }
            catch (Exception e)
            {
                Logger.Warning("[Warning] " + MethodBase.GetCurrentMethod().Name + " : " + e.Message);
                headerHashs = null;
            }
            return headerHashs;
        }

        public TurnTableState GetCurrentTurnTable()
        {
            TurnTableState table = null;
            try
            {
                table = new TurnTableState();
                Slice value = _db.Get(ReadOptions.Default, SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentTurnTable)).ToArray();

                using (MemoryStream ms = new MemoryStream(value.ToArray(), false))
                using (BinaryReader br = new BinaryReader(ms))
                {
                    table.Deserialize(br);
                }
            }
            catch (Exception e)
            {
                Logger.Warning("[Warning] " + MethodBase.GetCurrentMethod().Name + " : " + e.Message);
                table = null;
            }
            return table;
        }

        public TurnTableState GetTurnTable(uint height)
        {
            TurnTableState table = null;
            try
            {
                table = new TurnTableState();
                Slice value = _db.Get(ReadOptions.Default, SliceBuilder.Begin(DataEntryPrefix.ST_TurnTable).Add(height));

                using (MemoryStream ms = new MemoryStream(value.ToArray(), false))
                using (BinaryReader br = new BinaryReader(ms))
                {
                    table.Deserialize(br);
                }
            }
            catch (Exception e)
            {
                Logger.Warning("[Warning] " + MethodBase.GetCurrentMethod().Name + " : " + e.Message);
                table = null;
            }
            return table;
        }

        public IEnumerable<uint> GetTurnTableHeightList(uint height)
        {
            IEnumerable<uint> heightList = null;
            try
            {
                heightList = _db.Find(ReadOptions.Default, SliceBuilder.Begin(DataEntryPrefix.ST_TurnTable), (k, v) =>
                {
                    return k.ToArray().ToUInt32(1);
                }).Where(p => p <= height).ToArray();
            }
            catch (Exception e)
            {
                Logger.Warning("[Warning] " + MethodBase.GetCurrentMethod().Name + " : " + e.Message);
                heightList = null;
            }
            return heightList;
        }

        public IEnumerable<DelegateState> GetDelegateStateAll()
        {
            IEnumerable<DelegateState> delegateStates = null;
            try
            {
                delegateStates = _db.Find<DelegateState>(ReadOptions.Default, DataEntryPrefix.ST_Delegate);
            }
            catch (Exception e)
            {
                Logger.Warning("[Warning] " + MethodBase.GetCurrentMethod().Name + " : " + e.Message);
                delegateStates = null;
            }
            return delegateStates;
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
            BlockState blockState = new BlockState(block);
            _db.Put(WriteOptions.Default, SliceBuilder.Begin(DataEntryPrefix.DATA_Block).Add(block.Hash), SliceBuilder.Begin().Add(blockState.ToArray()));
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