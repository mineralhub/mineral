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
                if (this.rxPool.ContainsKey(tx.Hash))
                    return false;
                if (this.txPool.ContainsKey(tx.Hash))
                    return false;
                if (this.manager.Storage.GetTransaction(tx.Hash) != null)
                    return false;
                this.rxPool.Add(tx.Hash, tx);
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
                    if (this.txPool.ContainsKey(tx.Hash))
                    {
                        this.txPool.Remove(tx.Hash);
                        nRemove++;
                    }
                    if (this.rxPool.ContainsKey(tx.Hash))
                    {
                        this.rxPool.Remove(tx.Hash);
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
                foreach (Transaction tx in this.rxPool.Values)
                {
                    txs.Add(tx);
                    this.txPool.Add(tx.Hash, tx);
                    if (txs.Count >= Config.Instance.MaxTransactions)
                    {
                        foreach (Transaction rx in this.txPool.Values)
                            this.rxPool.Remove(rx.Hash);
                        return;
                    }
                }
                this.rxPool.Clear();
            }
        }

        public void NormalizeTransactions(ref List<Transaction> txs)
        {
            if (txs.Count == 0)
                return;
            lock (this.waitPersistBlocks)
            {
                foreach (Block block in this.waitPersistBlocks.Values)
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
            }
        }

        public bool HasTransactionPool(UInt256 hash)
        {
            lock (PoolLock)
            {
                if (this.rxPool.ContainsKey(hash))
                    return true;
                if (this.txPool.ContainsKey(hash))
                    return true;
                return false;
            }
        }
    }
}
