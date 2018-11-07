using System;
using System.Text;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Sky;
using Sky.Cryptography;
using Sky.Core;
using Sky.Wallets;
using Sky.Core.DPos;
using Sky.Network;
using Sky.Network.Payload;
using Sky.Network.RPC;

namespace Tester
{
    public class MainService
    {
        short BlockVersion => Config.Instance.BlockVersion;
        int GenesisBlockTimestamp => Config.Instance.GenesisBlock.Timestamp;
        WalletAccount _account;
        WalletAccount _fromAccount;
        Block _genesisBlock;
        LocalNode _node;
        RpcServer _rpcServer;
        //DPos _dpos;

        public void Run()
        {
            Logger.WriteConsole = true;

            // Generate Address
            /*
            for (int i = 0 ; i < 5 ;++i)
            {
                var account = new WalletAccount(Sky.Cryptography.Helper.SHA256(Encoding.ASCII.GetBytes((i+1).ToString())));
                Logger.Log((i+1).ToString());
                Logger.Log(account.Address);
            }
            return;
            */
            Config.Instance.Initialize();

            Initialize();
            if (ValidAccount() == false)
                return;
            if (ValidBlock() == false)
                return;

            StartLocalNode();
            StartRpcServer();
            // Config.Instance.GenesisBlock.Delegates.ForEach(p => dpos.TurnTable.Enqueue(p.Address));

            while (true)
            {
                do
                {
                    // delegator?
                    if (!_account.IsDelegate())
                        break;
                    // my turn?
                    if (_account.AddressHash != Blockchain.Instance.GetTurn())
                        break;
                    // create
                    //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                    //sw.Start();
                    CreateAndAddBlocks(1, true);
                    //sw.Stop();
                    //Logger.Log("AddBlock Elapsed=" + sw.Elapsed);
                }
                while (false);
                Thread.Sleep(100);
            }
        }

        void CreateAndAddBlocks(int cnt, bool directly)
        {
            List<Block> blocks = new List<Block>();
            int height = Blockchain.Instance.CurrentHeaderHeight;
            UInt256 prevhash = Blockchain.Instance.CurrentHeaderHash;
            List<Transaction> txs = new List<Transaction>();

            // Transaction TPS Check.
            /*
            var tx = CreateTransferTransaction();
            for (int i = 0; i < 1000; ++i)
                txs.Add(tx);
            */
            for (int i = 0; i < cnt; ++i)
            {
                txs.Clear();
                Blockchain.Instance.LoadTransactionPool(ref txs);
                Blockchain.Instance.NormalizeTransactions(ref txs);
                Block block = CreateBlock(height + i, prevhash, txs);

                if (!Blockchain.Instance.VerityBlock(block))
                {
                    Logger.Log("Block [" + block.Height + ":" + block.Hash + "] has unconfirmed transactions.");
                    if (!Blockchain.Instance.VerityBlock(block))
                    {
                        Logger.Log("Block [" + block.Height + ":" + block.Hash + "] has not verified.");
                    }
                }

                prevhash = block.Hash;

                if (directly)
                {
                    Blockchain.Instance.AddBlockDirectly(block);
                }
                else
                {
                    Blockchain.Instance.AddBlock(block);
                }
                blocks.Add(block);
            }

            if (directly)
                _node.BroadCast(Message.CommandName.BroadcastBlocks, BroadcastBlockPayload.Create(blocks));
        }

