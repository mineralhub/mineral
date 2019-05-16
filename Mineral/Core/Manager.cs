using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mineral.Core.State;
using Mineral.Core.Transactions;
using Mineral.Database;
using Mineral.Database.BlockChain;
using Mineral.Database.LevelDB;
using Mineral.Utils;
using Mineral.Wallets;

namespace Mineral.Core
{
    public class Manager
    {
        #region Field
        private LevelDBBlockChain _chain = new LevelDBBlockChain("./output-database");
        private LevelDBWalletIndexer _walletIndexer = new LevelDBWalletIndexer("./output-wallet-index");
        private LevelDBProperty _properties = new LevelDBProperty("./output-property");
        private CacheBlocks _cacheBlocks = null;
        private ForkDatabase _fork_db = new ForkDatabase();

        private const uint _defaultCacheCapacity = 200000;
        #endregion


        #region Property
        internal LevelDBBlockChain Chain { get { return _chain; } }
        internal LevelDBWalletIndexer WalletIndexer { get { return _walletIndexer; } }
        internal LevelDBProperty Properties { get { return _properties; } }
        internal CacheBlocks CacheBlocks { get { return _cacheBlocks; } }
        internal ForkDatabase ForkDB { get { return _fork_db; } }
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public bool Initialize(Block genesisBlock)
        {
            bool result = false;

            if (_chain.TryGetVersion(out Version version))
            {
                if (_chain.TryGetCurrentBlock(out UInt256 currentHash, out uint currentHeight))
                {
                    _cacheBlocks = new CacheBlocks((uint)(currentHeight * 1.1F));

                    uint index = 0;
                    IEnumerable<UInt256> headerHashs = _chain.GetHeaderHashList();
                    foreach (UInt256 headerHash in headerHashs)
                    {
                        _cacheBlocks.AddHeaderHash(index++, headerHash);
                    }

                    if (index == 0)
                    {
                        foreach (BlockHeader blockHeader in _chain.GetBlockHeaderList())
                            _cacheBlocks.AddHeaderHash(blockHeader.Height, blockHeader.Hash);
                    }
                    else if (index <= currentHeight)
                    {
                        UInt256 hash = currentHash;
                        Dictionary<uint, UInt256> headers = new Dictionary<uint, UInt256>();

                        while (hash != _cacheBlocks.GetBlockHash((uint)_cacheBlocks.HeaderCount - 1))
                        {
                            BlockState blockState = _chain.Storage.Block.Get(hash);
                            if (blockState != null)
                            {
                                headers.Add(blockState.Header.Height, blockState.Header.Hash);
                                hash = blockState.Header.PrevHash;
                            }
                        }

                        foreach (var header in headers.OrderBy(x => x.Key))
                            _cacheBlocks.AddHeaderHash(header.Key, header.Value);
                    }

                    result = true;
                }
                else
                {
                    Logger.Error("[Error] " + MethodBase.GetCurrentMethod().Name + " : " + "Not found lastest block.");
                }
            }
            else
            {
                _chain.PutVersion(Assembly.GetExecutingAssembly().GetName().Version);

                _cacheBlocks = new CacheBlocks(_defaultCacheCapacity);
                _cacheBlocks.AddHeaderHash(genesisBlock.Height, genesisBlock.Hash);
                result = true;
            }

            return result;
        }

        public void SwitchFork(Block newBlock)
        {
            Block lastBlock = _chain.GetBlock(_chain.GetCurrentBlockHash());

            KeyValuePair<List<Block>, List<Block>> branches = _fork_db.GetBranch(newBlock.Hash, lastBlock.Hash);

            foreach (Block block in branches.Value)
            {
                _fork_db.Pop();
            }

            foreach (Block block in branches.Key)
            {
                try
                {
                    //ApplyBlock();
                }
                catch (Exception e)
                {
                    foreach (Block key in branches.Key)
                        _fork_db.Pop();

                    foreach (Block value in branches.Value)
                        //ApplyBlock();

                    break;
                }
            }
        }

