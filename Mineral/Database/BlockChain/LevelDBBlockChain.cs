using Mineral.Core;
using Mineral.Database.LevelDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mineral.Database.BlockChain
{
    internal class LevelDBBlockChain
    {
        private DB db = null;
        private Storage storage = null;

        public LevelDBBlockChain(string path)
        {
            this.db = DB.Open(path, new Options { CreateIfMissing = true });
            NewStorage();
        }

        public void Dispose()
        {
            this.db.Dispose();
        }

        #region Properties
        public Storage Storage { get { return this.storage; } }
        public Storage SnapShot
        {
            get
            {
                return Storage.NewStorage(this.db, new ReadOptions() { Snapshot = this.db.GetSnapshot() });
            }
        }
        #endregion


        #region Storage
        public void NewStorage()
        {
            this.storage = Storage.NewStorage(this.db);
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
            this.db.Write(option, batch);
        }
        #endregion


        #region TryGet
        public bool TryGetVersion(out Version version)
        {
            Slice value;
            bool result = this.db.TryGet(ReadOptions.Default, SliceBuilder.Begin(DataEntryPrefix.SYS_Version), out value);
            if (result)
                Version.TryParse(value.ToString(), out version);
            else
                version = null;
            return result;
        }

        public bool TryGetCurrentHeader(out UInt256 headerHash, out int headerHeight)
        {
            Slice value;
            bool result = this.db.TryGet(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentHeader), out value);
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
            bool result = this.db.TryGet(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentBlock), out value);
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
            bool result = this.db.TryGet(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.DATA_Block).Add(headerHash), out value);
            if (result)
                blockHeader = BlockHeader.FromArray(value.ToArray(), sizeof(long));
            else
                blockHeader = null;

            return result;
        }

        public bool TryGetBlock(UInt256 blockHash, out Block block)
        {
            Slice value;
            bool result = this.db.TryGet(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.DATA_Block).Add(blockHash), out value);
            if (result)
                block = Block.FromTrimmedData(value.ToArray(), sizeof(long), p => Storage.GetTransaction(p));
            else
                block = null;

            return result;
        }

        public bool TryGetTransaction(UInt256 txHash, out Transaction tx)
        {
            Slice value;
            bool result = this.db.TryGet(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.DATA_Transaction).Add(txHash), out value);
            if (result)
                tx = Transaction.DeserializeFrom(value.ToArray().Skip(sizeof(int)).ToArray());
            else
                tx = null;

            return result;
        }

        public bool TryGetTransactionResult(UInt256 txHash, out ErrorCodes code)
        {
            Slice value;
            bool result = this.db.TryGet(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.DATA_TxResult).Add(txHash), out value);
            if (result)
                code = (ErrorCodes)value.ToInt64();
            else
                code = ErrorCodes.E_NO_ERROR;

            return result;
        }

        public bool TryGetCurrentTurnTable(out TurnTableState state)
        {
            state = new TurnTableState();
            Slice value;
            bool result = db.TryGet(ReadOptions.Default, SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentTurnTable), out value);
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
            bool result = this.db.TryGet(ReadOptions.Default, SliceBuilder.Begin(DataEntryPrefix.ST_TurnTable).Add(height), out value);
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
        public Version GetVersion()
        {
            Slice value;
            Version version;
            value = this.db.Get(ReadOptions.Default, SliceBuilder.Begin(DataEntryPrefix.SYS_Version));
            Version.TryParse(value.ToString(), out version);
            return version;
        }

        public UInt256 GetCurrentHeaderHash()
        {
            Slice value = this.db.Get(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentHeader));
            return new UInt256(value.ToArray().Take(32).ToArray());
        }

        public int GetCurrentHeaderHeight()
        {
            Slice value = this.db.Get(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentHeader));
            return value.ToArray().ToInt32(32);
        }

        public UInt256 GetCurrentBlockHash()
        {
            Slice value = this.db.Get(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentBlock));
            return new UInt256(value.ToArray().Take(32).ToArray());
        }

        public int GetCurrentBlockHeight()
        {
            Slice value = this.db.Get(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentBlock));
            return value.ToArray().ToInt32(32);
        }

        public BlockHeader GetBlockHeader(UInt256 blockHash)
        {
            return BlockHeader.FromArray(this.db.Get(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.DATA_Block).Add(blockHash)).ToArray(), sizeof(long));
        }

        public Block GetBlock(UInt256 blockHash)
        {
            return Block.FromTrimmedData(this.db.Get(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.DATA_Block).Add(blockHash)).ToArray(), sizeof(long), p => Storage.GetTransaction(p));
        }

        public IEnumerable<UInt256> GetHeaderHashList()
        {
            IEnumerable<UInt256> headerHashes = this.db.Find(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.IX_HeaderHashList), (k, v) =>
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
            return this.db.Find(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.DATA_Block), (k, v) =>
                                    BlockHeader.FromArray(v.ToArray(), sizeof(long))).OrderBy(p => p.Height).ToArray();
        }

        public IEnumerable<UInt256> GetBlockHeaderHashList()
        {
            return GetBlockHeaderList().Select(x => x.Hash);
        }

        public TurnTableState GetCurrentTurnTable()
        {
            TurnTableState table = new TurnTableState();
            Slice value = db.Get(ReadOptions.Default, SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentTurnTable)).ToArray();

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

            Slice value = this.db.Get(ReadOptions.Default, SliceBuilder.Begin(DataEntryPrefix.ST_TurnTable).Add(height));
            using (MemoryStream ms = new MemoryStream(value.ToArray(), false))
            using (BinaryReader br = new BinaryReader(ms))
            {
                table.Deserialize(br);
            }

            return table;
        }

        public IEnumerable<int> GetTurnTableHeightList(int height)
        {
            return this.db.Find(ReadOptions.Default, SliceBuilder.Begin(DataEntryPrefix.ST_TurnTable), (k, v) =>
            {
                return k.ToArray().ToInt32(1);
            }).Where(p => p <= height).ToArray();
        }

        public IEnumerable<DelegateState> GetDelegateStateAll()
        {
            return this.db.Find<DelegateState>(ReadOptions.Default, DataEntryPrefix.ST_Delegate);
        }
        #endregion


        #region Put
        public void PutVersion(Version version)
        {
            this.db.Put(WriteOptions.Default, SliceBuilder.Begin(DataEntryPrefix.SYS_Version), version.ToString());
        }

        public void PutCurrentHeader(BlockHeader header)
        {
            this.db.Put(WriteOptions.Default, SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentHeader), SliceBuilder.Begin().Add(header.Hash).Add(header.Height));
        }

        public void PutCurrentBlock(Block block)
        {
            this.db.Put(WriteOptions.Default, SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentBlock), SliceBuilder.Begin().Add(block.Hash).Add(block.Height));
        }

        public void PutBlock(Block block)
        {
            this.db.Put(WriteOptions.Default, SliceBuilder.Begin(DataEntryPrefix.DATA_Block).Add(block.Hash), SliceBuilder.Begin().Add(0L).Add(block.ToArray()));
        }

        public void PutHeaderHashList(int prefix_count, IEnumerable<UInt256> headerHashList)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.WriteSerializableArray(headerHashList);
                bw.Flush();
                this.db.Put(WriteOptions.Default, SliceBuilder.Begin(DataEntryPrefix.IX_HeaderHashList).Add(prefix_count), ms.ToArray());
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
                this.db.Put(WriteOptions.Default, SliceBuilder.Begin(DataEntryPrefix.SYS_CurrentTurnTable), data);
                this.db.Put(WriteOptions.Default, SliceBuilder.Begin(DataEntryPrefix.ST_TurnTable).Add(state.turnTableHeight), data);
            }
        }
        #endregion
    }
}
