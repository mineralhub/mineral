using Mineral.Utils;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Mineral.Core
{
    public class CacheChain
    {
        public ConcurrentDictionary<int, UInt256> HeaderIndices { get; } = new ConcurrentDictionary<int, UInt256>();
        public ConcurrentDictionary<UInt256, Block> HashBlocks { get; } = new ConcurrentDictionary<UInt256, Block>();
        private int _capacity = 40960;

        public int HeaderHeight => HeaderIndices.Count - 1;

        public void SetCapacity(int capacity) { _capacity = capacity; }
        public bool AddBlock(Block block)
        {
            if (!HeaderIndices.TryGetValue(block.Height, out UInt256 hash))
                return false;
            if (hash != block.Hash)
                return false;
            if (!HashBlocks.TryAdd(block.Hash, block))
                return false;
            if (_capacity < HashBlocks.Count)
                HashBlocks.TryRemove(HeaderIndices[HeaderIndices.Count - _capacity - 1], out _);
            return true;
        }

        public Block GetBlock(int height)
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

        public bool AddHeaderIndex(int height, UInt256 hash)
        {
            return HeaderIndices.TryAdd(height, hash);
        }
    }
}
