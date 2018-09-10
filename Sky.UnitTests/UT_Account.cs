using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    }
}
