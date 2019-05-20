using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using Mineral.Core.Database.LevelDB;
using Mineral.Core2;
using Mineral.Core2.State;
using Mineral.Core2.Transactions;
using Mineral.Database.LevelDB;
using Mineral.Utils;

namespace Mineral.Database.BlockChain
{
    public class LevelDBWalletIndexer : WalletIndexer
    {
        // height, group(addressHash)
        private Dictionary<uint, HashSet<UInt160>> _accountGroup = new Dictionary<uint, HashSet<UInt160>>();

        private DB _db;
        private Thread _threadProcessBlock;

        public LevelDBWalletIndexer(string path)
        {
            Version version;
            Slice value;
            ReadOptions options = new ReadOptions { FillCache = false };
            _db = DB.Open(path, new Options { CreateIfMissing = true });
            if (_db.TryGet(ReadOptions.Default, SliceBuilder.Begin(WIDataEntryPrefix.SYS_Version), out value) && Version.TryParse(value.ToString(), out version))
            {
                foreach (var group in _db.Find(options, SliceBuilder.Begin(WIDataEntryPrefix.IX_Group), (k, v) => new
                {
                    Height = k.ToUInt32(1),
                    Id = v.ToArray()
                }))
                {
                    List<UInt160> accounts = _db.Get(options, SliceBuilder.Begin(WIDataEntryPrefix.IX_Accounts).Add(group.Id)).ToArray().SerializableArray<UInt160>();
                    _accountGroup.Add(group.Height, new HashSet<UInt160>(accounts));
                    foreach (UInt160 account in accounts)
                        _accountTracked.Add(account, new HashSet<UInt256>());
                }
                var txGroups = _db.Find(options, SliceBuilder.Begin(WIDataEntryPrefix.ST_Transaction), 
                    (k, v) =>
                    {
                        return new
                        {
                            account = new UInt160(k.ToArray().Skip(1).Take(20).ToArray()),
                            txHash = new UInt256(k.ToArray().Skip(21).ToArray()),
                        };
                    });
                foreach (var txGroup in txGroups)
                {
                    _accountTracked[txGroup.account].Add(txGroup.txHash);
                }
            }
            else
            {
                WriteBatch batch = new WriteBatch();
                using (Iterator it = _db.NewIterator(options))
                {
                    for (it.SeekToFirst(); it.Valid(); it.Next())
                        batch.Delete(it.Key());
                }
                _db.Write(WriteOptions.Default, batch);
                _db.Put(WriteOptions.Default, SliceBuilder.Begin(WIDataEntryPrefix.SYS_Version), Assembly.GetExecutingAssembly().GetName().Version.ToString());
            }

            _threadProcessBlock = new Thread(ProcessBlocksLoop);
            _threadProcessBlock.Name = $"{nameof(LevelDBWalletIndexer)}.{nameof(ProcessBlocksLoop)}";
            _threadProcessBlock.Start();
        }

        public override void AddAccounts(IEnumerable<UInt160> accounts)
        {
            lock (SyncRoot)
            {
                uint height = 0;
                HashSet<UInt160> index;
                bool existGroup = _accountGroup.ContainsKey(height);
                index = existGroup == true ? _accountGroup[height] : new HashSet<UInt160>();
                foreach (UInt160 account in accounts)
                {
                    if (!_accountTracked.ContainsKey(account))
                    {
                        index.Add(account);
                        _accountTracked.Add(account, new HashSet<UInt256>());
                    }
                }
                if (0 < index.Count)
                {
                    WriteBatch batch = new WriteBatch();
                    byte[] groupId;
                    if (existGroup)
                    {
                        groupId = _db.Get(ReadOptions.Default, SliceBuilder.Begin(WIDataEntryPrefix.IX_Group).Add(height)).ToArray();
                    }
                    else
                    {
                        _accountGroup.Add(height, index);
                        groupId = MakeGroupID();
                        batch.Put(SliceBuilder.Begin(WIDataEntryPrefix.IX_Group).Add(height), groupId);
                    }
                    batch.Put(SliceBuilder.Begin(WIDataEntryPrefix.IX_Accounts).Add(groupId), index.ToArray().ToByteArray());
                    _db.Write(WriteOptions.Default, batch);
                }
            }
        }

        public override void RemoveAccounts(IEnumerable<UInt160> accounts)
        {
            lock (SyncRoot)
            {
                WriteBatch batch = new WriteBatch();
                ReadOptions options = new ReadOptions { FillCache = false };
                foreach (UInt160 account in accounts)
                {
                    if (!_accountTracked.ContainsKey(account))
                        continue;
                    // remove account group
                    foreach (uint height in _accountGroup.Keys.ToArray())
                    {
                        var index = _accountGroup[height];
                        if (index.Remove(account))
                        {
                            byte[] groupId = _db.Get(options, SliceBuilder.Begin(WIDataEntryPrefix.IX_Group).Add(height)).ToArray();
                            if (index.Count == 0)
                            {
                                _accountGroup.Remove(height);
                                batch.Delete(SliceBuilder.Begin(WIDataEntryPrefix.IX_Group).Add(height));
                                batch.Delete(SliceBuilder.Begin(WIDataEntryPrefix.IX_Accounts).Add(groupId));
                            }
                            else
                            {
                                batch.Put(SliceBuilder.Begin(WIDataEntryPrefix.IX_Accounts).Add(groupId), index.ToArray().ToByteArray());
                            }
                            break;
                        }
                    }
                    // remove tracked
                    _accountTracked.Remove(account);

                    // remove transaction
                    foreach (Slice key in _db.Find(options, SliceBuilder.Begin(WIDataEntryPrefix.ST_Transaction).Add(account), (k, v) => k))
                        batch.Delete(key);
                }
            }
        }

