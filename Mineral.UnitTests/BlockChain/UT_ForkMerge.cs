using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mineral.Core;
using Mineral.Core.Transactions;
using Mineral.Cryptography;
using Mineral.Utils;
using Mineral.Wallets;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace Mineral.UnitTests.BlockChain
{
    [TestClass]
    public class UT_ForkMerge
    {
        class SimChain
        {
            WalletAccount _acc;
            Dictionary<UInt256, Block> chain = new Dictionary<UInt256, Block>();
            List<BlockHeader> header = new List<BlockHeader>();
            List<Transaction> trx = new List<Transaction>();
            uint lastHeight = 0;
            UInt256 lastHash = UInt256.Zero;

            public SimChain(WalletAccount acc)
            {
                _acc = acc;
            }

            public bool addBlock(Block block)
            {
                if (header.Count == block.Header.Height - 1)
                {
                    header.Add(block.Header);
                    lastHash = block.Header.Hash;
                    lastHeight = block.Header.Height;

                    chain.Add(block.Hash, block);
                    return true;
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

            public bool isForked(Block block)
            {
                if (header.Count == block.Header.Height)
                {
                    return false;
                }
                else if (header.Count > block.Header.Height)
                {
                    return true;
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
                    Block block = chain[header[i].Hash];
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
            simchainA = new SimChain(_1);
            simchainB = new SimChain(_2);
            simchainC = new SimChain(_3);
            simchainD = new SimChain(_4);

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
        }

        [TestMethod]
        public void CheckForked()
        {
            simchainA.isForked(_forkedBlock).Should().BeTrue();
            simchainB.isForked(_forkedBlock).Should().BeTrue();
        }

        [TestMethod]
        public void FindBranchBlock()
        {
            simchainA.GetBranch(_forkedBlock);
        }
    }
}