        void Initialize()
        {
            Logger.Log("---------- Initialize ----------");
            //_account = new WalletAccount(Sky.Cryptography.Helper.SHA256(Config.Instance.User.PrivateKey));
            _account = new WalletAccount(Sky.Cryptography.Helper.SHA256(new byte[1]));
            _fromAccount = new WalletAccount(Sky.Cryptography.Helper.SHA256(Encoding.Default.GetBytes("256")));

            // create genesis block.
            {
                List<RewardTransaction> rewardTxs = new List<RewardTransaction>();
                Config.Instance.GenesisBlock.Accounts.ForEach(
                    p =>
                    {
                        rewardTxs.Add(new RewardTransaction
                        {
                            From = p.Address,
                            Reward = p.Balance
                        });
                    });

                var txs = new List<Transaction>();
                foreach (var reward in rewardTxs)
                {
                    var tx = new Transaction(eTransactionType.RewardTransaction,
                                    GenesisBlockTimestamp - 1,
                                    reward)
                    {
                        Signature = new MakerSignature()
                    };
                    txs.Add(tx);
                }

                Config.Instance.GenesisBlock.Delegates.ForEach(
                    p =>
                    {
                        var register = new RegisterDelegateTransaction
                        {
                            Name = Encoding.UTF8.GetBytes(p.Name),
                            From = p.Address
                        };
                        var tx = new Transaction(eTransactionType.RegisterDelegateTransaction,
                                                 GenesisBlockTimestamp - 1,
                                                 register)
                        {
                            Signature = new MakerSignature()
                        };
                        txs.Add(tx);
                    });

                var merkle = new MerkleTree(txs.Select(p => p.Hash).ToArray());
                var blockHeader = new BlockHeader
                {
                    PrevHash = UInt256.Zero,
                    MerkleRoot = merkle.RootHash,
                    Version = BlockVersion,
                    Timestamp = GenesisBlockTimestamp,
                    Height = 0,
                    Signature = new MakerSignature()
                };
                _genesisBlock = new Block(blockHeader, txs);
            }

            Logger.Log("genesis block. hash : " + _genesisBlock.Hash);
            Blockchain.SetInstance(new Sky.Database.LevelDB.LevelDBBlockchain("./output-database", _genesisBlock));
            Blockchain.Instance.PersistCompleted += PersistCompleted;
            Blockchain.Instance.Run();

            var genesisBlockTx = Blockchain.Instance.storage.GetTransaction(_genesisBlock.Transactions[0].Hash);
            Logger.Log("genesis block tx. hash : " + genesisBlockTx.Hash);

            WalletIndexer.SetInstance(new Sky.Database.LevelDB.LevelDBWalletIndexer("./output-wallet-index"));
            /*
            var accounts = new List<UInt160> { _delegator.AddressHash, _randomAccount.AddressHash };
            WalletIndexer.Instance.AddAccounts(accounts);
            WalletIndexer.Instance.BalanceChange += (obj, e) =>
            {
                foreach (var addrHash in e.ChangedAccount.Keys)
                {
                    foreach (var value in e.ChangedAccount[addrHash])
                    {
                        //Logger.Log("change account. addr : " + WalletAccount.ToAddress(addrHash) + ", added : " + value);
                    }
                }
            };
            */
        }

        Block CreateBlock(int height, UInt256 prevhash, List<Transaction> txs = null)
        {
            if (txs == null)
                txs = new List<Transaction>();

            // block reward
            RewardTransaction rewardTx = new RewardTransaction
            {
                From = _account.AddressHash,
                Reward = Config.Instance.BlockReward
            };
            var tx = new Transaction(
                eTransactionType.RewardTransaction,
                DateTime.UtcNow.ToTimestamp(),
                rewardTx
            );
            tx.Sign(_account);
            txs.Insert(0, tx);

            var merkle = new MerkleTree(txs.Select(p => p.Hash).ToArray());
            var blockHeader = new BlockHeader
            {
                PrevHash = prevhash,
                MerkleRoot = merkle.RootHash,
                Version = BlockVersion,
                Timestamp = DateTime.UtcNow.ToTimestamp(),
                Height = height + 1
            };
            blockHeader.Sign(_account.Key);
            return new Block(blockHeader, txs);
        }

        Transaction CreateTransferTransaction()
        {
            var trans = new TransferTransaction
            {
                From = _account.AddressHash,
                To = new Dictionary<UInt160, Fixed8> { { _fromAccount.AddressHash, Fixed8.Satoshi } }
            };
            var tx = new Transaction(eTransactionType.TransferTransaction, DateTime.UtcNow.ToTimestamp(), trans);
            tx.Sign(_account);
            return tx;
        }

