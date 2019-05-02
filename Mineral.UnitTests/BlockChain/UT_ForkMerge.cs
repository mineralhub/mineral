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

            public SimChain(WalletAccount acc)
            {
                _acc = acc;
            }

            public bool addBlock(Block block)
            {
                if (header.Count == block.Header.Height + 1)
                {
                    header.Add(block.Header);
                    chain.Add(block.Hash, block);
                    return true;
                }
                else if (header.Count < block.Header.Height + 1)
                {   // 도달해야될 블럭보다 이후의 블럭이 도달
                    return false;
                }
                else if (header.Count > block.Header.Height + 1)
                {   // 도달해야될 블럭보다 이전의 블럭이 도달
                    return false;
                }
                return false;
            }

            public bool isForked(Block block)
            {
                if (header.Count == block.Header.Height + 1)
                {
                    return false;
                }
                else if (header.Count > block.Header.Height + 1)
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
                    Type = TransactionType.Transfer,
                    Timestamp = DateTime.UtcNow.ToTimestamp(),
                    Data = _vote,
                };
                _transaction.Sign(_acc.Key);
                return _transaction;
            }
        }

        WalletAccount _1 = new WalletAccount(Encoding.Default.GetBytes("0"));
        WalletAccount _2 = new WalletAccount(Encoding.Default.GetBytes("1"));

        SimChain simchain1 = null;
        SimChain simchain2 = null;
        SimClient simClient1 = null;
        SimClient simClient2 = null;

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
            simchain1 = new SimChain(_1);
            simchain2 = new SimChain(_2);
            simClient1 = new SimClient(_1);
            simClient2 = new SimClient(_2);


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
            _transaction.Signature.Pubkey.Should().NotBeNull();
            _transaction.Signature.Signature.Should().NotBeNull();
            List<Transaction> trx = new List<Transaction>();
            trx.Add(_transaction);

            var merkle = new MerkleTree(trx.ConvertAll(p => p.Hash).ToArray());
            BlockHeader hdr = new BlockHeader
            {
                PrevHash = rootHash,
                MerkleRoot = merkle.RootHash,
                Version = 0,
                Timestamp = 0,
                Height = 1
            };
            hdr.Sign(_1.Key);
            Block _block1 = new Block(hdr, trx);
            Trace.WriteLine(_block1.ToJson().ToString());
            simchain1.addBlock(_block1);

            hdr = new BlockHeader
            {
                PrevHash = rootHash,
                MerkleRoot = merkle.RootHash,
                Version = 0,
                Timestamp = 0,
                Height = 1
            };
            hdr.Sign(_2.Key);
            simchain2.addBlock(_block1);

            trx.Clear();
            trx.Add(simClient1.transfer(_2, 1));
            merkle = new MerkleTree(trx.ConvertAll(p => p.Hash).ToArray());
            hdr = new BlockHeader
            {
                PrevHash = _block1.Hash,
                MerkleRoot = merkle.RootHash,
                Version = 0,
                Timestamp = 0,
                Height = 1
            };
            hdr.Sign(_1.Key);
            _forkedBlock = new Block(hdr, trx);
        }

        [TestMethod]
        public void CheckForked()
        {
            simchain1.isForked(_forkedBlock).Should().BeTrue();
            simchain2.isForked(_forkedBlock).Should().BeTrue();
        }

        [TestMethod]
        public void FindBranchBlock()
        {
            simchain1.GetBranch(_forkedBlock);
        }
    }
}
