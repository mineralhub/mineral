using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptography.ECDSA;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mineral.Cryptography;
using Mineral.Utils;
using Org.BouncyCastle.Crypto;
using Protocol;

namespace Mineral.UnitTests.Wallet
{
    [TestClass]
    public class UT_ECKey
    {
        private readonly byte[] privatekey = "d86007eabbcbbe43d101bdc6fcded3419ecd01d1f626c50389e419237489dedd".HexToBytes();
        private readonly byte[] publickey = "046a1b45e38eabe198d69e962644b9cb27969185d72afb888a23c2f1238dd288acea5ee4fc1ba1b8b87b3a0c5c702ef3eaff9ca40f3e7c0140d0490ed9bfd168cb".HexToBytes();
        private readonly string address = "MM7aZ42XMFDvWrhLGZDdv7pDawVfadQtqW";

        private readonly byte[] signature_message = Encoding.UTF8.GetBytes("signature message");

        [TestInitialize]
        public void TestSetup()
        {
        }

        [TestCleanup]
        public void CleanUp()
        {
        }

        [TestMethod]
        public void GeneratePrivateKey()
        {
            new ECKey().PrivateKey.Length.Should().Be(32);
        }

        [TestMethod]
        public void IsValidPrivateKey()
        {
            ECKey key = ECKey.FromPrivateKey(privatekey);

            this.privatekey.SequenceEqual(key.PrivateKey).Should().BeTrue();
            this.publickey.SequenceEqual(key.PublicKey).Should().BeTrue();
            this.address.Equals(key.Address.ToAddressEncodeBase58()).Should().BeTrue();
        }

        [TestMethod]
        public void IsValidPublicKey()
        {
            ECKey key = ECKey.FromPublicKey(this.publickey);

            this.publickey.SequenceEqual(key.PublicKey).Should().BeTrue();
            this.address.Equals(key.Address.ToAddressEncodeBase58()).Should().BeTrue();
        }
        
        [TestMethod]
        public void Signature()
        {
            ECKey key = ECKey.FromPrivateKey(this.privatekey);

            ECDSASignature signature = key.Sign(signature_message);
            signature.Should().NotBeNull();

            key.Verify(signature_message, signature).Should().BeTrue();
        }

        [TestMethod]
        public void RecoverySignature()
        {
            ECKey key = ECKey.FromPrivateKey(this.privatekey);

            ECDSASignature signature = key.Sign(signature_message);
            signature.Should().NotBeNull();

            key.Verify(signature_message, signature).Should().BeTrue();
            ECKey.RecoverFromSignature(signature, signature_message, false).PublicKey.SequenceEqual(key.PublicKey).Should().BeTrue();
        }
    }
}
