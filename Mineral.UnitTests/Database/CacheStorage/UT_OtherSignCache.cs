using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mineral.Core.Transactions;
using Mineral.Database.LevelDB;
using Mineral.Utils;
using Mineral.Wallets;

namespace Mineral.UnitTests.Database.CacheStorage
{
    [TestClass]
    public class UT_OtherSignCache
    {
        private DB _db;
        private Storage _storage;

        private byte[] _fromName = Encoding.Default.GetBytes("0");
        private byte[] _toName = Encoding.Default.GetBytes("1");
        private WalletAccount _from;
        private WalletAccount _to;

        [TestInitialize]
        public void TestSetup()
        {
            _db = DB.Open("./output-database", new Options { CreateIfMissing = true });
            _storage = Storage.NewStorage(_db);

            _from = new WalletAccount(_fromName);
            _to = new WalletAccount(_toName);

            _storage.Delegate.Add(_from.AddressHash, _fromName);
            _storage.Delegate.Add(_to.AddressHash, _toName);
            _storage.Commit(0);

            _storage = Storage.NewStorage(_db);
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
        public void TestOtherSign()
        {
            OtherSignTransaction otherSign = new OtherSignTransaction
            {
                From = _from.AddressHash,
                To = new Dictionary<UInt160, Fixed8> { { _to.AddressHash, Fixed8.One } },
                Others = new HashSet<string> { _from.Address, _to.Address },
                ExpirationBlockHeight = 10
            };
            otherSign.CalcFee();

            Transaction tx = new Transaction(TransactionType.OtherSign, DateTime.UtcNow.ToTimestamp(), otherSign);
            tx.Sign(_from.Key);
            tx.Verify().Should().BeTrue();

            _storage.OtherSign.Add(otherSign.Owner.Hash, otherSign.Others);
            _storage.Commit(0);

            _storage = Storage.NewStorage(_db);
            _storage.OtherSign.GetAndChange(otherSign.Owner.Hash).Should().NotBeNull();
        }
    }
}
