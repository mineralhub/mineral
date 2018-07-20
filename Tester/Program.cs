using System;
using System.Linq;
using System.Threading;
using Sky;
using Sky.Cryptography;
using Sky.Core;
using Sky.Wallets;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Sky.Core.DPos;
using Sky.Network;

namespace test
{
    class Program
    {
        static int _version = 1;
        static WalletAccount _delegator;
        static WalletAccount _randomAccount;
        static UInt256 _from = new UInt256(new byte[32] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
        static UInt256 _to = new UInt256(new byte[32] { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
        static int _genesisBlockTimestamp = 1532051710;
        static Block _genesisBlock;
        static LocalNode _node;

        static void Main(string[] args)
        {
            try
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
                CheckAccount();
                DPos dpos = new DPos();
                dpos.TurnTable.Enqueue(_delegator.Key);
                StartLocalNode();

                while (true)
                {
                    do
                    {
                        // delegator?
                        if (!_delegator.IsDelegate())
                            continue;
                        // my turn?
                        var turn = dpos.TurnTable.Front;
                        if (!turn.PublicKey.Equals(_delegator.Key.PublicKey))
                            continue;
                        // create time?
                        var time = dpos.CalcBlockTime(_genesisBlock.Header.Timestamp, Blockchain.Instance.CurrentBlockHeight + 1);
                        if (DateTime.UtcNow.ToTimestamp() < time)
                            continue;
                        // create
                        Block block = CreateTranslateBlock();
                        if (Blockchain.Instance.AddBlock(block))
                        {
                            
                        }
                    }
                    while (false);
                    Thread.Sleep(100);
                }
            }
            catch (Exception e)
            {
                Logger.Log(e.Message + '\n' + e.StackTrace + '\n' + e.Source);
            }
        }

        static void Initialize()
        {
            Logger.Log("---------- Initialize ----------");
            _delegator = new WalletAccount(Sky.Cryptography.Helper.SHA256(Encoding.ASCII.GetBytes("1")));
            _randomAccount = new WalletAccount(new ECKey(ECKey.Generate()).PrivateKey.D.ToByteArray());
            Logger.Log("random account private key : " + _randomAccount.Key.PrivateKey.D.ToByteArray().ToHexString());
            Logger.Log("random account public key : " + _randomAccount.AddressHash);

            // create genesis block.
            {
                var inputs = new List<TransactionInput>();
                var outputs = new List<TransactionOutput>
                {
                    new TransactionOutput(Fixed8.One * 100000, _delegator.AddressHash)
                };
                var txs = new List<Transaction>
                {
                    new Transaction(0, eTransactionType.RewardTransaction, _genesisBlockTimestamp - 1, inputs, outputs, new List<MakerSignature>()),
                    new RegisterDelegateTransaction(0, eTransactionType.RegisterDelegateTransaction, _genesisBlockTimestamp - 1, inputs, outputs, new List<MakerSignature>(), _delegator.AddressHash, Encoding.UTF8.GetBytes("GenesisDelegate"))
                };
                var merkle = new MerkleTree(txs.Select(p => p.Hash).ToArray());
                var blockHeader = new BlockHeader(0, _version, _genesisBlockTimestamp, merkle.RootHash, UInt256.Zero, _delegator.Key);
                _genesisBlock = new Block(blockHeader, txs);

                byte[] sign = Sky.Cryptography.Helper.Sign(blockHeader.Hash.Data, _delegator.Key);
                Logger.Log("hash : " + blockHeader.Hash);
                Logger.Log("sign : " + sign.ToHexString());
                bool verify = Sky.Cryptography.Helper.VerifySignature(_genesisBlock.Header.Signature, blockHeader.Hash.Data);
                Logger.Log("VerifySignature : " + verify);
                Logger.Log("----------------");
            }

            {
                byte[] bytes = null;
                using (MemoryStream ms = new MemoryStream())
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    _genesisBlock.Serialize(bw);
                    ms.Flush();
                    bytes = ms.ToArray();
                }

                using (MemoryStream ms = new MemoryStream(bytes, false))
                using (BinaryReader br = new BinaryReader(ms))
                {
                    Block serializeBlock = new Block(bytes, 0);
                    Logger.Log("- serialize block test -");
                    Logger.Log("compare block hash : " + (serializeBlock.Hash == _genesisBlock.Hash));
                }
            }

            Logger.Log("genesis block. hash : " + _genesisBlock.Hash);
            Blockchain.SetInstance(new Sky.Database.LevelDB.LevelDBBlockchain("./output-database", _genesisBlock));

            var genesisBlockTx = Blockchain.Instance.GetTransaction(_genesisBlock.Transactions[0].Hash);
            Logger.Log("genesis block tx. hash : " + genesisBlockTx.Hash);

            WalletIndexer.SetInstance(new Sky.Database.LevelDB.LevelDBWalletIndexer("./output-wallet-index"));
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
            {
                // register delegate
                var inputs = new List<TransactionInput>
                {
                    new TransactionInput(_genesisBlock.Transactions[0].Hash, 0)
                };
                var outputs = new List<TransactionOutput>
                {
                    new TransactionOutput(_delegator.GetBalance() - Config.RegisterDelegateFee, _delegator.AddressHash)
                };
                var txs = new List<Transaction>
                {
                    new Transaction(0, eTransactionType.RegisterDelegateTransaction, DateTime.UtcNow.ToTimestamp(), inputs, outputs, new List<MakerSignature>())
                };
                var merkle = new MerkleTree(txs.Select(p => p.Hash).ToArray());
                var blockHeader = new BlockHeader(0, _version, DateTime.UtcNow.ToTimestamp(), merkle.RootHash, UInt256.Zero, _delegator.Key);
                Blockchain.Instance.AddBlock(new Block(blockHeader, txs));
            }
        }

        static Block CreateTranslateBlock()
        {
            // translate trasnaction block.
            Block previosBlock = Blockchain.Instance.GetBlock(Blockchain.Instance.CurrentBlockHash);
            var txs = new List<Transaction>();
            {
                // block reward
                var txIn = new List<TransactionInput>();
                var txOut = new List<TransactionOutput>
                {
                    new TransactionOutput(Config.BlockReward, _delegator.AddressHash)
                };
                var txSign = new List<MakerSignature>();
                txs.Add(new Transaction(0, eTransactionType.RewardTransaction, DateTime.UtcNow.ToTimestamp(), txIn, txOut, txSign));
            }
            {
                // translate
                var txIn = new List<TransactionInput>
                {
                    new TransactionInput(previosBlock.Transactions[0].Hash, 0)
                };
                var txOut = new List<TransactionOutput>
                {
                    new TransactionOutput(Fixed8.One, _randomAccount.AddressHash),
                    new TransactionOutput(Config.BlockReward - Fixed8.One - Config.DefaultFee, _delegator.AddressHash)
                };
                var txSign = new List<MakerSignature>
                {
                    new MakerSignature(Sky.Cryptography.Helper.Sign(txIn[0].Hash.Data, _delegator.Key), _delegator.Key.PublicKey.ToByteArray())
                };
                txs.Add(new Transaction(0, eTransactionType.DataTransaction, DateTime.UtcNow.ToTimestamp(), txIn, txOut, txSign));
            }

            var merkle = new MerkleTree(txs.Select(p => p.Hash).ToArray());
            var blockHeader = new BlockHeader(previosBlock.Height + 1, _version, DateTime.UtcNow.ToTimestamp(), merkle.RootHash, previosBlock.Hash, _delegator.Key);
            return new Block(blockHeader, txs);
        }

        private static void CheckAccount()
        {
            Logger.Log("---------- Check Account ----------");
            var account = _delegator;
            Logger.Log("address : " + account.Address + " length : " + account.Address.Length);
            Logger.Log("addressHash : " + account.AddressHash + " byte size : " + account.AddressHash.Size);
            Logger.Log("pubkey : " + account.Key.PublicKey.ToByteArray().ToHexString());
            var hashToAddr = WalletAccount.ToAddress(account.AddressHash);
            Logger.Log("- check addressHash to address : " + (hashToAddr == account.Address));
            var generate = new ECKey(ECKey.Generate());
            Logger.Log("generate prikey : " + generate.PrivateKey.D.ToByteArray().ToHexString());
            Logger.Log("generate pubkey : " + generate.PublicKey.ToByteArray().ToHexString());
        }

        private static void StartLocalNode()
        {
            _node = new LocalNode();
            _node.Listen();
        }
    }
}
