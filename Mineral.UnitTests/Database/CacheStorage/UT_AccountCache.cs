using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mineral.Core.State;
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
        private AccountCache _cache;

        [TestInitialize]
        public void TestSetup()
        {
            _db = DB.Open("./output-database", new Options { CreateIfMissing = true });
            _cache = new AccountCache(_db);
        }

        [TestCleanup]
        public void TestClean()
        {
            _db.Dispose();
            _db = null;
            _cache = null;

            DirectoryInfo di = new DirectoryInfo("./output-database");
            if (di.Exists)
                di.Delete(true);
        }

        [TestMethod]
        public void TestAccountCache()
        {
            Fixed8 balance = Fixed8.Parse("10000");
            byte[] priKey = Encoding.Default.GetBytes("account");
            WalletAccount account = new WalletAccount(priKey);

            AccountState accountState =_cache.GetAndChange(account.AddressHash);
            accountState.Should().NotBeNull();

            accountState.AddBalance(balance);

            _cache.Clean();

            WriteBatch batch = new WriteBatch();
            _cache.Commit(batch);

            _cache = new AccountCache(_db);
            _cache.GetAndChange(account.AddressHash).Balance.Should().Equals(balance);

        }
    }
}
