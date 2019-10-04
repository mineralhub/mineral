using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mineral.Core;
using Mineral.Cryptography;

namespace Mineral.UnitTests.Wallet
{
    using Wallet = Mineral.Core.Wallet;

    [TestClass]
    public class UT_Wallet
    {
        private readonly byte prefix = Wallet.ADDRESS_PREFIX_BYTES;
        private readonly byte[] privatekey = "d86007eabbcbbe43d101bdc6fcded3419ecd01d1f626c50389e419237489dedd".HexToBytes();
        private readonly byte[] publickey = "046a1b45e38eabe198d69e962644b9cb27969185d72afb888a23c2f1238dd288acea5ee4fc1ba1b8b87b3a0c5c702ef3eaff9ca40f3e7c0140d0490ed9bfd168cb".HexToBytes();
        private readonly byte[] prefix_address = "3290f2edcf8b8575ce21079f7f9e5c40d13985ecbe".HexToBytes();
        private readonly string base85_address = "MM7aZ42XMFDvWrhLGZDdv7pDawVfc99zTj";
        private ECKey key = null;

        [TestInitialize]
        public void TestSetup()
        {
            this.key = ECKey.FromPrivateKey(privatekey);
        }

        [TestCleanup]
        public void CleanUp()
        {
        }

        [TestMethod]
        public void AddressToBase58()
        {
            byte[] address = Wallet.PublickKeyToAddress(this.publickey);
            address.Length.Should().Be(21);

            string address_str = Wallet.AddressToBase58(address);
            address_str.Equals(this.base85_address).Should().BeTrue();
        }

        [TestMethod]
        public void Base58ToAddress()
        {
            Wallet.Base58ToAddress(this.base85_address).SequenceEqual(prefix_address).Should().BeTrue();
        }

        [TestMethod]
        public void IsValidAddress()
        {
            byte[] address = Wallet.PublickKeyToAddress(this.publickey);
            address.Length.Should().Be(21);

            Wallet.IsValidAddress(address).Should().BeTrue();
        }
    }
}
