using Mineral.Core.Database.LevelDB;
using Mineral.Core2;
using Mineral.Core2.State;
using Mineral.Database.BlockChain;
using Mineral.Database.LevelDB;
using Mineral.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Database.CacheStorage
{
    internal class BlockCache
    {
        private DbCache<UInt256, BlockState> _cache;

        public BlockCache(DB db)
        {
            _cache = new DbCache<UInt256, BlockState>(db, DataEntryPrefix.DATA_Block);
        }

        public void Add(UInt256 hash, Block block, Fixed8 fee = default(Fixed8))
        {
            _cache.Add(hash, new BlockState(block, fee));
        }

        public BlockState Get(UInt256 hash)
        {
            return _cache.TryGet(hash);
        }

        public void Commit(WriteBatch batch)
        {
            _cache.Commit(batch);
        }
    }
}
