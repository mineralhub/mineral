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
        WalletAccount _1 = new WalletAccount(Encoding.Default.GetBytes("0"));
        WalletAccount _2 = new WalletAccount(Encoding.Default.GetBytes("1"));
        Dictionary<UInt256, Block> fork1 = new Dictionary<UInt256, Block>();
        Dictionary<UInt256, Block> fork2 = new Dictionary<UInt256, Block>();
        Block _block1;
        Block _block2;

        private TestContext testContextInstance;
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        [TestInitialize]
        public void TestSetup()
        {
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
            _block1 = new Block(hdr, trx);
            Trace.WriteLine(_block1.ToJson().ToString());
            fork1.Add(_block1.Hash, _block1);

            hdr = new BlockHeader
            {
                PrevHash = rootHash,
                MerkleRoot = merkle.RootHash,
                Version = 0,
                Timestamp = 0,
                Height = 1
            };
            hdr.Sign(_2.Key);
            _block2 = new Block(hdr, trx);
            TestContext.WriteLine(_block2.ToJson().ToString());
            fork2.Add(_block2.Hash, _block2);
        }

        [TestMethod]
        public void CheckForked()
        {
            (fork1[_block1.Hash].Height == fork2[_block2.Hash].Height && _block1.Hash != _block2.Hash).Should().BeTrue();
        }
    }
}
