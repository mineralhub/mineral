using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sky.Cryptography;
using Sky.Wallets;
using System.Text;

namespace Sky.UnitTests
{
    [TestClass]
    public class UT_Account
    {
        WalletAccount _account;

        [TestInitialize]
        public void TestSetup()
        {
            byte[] pk = Encoding.Default.GetBytes("account");
            _account = new WalletAccount(pk);
        }

        [TestMethod]
        public void AddressLength()
        {
            _account.Address.Length.Should().Be(34);
        }

        [TestMethod]
        public void AddressFormat()
        {
            _account.Address.Base58CheckDecode().Length.Should().Be(21);
        }

        [TestMethod]
        public void HashToAddress()
        {
            WalletAccount.ToAddress(_account.AddressHash).Should().Be(_account.Address);
        }

        [TestMethod]
        public void AddressToHash()
        {
            WalletAccount.ToAddressHash(_account.Address).Should().Be(_account.AddressHash);
        }

        [TestMethod]
        public void ValidAddress()
        {
            WalletAccount.IsAddress(_account.Address).Should().BeTrue();
        }
    }
}
