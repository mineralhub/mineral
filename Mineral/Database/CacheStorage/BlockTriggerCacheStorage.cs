using Mineral.Core;
using Mineral.Database.LevelDB;
using System.Collections.Generic;

namespace Mineral.Database.CacheStorage
{
    internal class BlockTriggerCacheStorage
    {
        DbCache<SerializeInteger, BlockTriggerState> _cache;

        public BlockTriggerCacheStorage(DB db)
        {
            _cache = new DbCache<SerializeInteger, BlockTriggerState>(db, DataEntryPrefix.ST_BlockTrigger);
        }

        public BlockTriggerState GetAndChange(int height)
        {
            return GetAndChange(new SerializeInteger(height));
        }

        public BlockTriggerState GetAndChange(SerializeInteger height)
        {
            return _cache.GetAndChange(height, () => new BlockTriggerState());
        }

        public BlockTriggerState TryGet(int height)
        {
            return _cache.TryGet(new SerializeInteger(height));
        }

        public void Clean(int height)
        {
            _cache.DeleteWhere((k, v) => k <= height);
        }

        public void Commit(WriteBatch batch)
        {
            _cache.Commit(batch);
        }
    }
}
