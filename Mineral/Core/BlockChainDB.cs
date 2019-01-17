using Mineral.Core.Transactions;
using Mineral.Database.BlockChain;
using Mineral.Database.LevelDB;
using Mineral.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mineral.Core
{
    public partial class BlockChain
    {
        private LevelDBBlockChain _manager = null;

        public Storage NewStorage()
        {
            _manager.NewStorage();
            return _manager.Storage;
        }

        #region Block Header
        public BlockHeader GetHeader(uint height)
        {
            Block block = _cacheChain.GetBlock(height);
            if (block != null)
                return block.Header;

            UInt256 hash = GetBlockHash(height);
            if (hash == null)
                return null;

            BlockHeader header = null;
            _manager.TryGetBlockHeader(hash, out header);

            return header;
        }

        public BlockHeader GetHeader(UInt256 hash)
        {
            Block block = _cacheChain.GetBlock(hash);
            if (block != null)
                return block.Header;

            BlockHeader header = null;
            _manager.TryGetBlockHeader(hash, out header);

            return header;
        }

        public BlockHeader GetNextHeader(UInt256 hash)
        {
            BlockHeader header = GetHeader(hash);
            if (header == null)
                return null;

            return GetHeader(header.Height + 1);
        }

        public bool ContainsBlock(UInt256 hash)
        {
            return GetHeader(hash)?.Height <= _currentBlockHeight;
        }
        #endregion


        #region Block
        public Block GetBlock(uint height)
        {
            Block block = _cacheChain.GetBlock(height);
            if (block != null)
                return block;

            UInt256 hash = GetBlockHash(height);
            if (hash == null)
                return null;

            return GetBlock(hash);
        }

        public Block GetBlock(UInt256 hash)
        {
            Block block = _cacheChain.GetBlock(hash);
            if (block != null)
                return block;

            block = _manager.GetBlock(hash);
            return block;
        }

        public UInt256 GetBlockHash(uint height)
        {
            _cacheChain.HeaderIndices.TryGetValue(height, out UInt256 hash);
            return hash;
        }

        public Block GetNextBlock(UInt256 hash)
        {
            Block block = GetBlock(hash);
            if (block == null)
                return null;

            return GetBlock(block.Height + 1);
        }

        public List<Block> GetBlocks(uint start, uint end)
        {
            var hashes = _cacheChain.HeaderIndices.Values.Skip((int)start).Take((int)(end - start));
            List<Block> blocks = new List<Block>();
            foreach (var hash in hashes)
            {
                Block block = GetBlock(hash);
                if (block == null)
                    break;
                blocks.Add(block);
            }
            return blocks;
        }
        #endregion


        #region Transaction
        public Transaction GetTransaction(UInt256 hash)
        {
            return _manager.Storage.GetTransaction(hash);
        }
        #endregion


        #region Account
        public AccountState GetAccountState(UInt160 hash)
        {
            return _manager.Storage.GetAccountState(hash);
        }
        #endregion


        #region Turn table
        public TurnTableState GetTurnTable(uint height)
        {
            List<uint> heights = _manager.GetTurnTableHeightList(height).ToList();

            heights.Sort((a, b) => { return a > b ? (-1) : (a < b ? 1 : 0); });
            return _manager.GetTurnTable(heights.Count > 0 ? heights.First() : 0);
        }
        #endregion


        #region Delegate
        public DelegateState GetDelegateState(UInt160 hash)
        {
            return _manager.Storage.GetDelegateState(hash);
        }

        public List<DelegateState> GetDelegateStateAll()
        {
            return _manager.GetDelegateStateAll().ToList();
        }
        #endregion
    }
}
