using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mineral.Wallets.KeyStore;
using Newtonsoft.Json.Linq;

namespace Mineral.UnitTests.Wallet
{
    [TestClass]
    public class UT_Keystore
    {
        private readonly byte[] privatekey = "d86007eabbcbbe43d101bdc6fcded3419ecd01d1f626c50389e419237489dedd".HexToBytes();
        private readonly string base85_address = "MM7aZ42XMFDvWrhLGZDdv7pDawVfc99zTj";
        public readonly string keystore_file = "mineral.keystore";
        public readonly string password = "mineral_password";
        //public readonly byte[] password_encryt = "";

        [TestInitialize]
        public void TestSetup()
        {
            if (!File.Exists(this.keystore_file))
            {
                KeyStoreService.GenerateKeyStore(this.keystore_file,
                                                 this.password,
                                                 this.privatekey,
                                                 this.base85_address);
            }
        }

        [TestCleanup]
        public void CleanUp()
        {
        }

        [TestMethod]
        public void MakeKeyStoreFile()
        {
            if (File.Exists(this.keystore_file))
            {
                File.Delete(this.keystore_file);
            }

            KeyStoreService.GenerateKeyStore(this.keystore_file,
                                             this.password,
                                             this.privatekey,
                                             this.base85_address).Should().BeTrue();
        }

        [TestMethod]
        public void DecryptKeyStore()
        {
            JObject json = null;
            using (var file = File.OpenText(this.keystore_file))
            {
                json = JObject.Parse(file.ReadToEnd());
            }

            KeyStore keystore = KeyStore.FromJson(json.ToString());

            keystore.Should().NotBeNull();
            KeyStoreService.DecryptKeyStore(this.password, keystore, out byte[] privatekey);
            privatekey.Should().NotBeNull();
            privatekey.Length.Should().Be(32);
            privatekey.SequenceEqual(this.privatekey).Should().BeTrue();
        }
    }
}
