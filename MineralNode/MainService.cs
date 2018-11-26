using System;
using System.Text;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Mineral;
using Mineral.Cryptography;
using Mineral.Core;
using Mineral.Wallets;
using Mineral.Core.DPos;
using Mineral.Network;
using Mineral.Network.Payload;
using Mineral.Network.RPC;
using Mineral.Wallets.KeyStore;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace MineralNode
{
    public class MainService
    {
        short BlockVersion => Config.Instance.BlockVersion;
        int GenesisBlockTimestamp => Config.Instance.GenesisBlock.Timestamp;
        WalletAccount _account;
        Block _genesisBlock;
        LocalNode _node;
        RpcServer _rpcServer;
        DPos _dpos;
        Options option;

        public bool InitOption(string[] args)
        {
            option = new Options(args);
            return option.IsValid();
        }

        public bool InitConfig()
        {
            string path = option.ConfigDir ?? "./config.json";
            return Config.Instance.Initialize(path);
        }

        public bool InitAccount()
        {
            string path;
            byte[] privatekey = null;
            if (!string.IsNullOrEmpty(option.KeyStoreDir))
            {
                if (string.IsNullOrEmpty(option.KeyStorePassword))
                {
                    Logger.Log("Keystore password is empty.");
                    return false;
                }

                path = option.KeyStoreDir.Contains(".keystore") ? option.KeyStoreDir : option.KeyStoreDir + ".keystore";
                if (!File.Exists(path))
                {
                    Logger.Log(string.Format("Not found keystore file : [0]", path));
                    return true;
                }

                JObject json;
                using (var file = File.OpenText(path))
                {
                    string data = file.ReadToEnd();
                    json = JObject.Parse(data);
                }
                KeyStore keystore = new KeyStore();
                keystore = JsonConvert.DeserializeObject<KeyStore>(json.ToString());

                if (!KeyStoreService.DecryptKeyStore(option.KeyStorePassword, keystore, out privatekey))
                {
                    Logger.Log("Fail to decrypt keystore file.");
                    return false;
                }
            }
            else if (!string.IsNullOrEmpty(option.PrivateKey))
            {
                privatekey = option.PrivateKey.HexToBytes();
            }
            else
            {
                Console.WriteLine("Please input to keystore director or privatekey.");
                return false;
            }

            _account = new WalletAccount(privatekey);

            return true;
        }

        public bool InitGenesisBlock()
        {
            _dpos = new DPos();
            {
                List<SupplyTransaction> supplyTxs = new List<SupplyTransaction>();
                Config.Instance.GenesisBlock.Accounts.ForEach(
                    p =>
                    {
                        supplyTxs.Add(new SupplyTransaction
                        {
                            From = p.Address,
                            Supply = p.Balance
                        });
                    });

                var txs = new List<Transaction>();
                foreach (var reward in supplyTxs)
                {
                    var tx = new Transaction(eTransactionType.SupplyTransaction,
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
            Blockchain.SetInstance(new Mineral.Database.LevelDB.LevelDBBlockchain("./output-database", _genesisBlock));
            Blockchain.Instance.SetCacheBlockCapacity(Config.Instance.Block.CacheCapacity);
            Blockchain.Instance.PersistCompleted += PersistCompleted;
            Blockchain.Instance.Run();

            var genesisBlockTx = Blockchain.Instance.Storage.GetTransaction(_genesisBlock.Transactions[0].Hash);
            Logger.Log("genesis block tx. hash : " + genesisBlockTx.Hash);

            WalletIndexer.SetInstance(new Mineral.Database.LevelDB.LevelDBWalletIndexer("./output-wallet-index"));
            UpdateTurnTable();

            return true;
        }

        public bool Initialize(string[] args)
        {
            Logger.Log("---------- MainService Initialize ----------");

            bool result = true;

            if (result) result = InitOption(args);
            if (result) result = InitConfig();
            if (result) result = InitAccount();
            if (result) result = InitGenesisBlock();

            return result;
        }


        public void Run()
        {
            Logger.WriteConsole = true;
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


        Block CreateBlock(int height, UInt256 prevhash, List<Transaction> txs = null)
        {
            if (txs == null)
                txs = new List<Transaction>();

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

        Transaction CreateTransferTransaction(UInt160 target, Fixed8 value)
        {
            var trans = new TransferTransaction
            {
                From = _account.AddressHash,
                To = new Dictionary<UInt160, Fixed8> { { target, value } }
            };
            var tx = new Transaction(eTransactionType.TransferTransaction, DateTime.UtcNow.ToTimestamp(), trans);
            tx.Sign(_account);
            return tx;
        }

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
                        return 1;
                    else
                        return -1;
                }
                else if (valueX < valueY)
                    return 1;
                return -1;
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
