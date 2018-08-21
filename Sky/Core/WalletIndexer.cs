using System;
using System.Collections.Generic;

namespace Sky.Core
{
    public abstract class WalletIndexer
    {
        public class TransactionEventArgs : EventArgs
        {
            public Transaction Transaction;
        }

        static private WalletIndexer _instance = null;
        static public WalletIndexer Instance => _instance;

        public EventHandler<TransactionEventArgs> TransactionEvent;
        public EventHandler<int> CompletedProcessBlock;

        protected object SyncRoot = new object();
        protected Dictionary<UInt160, HashSet<UInt256>> _accountTracked = new Dictionary<UInt160, HashSet<UInt256>>();

        static public void SetInstance(WalletIndexer indexer)
        {
            _instance = indexer;
        }

        public abstract void AddAccounts(IEnumerable<UInt160> accounts);
        public abstract void RemoveAccounts(IEnumerable<UInt160> accounts);
    }
}
