using Mineral.Utils;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Mineral.Core
{
    public class CacheBlocks
    {
        private ConcurrentDictionary<uint, UInt256> _headerIndices = null;
        private ConcurrentDictionary<UInt256, Block> _hashBlocks = null;
        public int HeaderCount => _headerIndices.Count;
        public int BlockCount => _hashBlocks.Count;

        public uint Capacity { get; set; }
        public uint HeaderHeight => (uint)(_headerIndices.Count > 0 ? _headerIndices.Count - 1 : 0);
        public UInt256 HeaderHash
        {
            get
            {
                if (_headerIndices.TryGetValue(HeaderHeight, out UInt256 hash))
                    return hash;
                return null;
            }
        }

        private CacheBlocks() { }
        public CacheBlocks(uint header_capacity)
        {
            _headerIndices = new ConcurrentDictionary<uint, UInt256>(System.Environment.ProcessorCount * 2, (int)header_capacity);
            _hashBlocks = new ConcurrentDictionary<UInt256, Block>();
        }

        public bool AddHeaderHash(uint height, UInt256 hash)
        {
            if (HeaderHeight != 0 && HeaderHeight + 1 != height) return false;
            return _headerIndices.TryAdd(height, hash);
        }

        public ERROR_BLOCK AddBlock(Block block)
        {
            if (!_headerIndices.TryGetValue(block.Height, out UInt256 hash))
                return ERROR_BLOCK.ERROR_HEIGHT;
            if (hash != block.Hash)
                return ERROR_BLOCK.ERROR_HASH;
            if (!_hashBlocks.TryAdd(block.Hash, block))
                return ERROR_BLOCK.ERROR_HASH;
            if (Capacity < _hashBlocks.Count)
                _hashBlocks.TryRemove(_headerIndices[(uint)_headerIndices.Count - Capacity - 1], out _);
            return ERROR_BLOCK.NO_ERROR;
        }

        public Block GetBlock(uint height)
        {
            if (!_headerIndices.TryGetValue(height, out UInt256 hash))
                return null;
            return GetBlock(hash);
        }

        public Block GetBlock(UInt256 hash)
        {
            _hashBlocks.TryGetValue(hash, out Block block);
            return block;
        }

        public UInt256 GetBlockHash(uint height)
        {
            _headerIndices.TryGetValue(height, out UInt256 hash);
            return hash;
        }

        public IEnumerable<UInt256> GetBlcokHashs(uint start, uint end)
        {
            for (uint i = start; i < end; i++)
            {
                if (_headerIndices.TryGetValue(i, out UInt256 hash))
                    yield return hash;
            }
        }

        public KeyValuePair<List<Block>, List<Block>> GetBranch(UInt256 hash1, UInt256 hash2)
        {
            List<Block> keys = new List<Block>();
            List<Block> values = new List<Block>();
            Block block1 = null;
            Block block2 = null;

            _hashBlocks.TryGetValue(hash1, out block1);
            _hashBlocks.TryGetValue(hash2, out block2);

            if (block1 == null && block2 != null)
            {
                while (!object.Equals(block1.Hash, block2.Hash))
                {
                    if (block1.Height > block2.Height)
                    {
                        keys.Add(block1);
                        _hashBlocks.TryGetValue(block1.Header.PrevHash, out block1);
                    }
                    else if (block1.Height < block2.Height)
                    {
                        values.Add(block2);
                        _hashBlocks.TryGetValue(block2.Header.PrevHash, out block2);
                    }
                    else
                    {
                        keys.Add(block1);
                        _hashBlocks.TryGetValue(block1.Header.PrevHash, out block1);
                        values.Add(block2);
                        _hashBlocks.TryGetValue(block2.Header.PrevHash, out block2);
                    }
                }
            }

            keys.Reverse();
            values.Reverse();

            return new KeyValuePair<List<Block>, List<Block>>(keys, values);
        }
    }
}
