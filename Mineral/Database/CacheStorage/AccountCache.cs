using Mineral.Core.Database.LevelDB;
using Mineral.Core2;
using Mineral.Core2.State;
using Mineral.Database.BlockChain;
using Mineral.Database.LevelDB;
using Mineral.Utils;

namespace Mineral.Database.CacheStorage
{
    internal class AccountCache
    {
        DbCache<UInt160, AccountState> _cache;

        public AccountCache(DB db)
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
