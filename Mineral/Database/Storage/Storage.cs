using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core2;
using Mineral.Core2.State;
using Mineral.Core2.Transactions;
using Mineral.Database.CacheStorage;
using Mineral.Database.LevelDB;
using Mineral.Utils;

namespace Mineral.Database.LevelDB
{
    public class Storage : IDisposable
    {
        private DB _db = null;
        private ReadOptions _opt = ReadOptions.Default;

        private BlockCache _block = null;
        private TransactionCache _transaction = null;
        private TransactionResultCache _transactionResult = null;
        private AccountCache _accounts = null;
        private DelegateCache _delegates = null;
        private OtherSignCache _otherSignTxs = null;
        private BlockTriggerCache _blockTriggers = null;

        internal BlockCache Block { get { return _block; } }
        internal TransactionCache Transaction { get { return _transaction; } }
        internal TransactionResultCache TransactionResult { get { return _transactionResult; } }
        internal AccountCache Account { get { return _accounts; } }
        internal DelegateCache Delegate { get { return _delegates; } }
        internal OtherSignCache OtherSign { get { return _otherSignTxs; } }
        internal BlockTriggerCache BlockTrigger { get { return _blockTriggers; } }

        internal static Storage NewStorage(DB _db, ReadOptions opt = null)
        {
            Storage sto = new Storage(_db);
            if (opt != null)
                sto._opt = opt;
            return sto;
        }

        public void Dispose()
        {
            _block = null;
            _transaction = null;
            _transactionResult = null;
            _accounts = null;
            _delegates = null;
            _otherSignTxs = null;
            _blockTriggers = null;
            _opt = null;
            _db = null;
        }

        private Storage(DB db)
        {
            _db = db;
            _block = new BlockCache(_db);
            _transaction = new TransactionCache(_db);
            _transactionResult = new TransactionResultCache(_db);
            _accounts = new AccountCache(_db);
            _delegates = new DelegateCache(_db);
            _otherSignTxs = new OtherSignCache(_db);
            _blockTriggers = new BlockTriggerCache(_db);
        }

        internal void Commit(uint height)
        {
            WriteBatch batch = new WriteBatch();
            _block.Commit(batch);
            _transaction.Commit(batch);
            _transactionResult.Commit(batch);
            _accounts.Clean();
            _accounts.Commit(batch);
            _delegates.Commit(batch);
            _otherSignTxs.Commit(batch);
            _blockTriggers.Clean(height);
            _blockTriggers.Commit(batch);
            _db.Write(WriteOptions.Default, batch);
        }
    }
}
