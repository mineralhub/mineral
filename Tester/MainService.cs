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
using Sky.Network.RPC;

namespace Tester
{
    public class MainService
    {
        short BlockVersion => Config.BlockVersion;
        int GenesisBlockTimestamp => Config.GenesisBlock.Timestamp;
        WalletAccount _account;
        WalletAccount _fromAccount;
        Block _genesisBlock;
        LocalNode _node;
        RpcServer _rpcServer;
        DPos _dpos;

        public void Run()
        {
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
            Logger.WriteConsole = true;
            Config.Initialize();

            Initialize();
            if (ValidAccount() == false)
                return;
            if (ValidBlock() == false)
                return;

            StartLocalNode();
            StartRpcServer();
            // Config.GenesisBlock.Delegates.ForEach(p => dpos.TurnTable.Enqueue(p.Address));

            while (true)
            {
                do
                {
                    // delegator?
                    if (!_account.IsDelegate())
                        break;
                    // my turn?
                    if (_account.AddressHash != _dpos.TurnTable.GetTurn(Blockchain.Instance.CurrentBlockHeight + 1))
                        break;
                    // create time?
                    var time = _dpos.CalcBlockTime(_genesisBlock.Header.Timestamp, Blockchain.Instance.CurrentBlockHeight + 1);
                    if (DateTime.UtcNow.ToTimestamp() < time)
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
                Block block = CreateBlock(height + i, prevhash, txs);
                blocks.Add(block);
                prevhash = block.Hash;
            }
            if (directly)
            {
                foreach (Block block in blocks)
                    Blockchain.Instance.AddBlock(block);
            }
            else
            {
                foreach (Block block in blocks)
                    Blockchain.Instance.AddBlockDirectly(block);
            }

        }

        void Initialize()
        {
            Logger.Log("---------- Initialize ----------");
            _account = new WalletAccount(Sky.Cryptography.Helper.SHA256(Config.User.PrivateKey));
            _fromAccount = new WalletAccount(Sky.Cryptography.Helper.SHA256(Encoding.Default.GetBytes("256")));
            _dpos = new DPos();

            // create genesis block.
            {
                List<TransferTransaction> transTxs = new List<TransferTransaction>();
                Config.GenesisBlock.Accounts.ForEach(
                    p =>
                    {
                        transTxs.Add(new TransferTransaction
                        {
                            From = UInt160.Zero,
                            To = new Dictionary<UInt160, Fixed8> { { p.Address, p.Balance } }
                        });
                    });

                var txs = new List<Transaction>();
                foreach (var trans in transTxs)
                {
                    var tx = new Transaction(eTransactionType.TransferTransaction,
                                    GenesisBlockTimestamp - 1,
                                    trans);
                    tx.Signature = new MakerSignature();
                    txs.Add(tx);
                }

                Config.GenesisBlock.Delegates.ForEach(
                    p =>
                    {
                        var register = new RegisterDelegateTransaction
                        {
                            Name = Encoding.UTF8.GetBytes(p.Name),
                            From = p.Address
                        };
                        var tx = new Transaction(eTransactionType.RegisterDelegateTransaction,
                                                 GenesisBlockTimestamp - 1,
                                                 register);
                        tx.Signature = new MakerSignature();
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

            var genesisBlockTx = Blockchain.Instance.GetTransaction(_genesisBlock.Transactions[0].Hash);
            Logger.Log("genesis block tx. hash : " + genesisBlockTx.Hash);

            WalletIndexer.SetInstance(new Sky.Database.LevelDB.LevelDBWalletIndexer("./output-wallet-index"));
            UpdateTurnTable();
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
                Reward = Config.BlockReward
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

            var txOut = new List<TransactionOutput> { new TransactionOutput(value - Config.VoteFee, _account.AddressHash) };
            var txSign = new List<MakerSignature> { new MakerSignature(Sky.Cryptography.Helper.Sign(txIn[0].Hash.Data, _account.Key), _account.Key.PublicKey.ToByteArray()) };
            return new VoteTransaction(txIn, txOut, txSign);
        }
        */

        void PersistCompleted(object sender, Block block)
        {
            int remain = _dpos.TurnTable.RemainUpdate(block.Height);
            if (0 < remain)
                return;

            UpdateTurnTable();
        }

        void UpdateTurnTable()
        {
            int currentHeight = Blockchain.Instance.CurrentBlockHeight;
            UpdateTurnTable(Blockchain.Instance.GetBlock(currentHeight - currentHeight % Config.RoundBlock));
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

            int delegateRange = Config.MaxDelegate < delegates.Count ? Config.MaxDelegate : delegates.Count;
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
            Logger.Log("generate prikey : " + generate.PrivateKey.D.ToByteArray().ToHexString());
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
            if (0 < Config.Network.RpcPort)
            {
                _rpcServer = new RpcServer(_node);
                _rpcServer.Start(Config.Network.RpcPort);                
            }
        }
    }
}
