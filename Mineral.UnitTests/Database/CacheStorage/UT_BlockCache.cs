using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mineral.Core.Database.LevelDB;
using Mineral.Core2;
using Mineral.Core2.State;
using Mineral.Cryptography;
using Mineral.Database.CacheStorage;
using Mineral.Database.LevelDB;
using Mineral.Utils;

namespace Mineral.UnitTests.Database.CacheStorage
{
    [TestClass]
    public class UT_BlockCache
    {
        private DB _db;
        private Storage _storage;

        private BlockState _blockState;

        [TestInitialize]
        public void TestSetup()
        {
            _db = DB.Open("./output-database", new Options { CreateIfMissing = true });
            _storage = Storage.NewStorage(_db);

            BlockHeader header = new BlockHeader
            {
                PrevHash = UInt256.Zero,
                MerkleRoot = UInt256.Zero,
                Version = 0,
                Timestamp = DateTime.UtcNow.ToTimestamp(),
                Height = 0,
                Signature = new MakerSignature()
            };

            Block block = new Block(header, new List<Core2.Transactions.Transaction>());
            _blockState = new BlockState(block);
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
        public void TestBlockCache()
        {
            Block block = _blockState.GetBlock(p => _storage.Transaction.Get(p));
            _storage.Block.Add(_blockState.Header.Hash, _blockState.GetBlock(p => _storage.Transaction.Get(p)));

            _storage.Commit(0);

            _storage = Storage.NewStorage(_db);
            _storage.Block.Get(_blockState.Header.Hash).Should().NotBeNull();
        }
    }
}
