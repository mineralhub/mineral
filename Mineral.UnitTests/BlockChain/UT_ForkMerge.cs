using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mineral.Core2;
using Mineral.Core2.Transactions;
using Mineral.Cryptography;
using Mineral.Utils;
using Mineral.Wallets;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Mineral.Database.LevelDB;
using Newtonsoft.Json.Linq;

namespace Mineral.UnitTests.BlockChain
{
    [TestClass]
    public class UT_ForkMerge
    {
        class SimChain
        {
            WalletAccount _acc;
            Dictionary<UInt256, Block> blockCache = new Dictionary<UInt256, Block>();
            Dictionary<UInt256, Block> persistCache = new Dictionary<UInt256, Block>();
            List<BlockHeader> header = new List<BlockHeader>();
            List<Transaction> trx = new List<Transaction>();
            List<UInt160> delegates = new List<UInt160>();
            uint lastHeight = 0;
            UInt256 lastHash = UInt256.Zero;

            public SimChain(WalletAccount acc)
            {
                _acc = acc;
            }

            public void addDelegate(UInt160 addr)
            {
                delegates.Add(addr);
            }

            public bool addBlock(Block block)
            {
                if (header.Count == block.Header.Height - 1)
                {
                    if (blockCache.TryAdd(block.Hash, block))
                    {
                        header.Add(block.Header);
                        lastHash = block.Header.Hash;
                        lastHeight = block.Header.Height;

                        BlockSignTransaction blockSignTransaction = new BlockSignTransaction()
                        {
                            Header = block.Header
                        };
                        Transaction tx = new Transaction()
                        {
                            Version = 0,
                            Type = TransactionType.BlockSign,
                            Data = blockSignTransaction
                        };
                        tx.Sign(_acc);

                        AddTransaction(tx);
                        return true;
                    }
                    // 이미 등록되어 있음
                    return false;
                }
                else if (header.Count < block.Header.Height - 1)
                {   // 도달해야될 블럭보다 이후의 블럭이 도달
                    return false;
                }
                else if (header.Count > block.Header.Height - 1)
                {   // 도달해야될 블럭보다 이전의 블럭이 도달
                    return false;
                }
                return false;
            }

            public void persist()
            {

            }

            public bool CanForked()
            {
                for (int i = 0; i < lastHeight; i++)
                {
                    Block b = blockCache[header[i].Hash];
                    if (b == null) return true;
                    UInt160 delegateAddr = WalletAccount.ToAddressHash(b.Header.Signature.Pubkey);
                    UInt160 ordered = delegates[i % delegates.Count];

                    if (ordered != delegateAddr)
                    {
                        Debug.WriteLine(delegateAddr);
                        Debug.WriteLine(ordered);
                        return true;
                    }
                    // pub key compare with delegate's
                }
                return false;
            }

            public List<BlockHeader> GetBranch(Block block)
            {
                List<BlockHeader> hdrs = new List<BlockHeader>();
                return hdrs;
            }

            public List<Block> GetBlocks(int from, int to)
            {
                List<Block> blocks = new List<Block>();
                for (int i = from; i < to && i < header.Count; i++)
                {
                    Block block = blockCache[header[i].Hash];
                    blocks.Add(block);
                }
                return blocks; ;
            }

            public void AddTransaction(Transaction tx)
            {
                trx.Add(tx);
            }

            public Block CreateBlock()
            {
                var merkle = new MerkleTree(trx.ConvertAll(p => p.Hash).ToArray());
                BlockHeader hdr = new BlockHeader
                {
                    PrevHash = lastHash,
                    MerkleRoot = merkle.RootHash,
                    Version = 0,
                    Timestamp = DateTime.UtcNow.ToTimestamp(),
                    Height = lastHeight + 1
                };
                hdr.Sign(_acc.Key);
                Block block = new Block(hdr, trx);
                trx.Clear();
                return block;
            }
        }

        class SimClient
        {
            WalletAccount _acc;
            public SimClient(WalletAccount acc)
            {
                _acc = acc;
            }

            public Transaction transfer(WalletAccount _to, long balance)
            {
                TransferTransaction _transfer;
                Transaction _transaction;
                _transfer = new TransferTransaction
                {
                    From = _acc.AddressHash,
                    To = new Dictionary<UInt160, Fixed8> { { _to.AddressHash, new Fixed8(balance) } }
                };
                _transfer.CalcFee();

                _transaction = new Transaction
                {
                    Version = 0,
                    Type = TransactionType.Transfer,
                    Timestamp = DateTime.UtcNow.ToTimestamp(),
                    Data = _transfer,
                };
                _transaction.Sign(_acc.Key);
                return _transaction;
            }

