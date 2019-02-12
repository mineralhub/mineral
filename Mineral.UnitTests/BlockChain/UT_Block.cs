using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mineral.Core;
using Mineral.Core.Transactions;
using Mineral.Utils;
using Mineral.Wallets;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mineral.UnitTests.BlcokChain
{
    [TestClass]
    public class UT_Block
    {
        WalletAccount _account;
        BlockHeader _header;
        Block _block;

        [TestInitialize]
        public void TestSetup()
        {
            _account = new WalletAccount(Encoding.Default.GetBytes("0"));
            _header = new BlockHeader
            {
                PrevHash = UInt256.Zero,
                MerkleRoot = UInt256.Zero,
                Version = 0,
                Timestamp = 0,
                Height = 0
            };
            _header.Sign(_account.Key);
            _block = new Block(_header, new List<Transaction>());
        }

        [TestMethod]
        public void Sign()
        {
            _block.Header.VerifySignature().Should().BeTrue();
        }

        [TestMethod]
        public void GetSize()
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                // 32 + 32 + 4 + 4 + 4 
                _header.SerializeUnsigned(bw);
                ms.Flush();
                ms.ToArray().Length.Should().Be(76);
            }

            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                // header + 4(transaction 0)
                _block.SerializeUnsigned(bw);
                ms.Flush();
                ms.ToArray().Length.Should().Be(80);
            }
        }
    }
}
