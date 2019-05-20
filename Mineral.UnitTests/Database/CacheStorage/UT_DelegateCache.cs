using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mineral.Core.Database.LevelDB;
using Mineral.Core2;
using Mineral.Core2.State;
using Mineral.Core2.Transactions;
using Mineral.Database.LevelDB;
using Mineral.Utils;
using Mineral.Wallets;

namespace Mineral.UnitTests.Database.CacheStorage
{
    [TestClass]
    public class UT_DelegateCache
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
        public void Vote()
        {
            VoteTransaction vote = new VoteTransaction
            {
                From = _from.AddressHash,
                Votes = new Dictionary<UInt160, Fixed8> { { _from.AddressHash, Fixed8.One } }
            };
            vote.CalcFee();

            Transaction tx = new Transaction(TransactionType.Vote, DateTime.UtcNow.ToTimestamp(), vote);
            tx.Sign(_from.Key);
            tx.Verify().Should().BeTrue();

            _storage.Delegate.Vote(vote);
            _storage.Commit(0);

            _storage.Delegate.Get(_to.AddressHash).Votes.ContainsKey(_from.AddressHash).Should().Equals(Fixed8.One);
        }

        [TestMethod]
        public void Downvote()
        {
            Vote();
            _storage.Delegate.Downvote(new Dictionary<UInt160, Fixed8> { { _from.AddressHash, Fixed8.One } });
            _storage.Commit(0);

            _storage.Delegate.Get(_to.AddressHash).Votes.ContainsKey(_from.AddressHash).Should().Equals(Fixed8.Zero);
        }
    }
}
