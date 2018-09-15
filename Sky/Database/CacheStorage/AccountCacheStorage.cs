using Sky.Core;
using Sky.Database.LevelDB;

namespace Sky.Database.CacheStorage
{
    internal class AccountCacheStorage
    {
        DbCache<UInt160, AccountState> _cache;

        public AccountCacheStorage(DB db)
        {
            _cache = new DbCache<UInt160, AccountState>(db, DataEntryPrefix.ST_Account);
        }

        public AccountState GetAndChange(UInt160 key)
        {
            return _cache.GetAndChange(key, () => new AccountState(key));
        }

        public void Clean()
        {
            _cache.DeleteWhere((k, v) => !v.IsFrozen && v.Balance <= Fixed8.Zero && v.Votes == null);
        }

        public void Commit(WriteBatch batch)
        {
            _cache.Commit(batch);
        }
    }
}
