using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mineral.Core2.State;
using Mineral.Database.CacheStorage;
using Mineral.Database.LevelDB;
using Mineral.Utils;
using Mineral.Wallets;
using System.IO;
using System.Text;

namespace Mineral.UnitTests.Database.CacheStorage
{
    [TestClass]
    public class UT_AccountCache
    {
        private DB _db;
        private Storage _storage;
        byte[] _priKey;
        WalletAccount _account;

        [TestInitialize]
        public void TestSetup()
        {
            _db = DB.Open("./output-database", new Options { CreateIfMissing = true });
            _storage = Storage.NewStorage(_db);

            _priKey = Encoding.Default.GetBytes("account");
            _account = new WalletAccount(_priKey);
        }

        [TestCleanup]
        public void TestClean()
        {
            _storage.Dispose();
            _storage = null;
            _db.Dispose();
            _db = null;

            DirectoryInfo di = new DirectoryInfo("./output-database");
            if (di.Exists)
                di.Delete(true);
        }

        [TestMethod]
        public void TestAccountCache()
        {
            Fixed8 balance = Fixed8.Parse("10000");
            AccountState accountState = _storage.Account.GetAndChange(_account.AddressHash);
            accountState.Should().NotBeNull();

            accountState.AddBalance(balance);
            _storage.Commit(0);

            _storage = Storage.NewStorage(_db);
            accountState = _storage.Account.GetAndChange(_account.AddressHash);
            accountState.Balance.Should().Be(balance);
        }
    }
}
