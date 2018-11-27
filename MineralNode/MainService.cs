﻿using System;
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
using System.Reflection;
using MineralNode.CommandLine;
using MineralNode.CommandLine.Attributes;

namespace MineralNode
{
    public class MainService
    {
        private short BlockVersion => Config.Instance.BlockVersion;
        private int GenesisBlockTimestamp => Config.Instance.GenesisBlock.Timestamp;
        private WalletAccount _account;
        private Block _genesisBlock;
        private LocalNode _node;
        private RpcServer _rpcServer;
        private DPos _dpos;
        private Options option;

        public bool InitOption(string[] args)
        {
            option = new Options(args);

            if (option.IsHelp || !option.IsValid)
            {
                OnHelpMessage();
                return false;
            }
            return true;
        }

        public bool InitConfig()
        {
            string path = option.Default.ConfigDir ?? "./config.json";
            return Config.Instance.Initialize(path);
        }

        public bool InitAccount()
        {
            string path;
            byte[] privatekey = null;
            if (!string.IsNullOrEmpty(option.Wallet.KeyStoreDir))
            {
                if (string.IsNullOrEmpty(option.Wallet.KeyStorePassword))
                {
                    Logger.Log("Keystore password is empty.");
                    return false;
                }

                path = option.Wallet.KeyStoreDir.Contains(".keystore") ? option.Wallet.KeyStoreDir : option.Wallet.KeyStoreDir + ".keystore";
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

                if (!KeyStoreService.DecryptKeyStore(option.Wallet.KeyStorePassword, keystore, out privatekey))
                {
                    Logger.Log("Fail to decrypt keystore file.");
                    return false;
                }
            }
            else if (!string.IsNullOrEmpty(option.Wallet.PrivateKey))
            {
                privatekey = option.Wallet.PrivateKey.HexToBytes();
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
            Blockchain.Instance.PersistCompleted += PersistCompleted;
            Blockchain.Instance.Run();

            var genesisBlockTx = Blockchain.Instance.storage.GetTransaction(_genesisBlock.Transactions[0].Hash);
            Logger.Log("genesis block tx. hash : " + genesisBlockTx.Hash);

            WalletIndexer.SetInstance(new Mineral.Database.LevelDB.LevelDBWalletIndexer("./output-wallet-index"));
            UpdateTurnTable();

            return true;
        }

        public bool Initialize(string[] args)
        {
            bool result = true;
            Logger.WriteConsole = true;
            Logger.Log("---------- MainService Initialize Start ----------");

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
                Thread.Sleep(100);
            }
        }

        private void CreateAndAddBlocks(int cnt, bool directly)
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


        private Block CreateBlock(int height, UInt256 prevhash, List<Transaction> txs = null)
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

        private Transaction CreateTransferTransaction(UInt160 target, Fixed8 value)
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

        private void PersistCompleted(object sender, Block block)
        {
            int remain = _dpos.TurnTable.RemainUpdate(block.Height);
            if (0 < remain)
                return;

            UpdateTurnTable();
        }

        private void UpdateTurnTable()
        {
            int currentHeight = Blockchain.Instance.CurrentBlockHeight;
            UpdateTurnTable(Blockchain.Instance.GetBlock(currentHeight - currentHeight % Config.Instance.RoundBlock));
        }

        private void UpdateTurnTable(Block block)
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

        private bool ValidBlock()
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

        private void OnHelpMessage()
        {
            AssemblyName assembly = Assembly.GetEntryAssembly().GetName();

            // VERSION
            string message = string.Empty
                + "\n"
                + (assembly.Name + "".PadLeft(2) + assembly.Version.ToString()) 
                ;

            // USAGE
            message += string.Empty
                + "\n"
                + "\n" + "".PadLeft(0) + "USAGE : "
                + "\n" + "".PadLeft(10) + string.Format("Mineral.dll {0} <dir> {1} <password> [options]", OptionName.KeyStoreDir, OptionName.KeyStorePassword)
                + "\n" + "".PadLeft(10) + string.Format("Mineral.dll {0} <key> [options]", OptionName.PrivateKey)
                ;

            // DEFAULT OPTIONS
            message += string.Empty
                + "\n"
                + "\n" + "".PadLeft(1) + "--DEFAULT OPTIONS : ";
            foreach (PropertyInfo info in typeof(OptionDefault).GetProperties())
            {
                DefaultAttribute attr = (DefaultAttribute)info.GetCustomAttribute(typeof(DefaultAttribute));
                if (attr != null)
                {
                    message += "\n" + "".PadLeft(4);
                    message += string.Format("{0,-25} {1}", attr.Name, attr.Description);
                }
            }

            // WALLET OPTIONS
            message += string.Empty
                + "\n"
                + "\n" + "".PadLeft(1) + "--WALLET OPTIONS : ";
            foreach (PropertyInfo info in typeof(OptionWallet).GetProperties())
            {
                WalletAttribute attr = (WalletAttribute)info.GetCustomAttribute(typeof(WalletAttribute));
                if (attr != null)
                {
                    message += "\n" + "".PadLeft(4);
                    message += string.Format("{0,-25} {1}", attr.Name, attr.Description);
                }
            }

            message += string.Empty
                + "\n"
                + "\n" + "".PadLeft(0) + "MISC OPTION :"
                + "\n" + "".PadLeft(4) + "-h   -help"
                ;

            Console.WriteLine(message);
        }
    }
}
