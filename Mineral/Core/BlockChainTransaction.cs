using Mineral.Core.Transactions;
using Mineral.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mineral.Core
{
    public partial class BlockChain
    {
        public bool AddTransactionPool(Transaction tx)
        {
            if (!tx.Verify())
                return false;

            lock (PoolLock)
            {
                if (_rxPool.ContainsKey(tx.Hash))
                    return false;
                if (_txPool.ContainsKey(tx.Hash))
                    return false;
                if (_manager.Storage.GetTransaction(tx.Hash) != null)
                    return false;
                _rxPool.Add(tx.Hash, tx);
                return true;
            }
        }

        public void AddTransactionPool(List<Transaction> txs)
        {
            foreach (var tx in txs)
                AddTransactionPool(tx);
        }

        public int RemoveTransactionPool(List<Transaction> txs)
        {
            int nRemove = 0;
            lock (PoolLock)
            {
                foreach (Transaction tx in txs)
                {
                    if (_txPool.ContainsKey(tx.Hash))
                    {
                        _txPool.Remove(tx.Hash);
                        nRemove++;
                    }
                    if (_rxPool.ContainsKey(tx.Hash))
                    {
                        _rxPool.Remove(tx.Hash);
                        nRemove++;
                    }
                }
            }
            return nRemove;
        }

        public void LoadTransactionPool(ref List<Transaction> txs)
        {
            if (txs == null)
                txs = new List<Transaction>();
            lock (PoolLock)
            {
                foreach (Transaction tx in _rxPool.Values)
                {
                    txs.Add(tx);
                    _txPool.Add(tx.Hash, tx);
                    if (txs.Count >= Config.Instance.MaxTransactions)
                    {
                        foreach (Transaction rx in _txPool.Values)
                            _rxPool.Remove(rx.Hash);
                        return;
                    }
                }
                _rxPool.Clear();
            }
        }

        public void NormalizeTransactions(ref List<Transaction> txs)
        {
            /*
            if (txs.Count == 0)
                return;
            foreach (Block block in _persistBlocks.Values)
            {
                int counter = txs.Count;
                while (counter > 0)
                {
                    counter--;
                    Transaction tx = txs.ElementAt(0);
                    txs.RemoveAt(0);
                    if (block.Transactions.Find((p) => { return p.Hash == tx.Hash; }) == null)
                        txs.Add(tx);
                }
            }
            */
        }

        public bool HasTransactionPool(UInt256 hash)
        {
            lock (PoolLock)
            {
                if (_rxPool.ContainsKey(hash))
                    return true;
                if (_txPool.ContainsKey(hash))
                    return true;
                return false;
            }
        }
    }
}
