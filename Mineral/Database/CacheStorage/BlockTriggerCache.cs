using Mineral.Core;
using Mineral.Database.LevelDB;
using Mineral.Utils;

namespace Mineral.Database.CacheStorage
{
    internal class BlockTriggerCache
    {
        DbCache<SerializableUInt32, BlockTriggerState> _cache;

        public BlockTriggerCache(DB db)
        {
            _cache = new DbCache<SerializableUInt32, BlockTriggerState>(db, DataEntryPrefix.ST_BlockTrigger);
        }

        public BlockTriggerState GetAndChange(uint height)
        {
            return GetAndChange(new SerializableUInt32(height));
        }

        public BlockTriggerState GetAndChange(SerializableUInt32 height)
        {
            return _cache.GetAndChange(height, () => new BlockTriggerState());
        }

        public BlockTriggerState Get(uint height)
        {
            return _cache.TryGet(new SerializableUInt32(height));
        }

        public void Clean(uint height)
        {
            _cache.DeleteWhere((k, v) => k <= height);
        }

        public void Commit(WriteBatch batch)
        {
            _cache.Commit(batch);
        }
    }
}
