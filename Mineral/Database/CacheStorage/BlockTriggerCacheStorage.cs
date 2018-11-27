using Mineral.Core;
using Mineral.Database.LevelDB;

namespace Mineral.Database.CacheStorage
{
    internal class BlockTriggerCacheStorage
    {
        DbCache<SerializableInt32, BlockTriggerState> _cache;

        public BlockTriggerCacheStorage(DB db)
        {
            _cache = new DbCache<SerializableInt32, BlockTriggerState>(db, DataEntryPrefix.ST_BlockTrigger);
        }

        public BlockTriggerState GetAndChange(int height)
        {
            return GetAndChange(new SerializableInt32(height));
        }

        public BlockTriggerState GetAndChange(SerializableInt32 height)
        {
            return _cache.GetAndChange(height, () => new BlockTriggerState());
        }

        public BlockTriggerState TryGet(int height)
        {
            return _cache.TryGet(new SerializableInt32(height));
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