        public void ApplyBlock(Block block)
        {
            using (Storage snapshot = _chain.SnapShot)
            {
                Fixed8 fee = block.Transactions.Sum(p => p.Fee);
                snapshot.Block.Add(block.Header.Hash, block, fee);

                foreach (Transaction tx in block.Transactions)
                {
                    snapshot.Transaction.Add(tx.Hash, block.Header.Height, tx);
                    if (BlockChain.Instance.GenesisBlock != block && !tx.VerifyBlockChain(_chain.Storage))
                    {
                        if (Fixed8.Zero < tx.Fee)
                        {
                            snapshot.Account.GetAndChange(tx.From).AddBalance(-tx.Fee);
                        }
                        snapshot.TransactionResult.Add(tx.Hash, tx.TxResult);
#if DEBUG
                        Logger.Debug("verified == false transaction. " + tx.ToJson());
#endif
                        continue;
                    }

                    AccountState from = snapshot.Account.GetAndChange(tx.From);
                    if (Fixed8.Zero < tx.Fee)
                        from.AddBalance(-tx.Fee);

                    switch (tx.Data)
                    {
                        case TransferTransaction transTx:
                            {
                                Fixed8 totalAmount = transTx.To.Sum(p => p.Value);
                                from.AddBalance(-totalAmount);
                                foreach (var i in transTx.To)
                                {
                                    snapshot.Account.GetAndChange(i.Key).AddBalance(i.Value);
                                }
                            }
                            break;
                        case VoteTransaction voteTx:
                            {
                                from.LastVoteTxID = tx.Hash;
                                snapshot.Delegate.Downvote(from.Votes);
                                snapshot.Delegate.Vote(voteTx);
                                from.SetVote(voteTx.Votes);
                            }
                            break;
                        case RegisterDelegateTransaction registerDelegateTx:
                            {
                                snapshot.Delegate.Add(registerDelegateTx.From, registerDelegateTx.Name);
                            }
                            break;
                        case OtherSignTransaction osignTx:
                            {
                                Fixed8 totalAmount = osignTx.To.Sum(p => p.Value);
                                from.AddBalance(-totalAmount);
                                snapshot.BlockTrigger.GetAndChange(osignTx.ExpirationBlockHeight).TransactionHashes.Add(osignTx.Owner.Hash);
                                snapshot.OtherSign.Add(osignTx.Owner.Hash, osignTx.Others);
                            }
                            break;
                        case SignTransaction signTx:
                            {
                                for (int i = 0; i < signTx.TxHashes.Count; ++i)
                                {
                                    OtherSignTransactionState state = snapshot.OtherSign.GetAndChange(signTx.TxHashes[i]);
                                    state.Sign(signTx.Owner.Signature);
                                    if (state.RemainSign.Count == 0)
                                    {
                                        TransactionState txState = snapshot.Transaction.Get(state.TxHash);
                                        if (txState != null)
                                        {
                                            var osign = txState.Transaction.Data as OtherSignTransaction;
                                            foreach (var to in osign.To)
                                                snapshot.Account.GetAndChange(to.Key).AddBalance(to.Value);
                                            var trigger = snapshot.BlockTrigger.GetAndChange(signTx.Reference[i].ExpirationBlockHeight);
                                            trigger.TransactionHashes.Remove(signTx.TxHashes[i]);
                                        }
                                    }
                                }
                            }
                            break;

                        case LockTransaction lockTx:
                            {
                                from.LastLockTxID = tx.Hash;
                                from.AddBalance(-lockTx.LockValue);
                                from.AddLock(lockTx.LockValue);
                            }
                            break;

                        case UnlockTransaction unlockTx:
                            {
                                from.LastLockTxID = tx.Hash;
                                Fixed8 lockValue = from.LockBalance;
                                from.AddBalance(lockValue);
                                from.AddLock(-lockValue);
                            }
                            break;
                        case SupplyTransaction rewardTx:
                            {
                                from.AddBalance(rewardTx.Supply);
                            }
                            break;
                    }
                }

                BlockTriggerState blockTrigger = snapshot.BlockTrigger.Get(block.Height);
                if (blockTrigger != null)
                {
                    foreach (UInt256 txhash in blockTrigger.TransactionHashes)
                    {
                        TransactionState txState = snapshot.Transaction.Get(txhash);
                        switch (txState.Transaction.Data)
                        {
                            case OtherSignTransaction osignTx:
                                {
                                    snapshot.Account.GetAndChange(osignTx.From).AddBalance(osignTx.To.Sum(p => p.Value));
                                }
                                break;
                        }
                    }
                }

                if (0 < block.Height)
                {
                    AccountState producer = snapshot.Account.GetAndChange(WalletAccount.ToAddressHash(block.Header.Signature.Pubkey));
                    producer.AddBalance(Config.Instance.BlockReward);
                }

                snapshot.Commit(block.Height);
            }

            WriteBatch batch = new WriteBatch();
            _chain.PutCurrentHeader(batch, block.Header);
            _chain.PutCurrentBlock(batch, block);
            _chain.BatchWrite(WriteOptions.Default, batch);

            //_currentBlockHeight = block.Header.Height;
            //_currentBlockHash = block.Header.Hash;

            Logger.Debug("persist block : " + block.Height);
        }
        #endregion
    }
}
