using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core;
using Mineral.Database.CacheStorage;
using Mineral.Database.LevelDB;

namespace Mineral.Database.LevelDB
{
    public class Storage : IDisposable
    {
        private DB _db = null;
        private ReadOptions _opt = ReadOptions.Default;
        private AccountCacheStorage accounts = null;
        private DelegateCacheStorage delegates = null;
        private OtherSignCacheStorage otherSignTxs = null;
        private BlockTriggerCacheStorage blockTriggers = null;

        internal static Storage NewStorage(DB _db, ReadOptions opt = null)
        {
            Storage sto = new Storage(_db);
            if (opt != null)
                sto._opt = opt;
            return sto;
        }

        public void Dispose()
        {
            accounts = null;
            delegates = null;
            otherSignTxs = null;
            blockTriggers = null;
            _opt = null;
            _db = null;
        }

        private Storage(DB db)
        {
            _db = db;
            accounts = new AccountCacheStorage(_db);
            delegates = new DelegateCacheStorage(_db);
            otherSignTxs = new OtherSignCacheStorage(_db);
            blockTriggers = new BlockTriggerCacheStorage(_db);
        }

        internal void commit(WriteBatch batch, int height)
        {
            accounts.Clean();
            accounts.Commit(batch);
            delegates.Commit(batch);
            otherSignTxs.Commit(batch);
            blockTriggers.Clean(height);
            blockTriggers.Commit(batch);

        }

        public AccountState GetAccountState(UInt160 hash)
        {
            return accounts.GetAndChange(hash);
        }

        public DelegateState GetDelegateState(UInt160 hash)
        {
            return delegates.TryGet(hash);
        }

        public void AddDelegate(UInt160 key, byte[] name)
        {
            delegates.Add(key, name);
        }

        public void Vote(VoteTransaction tx)
        {
            delegates.Vote(tx);
        }

        public void Downvote(Dictionary<UInt160, Fixed8> Votes)
        {
            delegates.Downvote(Votes);
        }

        public void AddOtherSignTxs(UInt256 hash, HashSet<string> others)
        {
            otherSignTxs.Add(hash, others);
        }

        public OtherSignTransactionState GetOtherSignTxs(UInt256 hash)
        {
            return otherSignTxs.GetAndChange(hash);
        }

        public BlockTriggerState GetBlockTriggers(int height)
        {
            return blockTriggers.GetAndChange(height);
        }

        public BlockTriggerState TryBlockTriggers(int height)
        {
            return blockTriggers.TryGet(height);
        }

        public Transaction GetTransaction(UInt256 hash)
        {
            return GetTransaction(hash, out _);
        }

        public Transaction GetTransaction(UInt256 hash, out int height)
        {
            return GetTransaction(ReadOptions.Default, hash, out height);
        }

        private Transaction GetTransaction(ReadOptions options, UInt256 hash, out int height)
        {
            Slice value;
            if (_db.TryGet(options, SliceBuilder.Begin(DataEntryPrefix.DATA_Transaction).Add(hash), out value))
            {
                byte[] data = value.ToArray();
                height = data.ToInt32(0);
                return Transaction.DeserializeFrom(data, sizeof(uint));
            }
            else
            {
                height = -1;
                return null;
            }
        }

        public List<DelegateState> GetCadidateDelgates()
        {
            return Blockchain.Instance.GetDelegateStateAll();
        }
    }
}
