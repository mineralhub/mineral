using System;
using System.Collections.Generic;

namespace Sky.Core
{
    public abstract class WalletIndexer
    {
        public class BalanceEventArgs : EventArgs
        {
            public Transaction Transaction;
            public Dictionary<UInt160, List<Fixed8>> ChangedAccount; // addressHash, added balance
            public int Height;
            public int Time;
        }

        static private WalletIndexer _instance = null;
        static public WalletIndexer Instance => _instance;

        public EventHandler<BalanceEventArgs> BalanceChange;
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
