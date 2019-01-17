using Mineral.Utils;
using System.Collections.Concurrent;

namespace Mineral.Core
{
    public class CacheChain
    {
        public ConcurrentDictionary<uint, UInt256> HeaderIndices { get; } = new ConcurrentDictionary<uint, UInt256>();
        public ConcurrentDictionary<UInt256, Block> HashBlocks { get; } = new ConcurrentDictionary<UInt256, Block>();
        private uint _capacity = 40960;

        public uint HeaderHeight => (uint)(HeaderIndices.Count - 1);
        public UInt256 HeaderHash
        {
            get
            {
                if (HeaderIndices.TryGetValue(HeaderHeight, out UInt256 hash))
                    return hash;
                return null;
            }
        }

        public void SetCapacity(uint capacity) { _capacity = capacity; }
        public uint GetCapacity() { return _capacity; }
        public ERROR_BLOCK AddBlock(Block block)
        {
            if (!HeaderIndices.TryGetValue(block.Height, out UInt256 hash))
                return ERROR_BLOCK.ERROR_HEIGHT;
            if (hash != block.Hash)
                return ERROR_BLOCK.ERROR_HASH;
            if (!HashBlocks.TryAdd(block.Hash, block))
                return ERROR_BLOCK.ERROR_HASH;
            if (_capacity < HashBlocks.Count)
                HashBlocks.TryRemove(HeaderIndices[(uint)HeaderIndices.Count -_capacity - 1], out _);
            return ERROR_BLOCK.NO_ERROR;
        }

        public Block GetBlock(uint height)
        {
            if (!HeaderIndices.TryGetValue(height, out UInt256 hash))
                return null;
            return GetBlock(hash);
        }

        public Block GetBlock(UInt256 hash)
        {
            HashBlocks.TryGetValue(hash, out Block block);
            return block;
        }

        public bool AddHeaderIndex(uint height, UInt256 hash)
        {
            return HeaderIndices.TryAdd(height, hash);
        }
    }
}
