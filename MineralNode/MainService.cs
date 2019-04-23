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
using Mineral.Utils;
using Mineral.Database.BlockChain;
using Mineral.Core.Transactions;

namespace MineralNode
{
    public class MainService
    {
        private short BlockVersion => Config.Instance.BlockVersion;
        private uint GenesisBlockTimestamp => Config.Instance.GenesisBlock.Timestamp;
        private WalletAccount _account;
        private Block _genesisBlock;
        private LocalNode _node;
        private RpcServer _rpcServer;
        private DPos _dpos;
        private Options _options;

        public bool InitOption(string[] args)
        {
            _options = new Options(args);

            if (_options.IsHelp || !_options.IsValid)
            {
                _options.ShowHelpMessage();
                return false;
            }
            return true;
        }

        public bool InitConfig()
        {
            Logger.Info("---------- MainService Initialize Start ----------");
            string path = _options.Default.ConfigDir ?? "./config.json";
            return Config.Instance.Initialize(path);
        }

        public bool InitAccount()
        {
            string path;
            byte[] privatekey = null;
            if (!string.IsNullOrEmpty(_options.Wallet.KeyStoreDir))
            {
                if (string.IsNullOrEmpty(_options.Wallet.KeyStorePassword))
                {
                    Logger.Warning("Keystore password is empty.");
                    return false;
                }

                path = _options.Wallet.KeyStoreDir.Contains(".keystore") ? _options.Wallet.KeyStoreDir : _options.Wallet.KeyStoreDir + ".keystore";
                if (!File.Exists(path))
                {
                    Logger.Warning(string.Format("Not found keystore file : [0]", path));
                    return false;
                }

                JObject json;
                using (var file = File.OpenText(path))
                {
                    string data = file.ReadToEnd();
                    json = JObject.Parse(data);
                }
                KeyStore keystore = new KeyStore();
                keystore = JsonConvert.DeserializeObject<KeyStore>(json.ToString());

                if (!KeyStoreService.DecryptKeyStore(_options.Wallet.KeyStorePassword, keystore, out privatekey))
                {
                    Logger.Warning("Fail to decrypt keystore file.");
                    return false;
                }
            }
            else if (!string.IsNullOrEmpty(_options.Wallet.PrivateKey))
            {
                privatekey = _options.Wallet.PrivateKey.HexToBytes();
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
                    var tx = new Transaction(TransactionType.Supply,
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
                        var tx = new Transaction(TransactionType.RegisterDelegate,
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

            Logger.Debug("genesis block. hash : " + _genesisBlock.Hash);

            BlockChain.Instance.Initialize(_genesisBlock);
            BlockChain.Instance.CacheBlockCapacity = Config.Instance.Block.CacheCapacity;
            BlockChain.Instance.PersistCompleted += PersistCompleted;

            var genesisBlockTx = BlockChain.Instance.GetTransaction(_genesisBlock.Transactions[0].Hash);
            Logger.Debug("genesis block tx. hash : " + genesisBlockTx.Transaction.Hash);

            //WalletIndexer.SetInstance(new LevelDBWalletIndexer("./output-wallet-index"));

            return true;
        }

        public bool Initialize(string[] args)
        {
            bool result = true;

            if (result) result = InitOption(args);
            if (result) result = InitConfig();
            if (result) result = InitAccount();
            if (result) result = InitGenesisBlock();
            return result;
        }

        public void Run()
        {
            StartLocalNode();
            StartRpcServer();

            while (true)
            {
                do
                {
                    if (!_account.IsDelegate())
                        break;

                    if (_node.IsSyncing)
                        break;

                    uint numCreate = BlockChain.Instance.Proof.GetCreateBlockCount(
                        _account.AddressHash,
                        BlockChain.Instance.CurrentHeaderHeight);

                    if (numCreate < 1)
                        break;

                    CreateAndAddBlocks(numCreate);
                } while (true);

            }
        }

        // TODO : clean & move
        private void CreateAndAddBlocks(uint cnt)
        {
            List<Block> blocks = new List<Block>();
            uint height = BlockChain.Instance.CurrentHeaderHeight;
            UInt256 prevhash = BlockChain.Instance.CurrentHeaderHash;

            for (uint i = 0; i < cnt; ++i)
            {
                List<Transaction> txs = new List<Transaction>();
                BlockChain.Instance.LoadTransactionPool(ref txs);
                BlockChain.Instance.NormalizeTransactions(ref txs);
                Block block = CreateBlock(height + i, prevhash, txs);
                
                if (!BlockChain.Instance.VerityBlock(block))
                {
                    Logger.Warning("Block [" + block.Height + ":" + block.Hash + "] has unconfirmed transactions.");
                    if (!BlockChain.Instance.VerityBlock(block))
                    {
                        Logger.Warning("Block [" + block.Height + ":" + block.Hash + "] has not verified.");
                    }
                }

                prevhash = block.Hash;

                BlockChain.Instance.AddBlock(block);
                blocks.Add(block);
            }

            _node.BroadCast(Message.CommandName.BroadcastBlocks, BroadcastBlockPayload.Create(blocks));
        }

        private Block CreateBlock(uint height, UInt256 prevhash, List<Transaction> txs = null)
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


        private void PersistCompleted(object sender, Block block)
        {
        }

        private void StartLocalNode()
        {
            _node = new LocalNode();
            _node.Listen();
        }

        private void StartRpcServer()
        {
            if (0 < Config.Instance.Network.RpcPort)
            {
                _rpcServer = new RpcServer(_node);
                _rpcServer.Start(Config.Instance.Network.RpcPort);
            }
        }
    }
}
