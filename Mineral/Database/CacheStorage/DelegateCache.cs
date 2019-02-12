using Mineral.Core;
using Mineral.Core.Transactions;
using Mineral.Database.LevelDB;
using Mineral.Utils;

namespace Mineral.Database.CacheStorage
{
    internal class DelegateCache
    {
        DbCache<UInt160, DelegateState> _cache;

        public DelegateCache(DB db)
        {
            _cache = new DbCache<UInt160, DelegateState>(db, DataEntryPrefix.ST_Delegate);
        }

        public void Add(UInt160 key, byte[] name)
        {
            _cache.Add(key, new DelegateState(key, name));
        }

        public void Vote(VoteTransaction tx)
        {
            foreach (var v in tx.Votes)
            {
                _cache.GetAndChange(v.Key)?.Vote(tx.From, v.Value);
            }
        }

        public void Downvote(System.Collections.Generic.Dictionary<UInt160, Fixed8> Votes)
        {
            foreach (var v in Votes)
            {
                _cache.GetAndChange(v.Key)?.Vote(v.Key, Fixed8.Zero);
            }
        }

        public DelegateState Get(UInt160 key)
        {
            return _cache.TryGet(key);
        }

        public void Commit(WriteBatch batch)
        {
            _cache.Commit(batch);
        }
    }
}
