using System;
using System.Text;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Sky;
using Sky.Cryptography;
using Sky.Core;
using Sky.Wallets;
using Sky.Core.DPos;
using Sky.Network;

namespace Tester
{
    public class MainService
    {
        short _version = 1;
        WalletAccount _user;
        WalletAccount _randomAccount;
        UInt256 _from = new UInt256(new byte[32] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
        UInt256 _to = new UInt256(new byte[32] { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
        int _genesisBlockTimestamp = 1532051710;
        Block _genesisBlock;
        LocalNode _node;
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
            CheckAccount();
            //                Config.GenesisBlock.Delegates.ForEach(p => dpos.TurnTable.Enqueue(p.Address));
            //                StartLocalNode();

            while (true)
            {
                do
                {
                    // delegator?
                    if (!_user.IsDelegate())
                        continue;
                    // my turn?
                    //if (dpos.TurnTable.Front != _user.AddressHash)
                    //    continue;
                    // create time?
                    var time = _dpos.CalcBlockTime(_genesisBlock.Header.Timestamp, Blockchain.Instance.CurrentBlockHeight + 1);
                    if (DateTime.UtcNow.ToTimestamp() < time)
                        continue;
                    // create
                    Block block = CreateBlock();
                    if (Blockchain.Instance.AddBlock(block))
                    {
                        //dpos.TurnTable.Enqueue(dpos.TurnTable.Dequeue);
                    }
                }
                while (false);
                Thread.Sleep(100);
            }
        }

        void Initialize()
        {
            Logger.Log("---------- Initialize ----------");
            _user = new WalletAccount(Sky.Cryptography.Helper.SHA256(Config.User.PrivateKey));
            _randomAccount = new WalletAccount(new ECKey(ECKey.Generate()).PrivateKey.D.ToByteArray());
            _dpos = new DPos();

            Logger.Log("random account private key : " + _randomAccount.Key.PrivateKey.D.ToByteArray().ToHexString());
            Logger.Log("random account public key : " + _randomAccount.AddressHash);

            // create genesis block.
            {
                var inputs = new List<TransactionInput>();
                var outputs = new List<TransactionOutput>();

                Config.GenesisBlock.Accounts.ForEach(
                    p =>
                    {
                        outputs.Add(new TransactionOutput(p.Balance, p.Address));
                    });

                var txs = new List<Transaction>
                {
                    new Transaction(_version, eTransactionType.RewardTransaction, _genesisBlockTimestamp - 1, inputs, outputs, new List<MakerSignature>()),
                };

                Config.GenesisBlock.Delegates.ForEach(
                    p =>
                    {
                        txs.Add(new Transaction(
                            _version,
                            eTransactionType.RegisterDelegateTransaction,
                            _genesisBlockTimestamp - 1,
                            new RegisterDelegateTransaction(
                            new List<TransactionInput>(),
                            new List<TransactionOutput>(),
                            new List<MakerSignature>(),
                            p.Address,
                            Encoding.UTF8.GetBytes(p.Name)))
                        );
                    });

                var merkle = new MerkleTree(txs.Select(p => p.Hash).ToArray());
                var blockHeader = new BlockHeader(0, _version, _genesisBlockTimestamp, merkle.RootHash, UInt256.Zero, new MakerSignature());
                _genesisBlock = new Block(blockHeader, txs);
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
            Blockchain.Instance.PersistCompleted += PersistCompleted;

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

        Block CreateBlock()
        {
            // translate trasnaction block.
            Block previosBlock = Blockchain.Instance.GetBlock(Blockchain.Instance.CurrentBlockHash);
            var txs = new List<Transaction>();
            {
                // block reward
                var txIn = new List<TransactionInput>();
                var txOut = new List<TransactionOutput>
                {
                    new TransactionOutput(Config.BlockReward, _user.AddressHash)
                };
                var txSign = new List<MakerSignature>();
                txs.Add(new Transaction(0, eTransactionType.RewardTransaction, DateTime.UtcNow.ToTimestamp(), txIn, txOut, txSign));
            }
            /*
            {
                // translate
                var txIn = new List<TransactionInput>
                {
                    new TransactionInput(previosBlock.Transactions[0].Hash, 0)
                };
                var txOut = new List<TransactionOutput>
                {
                    new TransactionOutput(Fixed8.One, _randomAccount.AddressHash),
                    new TransactionOutput(Config.BlockReward - Fixed8.One - Config.DefaultFee, _user.AddressHash)
                };
                var txSign = new List<MakerSignature>
                {
                    new MakerSignature(Sky.Cryptography.Helper.Sign(txIn[0].Hash.Data, _user.Key), _user.Key.PublicKey.ToByteArray())
                };
                txs.Add(new Transaction(0, eTransactionType.DataTransaction, DateTime.UtcNow.ToTimestamp(), txIn, txOut, txSign));
            }
            */
            var merkle = new MerkleTree(txs.Select(p => p.Hash).ToArray());
            var blockHeader = new BlockHeader(previosBlock.Height + 1, _version, DateTime.UtcNow.ToTimestamp(), merkle.RootHash, previosBlock.Hash, _user.Key);
            return new Block(blockHeader, txs);
        }

        VoteTransaction CreateVoteTransaction()
        {
            // find genesis block input
            List<TransactionInput> txIn = new List<TransactionInput>();
            Fixed8 value = Fixed8.Zero;
            foreach (var tx in _genesisBlock.Transactions)
            {
                for (int i = 0; i < tx.Outputs.Count; ++i)
                {
                    if (tx.Outputs[i].AddressHash == _user.AddressHash)
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

            var txOut = new List<TransactionOutput> { new TransactionOutput(value - Config.VoteFee, _user.AddressHash) };
            var txSign = new List<MakerSignature> { new MakerSignature(Sky.Cryptography.Helper.Sign(txIn[0].Hash.Data, _user.Key), _user.Key.PublicKey.ToByteArray()) };
            return new VoteTransaction(txIn, txOut, txSign);
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
            UpdateTurnTable(Blockchain.Instance.GetBlock(currentHeight - currentHeight % Config.RoundBlock));
        }

        void UpdateTurnTable(Block block)
        {
            // calculate turn table
            List<DelegatorState> delegates = Blockchain.Instance.GetDelegateStateAll();
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

        void CheckAccount()
        {
            Logger.Log("---------- Check Account ----------");
            Logger.Log("address : " + _user.Address + " length : " + _user.Address.Length);
            Logger.Log("addressHash : " + _user.AddressHash + " byte size : " + _user.AddressHash.Size);
            Logger.Log("pubkey : " + _user.Key.PublicKey.ToByteArray().ToHexString());
            var hashToAddr = WalletAccount.ToAddress(_user.AddressHash);
            Logger.Log("- check addressHash to address : " + (hashToAddr == _user.Address));
            var generate = new ECKey(ECKey.Generate());
            Logger.Log("generate prikey : " + generate.PrivateKey.D.ToByteArray().ToHexString());
            Logger.Log("generate pubkey : " + generate.PublicKey.ToByteArray().ToHexString());
        }

        void StartLocalNode()
        {
            _node = new LocalNode();
            _node.Listen();
        }
    }
}
