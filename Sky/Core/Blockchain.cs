using System;
using System.Collections.Generic;

namespace Sky.Core
{
    public abstract class Blockchain : IDisposable
    {
        static private Blockchain _instance = null;
        static public Blockchain Instance => _instance;

        static public void SetInstance(Blockchain chain)
        {
            _instance = chain;
        }

        public object PersistLock { get; } = new object();
        public event EventHandler<Block> PersistCompleted;

        public abstract Block GenesisBlock { get; }
        public abstract int CurrentBlockHeight { get; }
        public abstract UInt256 CurrentBlockHash { get; }

        public abstract int CurrentHeaderHeight { get; }
        public abstract UInt256 CurrentHeaderHash { get; }

        public abstract void Run();
        public abstract void Dispose();
        public abstract bool AddBlock(Block block);
        public abstract bool AddBlockDirectly(Block block);
        public abstract BlockHeader GetHeader(UInt256 hash);
        public abstract BlockHeader GetHeader(int height);
        public abstract BlockHeader GetNextHeader(UInt256 hash);
        public abstract bool ContainsBlock(UInt256 hash);
        public abstract Block GetBlock(UInt256 hash);
        public abstract Block GetBlock(int height);

        public Transaction GetTransaction(UInt256 hash)
        {
            return GetTransaction(hash, out _);
        }

        public abstract Transaction GetTransaction(UInt256 hash, out int height);

        public abstract AccountState GetAccountState(UInt160 addressHash);
        public abstract List<DelegateState> GetDelegateStateAll();
        public abstract List<DelegateState> GetDelegateStateMakers();

        protected void OnPersistCompleted(Block block)
        {
            PersistCompleted?.Invoke(this, block);
        }
    }
}