            public Transaction vote(WalletAccount _to, long data)
            {
                VoteTransaction _vote;
                Transaction _transaction;
                _vote = new VoteTransaction
                {
                    From = _acc.AddressHash,
                    Votes = new Dictionary<UInt160, Fixed8> { { _to.AddressHash, new Fixed8(data) } }
                };
                _vote.CalcFee();

                _transaction = new Transaction
                {
                    Version = 0,
                    Type = TransactionType.Vote,
                    Timestamp = DateTime.UtcNow.ToTimestamp(),
                    Data = _vote,
                };
                _transaction.Sign(_acc.Key);
                return _transaction;
            }
        }

        WalletAccount _1 = new WalletAccount(Encoding.Default.GetBytes("0"));
        WalletAccount _2 = new WalletAccount(Encoding.Default.GetBytes("1"));
        WalletAccount _3 = new WalletAccount(Encoding.Default.GetBytes("2"));
        WalletAccount _4 = new WalletAccount(Encoding.Default.GetBytes("3"));

        SimChain simchainA = null;
        SimChain simchainB = null;
        SimChain simchainC = null;
        SimChain simchainD = null;

        SimClient simClient1 = null;
        SimClient simClient2 = null;
        SimClient simClient3 = null;
        SimClient simClient4 = null;

        Block _forkedBlock = null;

        private TestContext testContextInstance;
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        [TestInitialize]
        public void TestSetup()
        {
            Debug.WriteLine(_1.Address);
            Debug.WriteLine(_2.Address);
            Debug.WriteLine(_3.Address);
            Debug.WriteLine(_4.Address);

            simchainA = new SimChain(_1);
            simchainB = new SimChain(_2);
            simchainC = new SimChain(_3);
            simchainD = new SimChain(_4);

            simchainA.addDelegate(_1.AddressHash);
            simchainA.addDelegate(_2.AddressHash);
            simchainA.addDelegate(_3.AddressHash);
            simchainA.addDelegate(_4.AddressHash);

            simchainB.addDelegate(_1.AddressHash);
            simchainB.addDelegate(_2.AddressHash);
            simchainB.addDelegate(_3.AddressHash);
            simchainB.addDelegate(_4.AddressHash);

            simchainC.addDelegate(_1.AddressHash);
            simchainC.addDelegate(_2.AddressHash);
            simchainC.addDelegate(_3.AddressHash);
            simchainC.addDelegate(_4.AddressHash);

            simchainD.addDelegate(_1.AddressHash);
            simchainD.addDelegate(_2.AddressHash);
            simchainD.addDelegate(_3.AddressHash);
            simchainD.addDelegate(_4.AddressHash);

            simClient1 = new SimClient(_1);
            simClient2 = new SimClient(_2);
            simClient3 = new SimClient(_3);
            simClient4 = new SimClient(_4);

            TransferTransaction _transfer;
            Transaction _transaction;
            UInt256 rootHash = UInt256.Zero;

            _transfer = new TransferTransaction
            {
                From = _1.AddressHash,
                To = new Dictionary<UInt160, Fixed8> { { _2.AddressHash, Fixed8.One } }
            };
            _transfer.CalcFee();

            _transaction = new Transaction
            {
                Version = 0,
                Type = TransactionType.Transfer,
                Timestamp = DateTime.UtcNow.ToTimestamp(),
                Data = _transfer,
            };
            _transaction.Sign(_1.Key);

            Block _block = simchainA.CreateBlock();
            Trace.WriteLine(_block.ToJson().ToString());

            simchainA.addBlock(_block);
            simchainB.addBlock(_block);
            simchainC.addBlock(_block);
            simchainD.addBlock(_block);

            _block = simchainB.CreateBlock();

            simchainA.addBlock(_block);
            simchainB.addBlock(_block);
            simchainC.addBlock(_block);
            simchainD.addBlock(_block);

            _block = simchainC.CreateBlock();

            simchainA.addBlock(_block);
            simchainB.addBlock(_block);
            simchainC.addBlock(_block);
            simchainD.addBlock(_block);

            _block = simchainD.CreateBlock();

            simchainA.addBlock(_block);
            simchainB.addBlock(_block);
            simchainC.addBlock(_block);
            simchainD.addBlock(_block);


            _block = simchainA.CreateBlock();
            _forkedBlock = simchainB.CreateBlock();

            simchainA.addBlock(_block);
            simchainC.addBlock(_block);

            simchainB.addBlock(_forkedBlock);
            simchainD.addBlock(_forkedBlock);
        }

        [TestMethod]
        public void CheckNotForked()
        {
            simchainA.CanForked().Should().BeFalse();
            simchainC.CanForked().Should().BeFalse();
        }

        [TestMethod]
        public void CheckForked()
        {
            simchainB.CanForked().Should().BeTrue();
            simchainD.CanForked().Should().BeTrue();
        }

        [TestMethod]
        public void FindBranchBlock()
        {
            simchainA.GetBranch(_forkedBlock);
        }
    }
}
