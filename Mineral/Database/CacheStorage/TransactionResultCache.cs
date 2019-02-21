using Mineral.Core;
using Mineral.Core.State;
using Mineral.Database.LevelDB;
using Mineral.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Database.CacheStorage
{
    internal class TransactionResultCache
    {
        private DbCache<UInt256, TransactionResultState> _cache;

        public TransactionResultCache(DB db)
        {
            _cache = new DbCache<UInt256, TransactionResultState>(db, DataEntryPrefix.DATA_TransactionResult);
        }

        public void Add(UInt256 hash, MINERAL_ERROR_CODES txResult)
        {
            _cache.Add(hash, new TransactionResultState(txResult));
        }

        public TransactionResultState TryGet(UInt256 hash)
        {
            return _cache.TryGet(hash);
        }

        public void Commit(WriteBatch batch)
        {
            _cache.Commit(batch);
        }
    }
}