        private byte[] MakeGroupID()
        {
            byte[] groupId = new byte[32];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(groupId);
            }
            return groupId;
        }

        private void ProcessBlock(Block block, HashSet<UInt160> accounts, WriteBatch batch)
        {
            foreach (Transaction tx in block.Transactions)
            {
                HashSet<UInt160> changed = new HashSet<UInt160>();
                if (accounts.Contains(tx.From) && !changed.Contains(tx.From))
                    changed.Add(tx.From);
                switch (tx.Data)
                {
                    case TransferTransaction transTx:
                        {
                            foreach (UInt160 to in transTx.To.Keys)
                            {
                                if (accounts.Contains(to) && !changed.Contains(to))
                                    changed.Add(to);
                            }
                        }
                        break;
                    case VoteTransaction voteTx:
                        {
                            foreach (UInt160 addr in voteTx.Votes.Keys)
                            {
                                if (accounts.Contains(addr) && !changed.Contains(addr))
                                    changed.Add(addr);
                            }
                        }
                        break;
                    case OtherSignTransaction osignTx:
                        {
                            foreach (UInt160 to in osignTx.To.Keys)
                            {
                                if (accounts.Contains(to) && !changed.Contains(to))
                                    changed.Add(to);                                
                            }
                        }
                        break;
                    case SignTransaction signTx:
                        {
                            foreach (var hash in signTx.TxHashes)
                            {
                                TransactionState txState = Core2.BlockChain.Instance.GetTransaction(hash);
                                if (txState != null)
                                {
                                    var osign = txState.Transaction.Data as OtherSignTransaction;
                                    if (accounts.Contains(osign.From) && !changed.Contains(osign.From))
                                        changed.Add(osign.From);
                                    foreach (UInt160 to in osign.To.Keys)
                                    {
                                        if (accounts.Contains(to) && !changed.Contains(to))
                                            changed.Add(to);
                                    }
                                }
                            }
                        }
                        break;
                }
                if (0 < changed.Count)
                {
                    foreach (UInt160 account in changed) 
                        batch.Put(SliceBuilder.Begin(WIDataEntryPrefix.ST_Transaction).Add(account).Add(tx.Hash), false);

                    TransactionEvent?.Invoke(this, new TransactionEventArgs
                    {
                        Transaction = tx
                    });
                }
            }
            CompletedProcessBlock?.Invoke(this, block.Height);
        }

        private void ProcessBlocksLoop()
        {
            ReadOptions options = ReadOptions.Default;
            bool sleep = false;
            while (true)
            {
                if (sleep)
                {
                    Thread.Sleep(2000);
                    sleep = false;
                }

                lock (SyncRoot)
                {
                    if (_accountGroup.Count == 0)
                    {
                        sleep = true;
                        continue;
                    }
                    uint height = (uint)_accountGroup.Keys.Min();
                    if (Core2.BlockChain.Instance.CurrentBlockHeight <= height)
                    {
                        sleep = true;
                        continue;
                    }

                    Block block = Core2.BlockChain.Instance.GetBlock(height);
                    if (block == null)
                    {
                        sleep = true;
                        continue;
                    }
                    WriteBatch batch = new WriteBatch();
                    HashSet<UInt160> accounts = _accountGroup[height];
                    byte[] groupId = _db.Get(options, SliceBuilder.Begin(WIDataEntryPrefix.IX_Group).Add(height)).ToArray();
                    ProcessBlock(block, accounts, batch);
                    _accountGroup.Remove(height);
                    batch.Delete(SliceBuilder.Begin(WIDataEntryPrefix.IX_Group).Add(height));
                    ++height;
                    if (_accountGroup.TryGetValue(height, out HashSet<UInt160> accountsNext))
                    {
                        // merge group
                        accountsNext.UnionWith(accounts);
                        groupId = _db.Get(options, SliceBuilder.Begin(WIDataEntryPrefix.IX_Group).Add(height)).ToArray();
                        batch.Put(SliceBuilder.Begin(WIDataEntryPrefix.IX_Accounts).Add(groupId), accountsNext.ToArray().ToByteArray());
                    }
                    else
                    {
                        // move group
                        _accountGroup.Add(height, accounts);
                        batch.Put(SliceBuilder.Begin(WIDataEntryPrefix.IX_Group).Add(height), groupId);
                    }
                    _db.Write(WriteOptions.Default, batch);
                }
            }
        }
    }
}
