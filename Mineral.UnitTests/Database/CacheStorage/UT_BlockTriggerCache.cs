using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mineral.Core.Database.LevelDB;
using Mineral.Core2;
using Mineral.Database.CacheStorage;
using Mineral.Database.LevelDB;

namespace Mineral.UnitTests.Database.CacheStorage
{
    [TestClass]
    public class UT_BlockTriggerCache
    {
        private DB _db;
        private Storage _storage;

        [TestInitialize]
        public void TestSetup()
        {
            _db = DB.Open("./output-database", new Options { CreateIfMissing = true });
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
        public void TestBlockTriggerCache()
        {
            uint height = 0;
            BlockTriggerState trigger = _storage.BlockTrigger.GetAndChange(height);
            trigger.Should().NotBeNull();
            _storage.Commit(0);
        }
    }
}