        /*
        VoteTransaction CreateVoteTransaction()
        {
            // find genesis block input
            List<TransactionInput> txIn = new List<TransactionInput>();
            Fixed8 value = Fixed8.Zero;
            foreach (var tx in _genesisBlock.Transactions)
            {
                for (int i = 0; i < tx.Outputs.Count; ++i)
                {
                    if (tx.Outputs[i].AddressHash == _account.AddressHash)
                    {
                        txIn.Add(new TransactionInput(tx.Hash, (ushort)i));
                        value = tx.Outputs[i].Value;
                        break;
                    }
                }
                if (0 < txIn.Count)
                    break;
            }
            if (txIn.Count == 0)
                return null;

            var txOut = new List<TransactionOutput> { new TransactionOutput(value - Config.Instance.VoteFee, _account.AddressHash) };
            var txSign = new List<MakerSignature> { new MakerSignature(Sky.Cryptography.Helper.Sign(txIn[0].Hash.Data, _account.Key), _account.Key.PublicKey.ToByteArray()) };
            return new VoteTransaction(txIn, txOut, txSign);
        }
        */

        void PersistCompleted(object sender, Block block)
        {
        }

        void UpdateTurnTable()
        {
            int currentHeight = Blockchain.Instance.CurrentBlockHeight;
            UpdateTurnTable(Blockchain.Instance.GetBlock(currentHeight - currentHeight % Config.Instance.RoundBlock));
        }

        void UpdateTurnTable(Block block)
        {
            // calculate turn table
            List<DelegateState> delegates = Blockchain.Instance.GetDelegateStateAll();
            delegates.Sort((x, y) =>
            {
                var valueX = x.Votes.Sum(p => p.Value).Value;
                var valueY = y.Votes.Sum(p => p.Value).Value;
                if (valueX == valueY)
                {
                    if (x.AddressHash < y.AddressHash)
                        return -1;
                    else
                        return 1;
                }
                else if (valueX < valueY)
                    return -1;
                return 1;
            });

            int delegateRange = Config.Instance.MaxDelegate < delegates.Count ? Config.Instance.MaxDelegate : delegates.Count;
            List<UInt160> addrs = new List<UInt160>();
            for (int i = 0; i < delegateRange; ++i)
                addrs.Add(delegates[i].AddressHash);

            _dpos.TurnTable.SetTable(addrs);
            _dpos.TurnTable.SetUpdateHeight(block.Height);
        }

        bool ValidAccount()
        {
            Logger.Log("---------- Check Account ----------");
            Logger.Log("address : " + _account.Address + " length : " + _account.Address.Length);
            Logger.Log("addressHash : " + _account.AddressHash + " byte size : " + _account.AddressHash.Size);
            Logger.Log("pubkey : " + _account.Key.PublicKey.ToByteArray().ToHexString());
            var hashToAddr = WalletAccount.ToAddress(_account.AddressHash);
            bool validHashToAddr = hashToAddr == _account.Address;
            Logger.Log("- check addressHash to address : " + validHashToAddr);
            if (!validHashToAddr)
                return false;
            bool validAddress = WalletAccount.IsAddress(_account.Address);
            Logger.Log("check is address : " + validAddress);
            if (!validAddress)
                return false;
            var generate = new ECKey(ECKey.Generate());
            Logger.Log("generate prikey : " + generate.PrivateKeyBytes.ToHexString());
            Logger.Log("generate pubkey : " + generate.PublicKey.ToByteArray().ToHexString());
            return true;
        }

        bool ValidBlock()
        {
            BlockHeader heightHeader = Blockchain.Instance.GetHeader(0);
            BlockHeader hashHeader = Blockchain.Instance.GetHeader(heightHeader.Hash);
            if (heightHeader == null || hashHeader == null)
                return false;
            Block heightBlock = Blockchain.Instance.GetBlock(0);
            Block hashBlock = Blockchain.Instance.GetBlock(heightBlock.Hash);
            if (heightBlock == null || hashBlock == null)
                return false;
            return true;
        }

        void StartLocalNode()
        {
            _node = new LocalNode();
            _node.Listen();
        }

        void StartRpcServer()
        {
            if (0 < Config.Instance.Network.RpcPort)
            {
                _rpcServer = new RpcServer(_node);
                _rpcServer.Start(Config.Instance.Network.RpcPort);
            }
        }
    }
}
