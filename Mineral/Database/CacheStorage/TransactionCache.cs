using Mineral.Core;
using Mineral.Core.State;
using Mineral.Core.Transactions;
using Mineral.Database.LevelDB;
using Mineral.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Database.CacheStorage
{
    internal class TransactionCache
    {
        private DbCache<UInt256, TransactionState> _cache;

        public TransactionCache(DB db)
        {
            _cache = new DbCache<UInt256, TransactionState>(db, DataEntryPrefix.DATA_Transaction);
        }

        public void Add(UInt256 hash, uint height, Transaction tx)
        {
            _cache.Add(hash, new TransactionState(height, tx));
        }

        public void Add(TransactionState txState)
        {
            _cache.Add(txState.Transaction.Hash, txState);
        }

        public TransactionState Get(UInt256 hash)
        {
            return _cache.TryGet(hash);
        }

        public void Commit(WriteBatch batch)
        {
            _cache.Commit(batch);
        }
    }
}
