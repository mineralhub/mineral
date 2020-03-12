using FluentAssertions;
using LevelDB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Mineral.UnitTests.Database
{
    [TestClass]
    public class UT_LevelDB
    {
        private DB db = null;
        private readonly string db_name = "UT_LevelDB";
        private readonly byte[] default_key = Encoding.Default.GetBytes("default_mineral");
        private readonly byte[] default_value = Encoding.Default.GetBytes("default_mineral_value");
        private readonly byte[] key = Encoding.Default.GetBytes("mineral");
        private readonly byte[] value = Encoding.Default.GetBytes("mineral_value");
        private WriteOptions write_option = new WriteOptions();
        private readonly ReadOptions read_option = new ReadOptions();

        private readonly CompressionLevel DEFAULT_COMPRESSION_TYPE = CompressionLevel.SnappyCompression;
        private readonly int DEFAULT_BLOCK_SIZE = 4 * 1024;
        private readonly int DEFAULT_WRITE_BUFFER_SIZE = 10 * 1024 * 1024;
        private readonly long DEFAULT_CACHE_SIZE = 10 * 1024 * 1024L;
        private readonly int DEFAULT_MAX_OPEN_FILES = 100;

        [TestInitialize]
        public void TestSetup()
        {
            Options option = new Options()
            {
                CreateIfMissing = true,
                ParanoidChecks = true,

                CompressionLevel = CompressionLevel.SnappyCompression,
                BlockSize = DEFAULT_BLOCK_SIZE,
                WriteBufferSize = DEFAULT_WRITE_BUFFER_SIZE,
                Cache = new LevelDB.Cache((int)DEFAULT_CACHE_SIZE),
                MaxOpenFiles = DEFAULT_MAX_OPEN_FILES
            };

            this.db = new DB(option, this.db_name);
            this.db.Put(this.default_key, this.default_value, this.write_option);
        }

        [TestCleanup]
        public void CleanUp()
        {
            this.db.Dispose();

            DirectoryInfo di = new DirectoryInfo(this.db_name);
            if (di.Exists)
                di.Delete(true);
        }

        [TestMethod]
        public void Put()
        {
            bool result = false;
            try
            {
                this.db.Put(this.key, this.value, this.write_option);
                result = true;
            }
            catch
            {
                result = false;
            }
            result.Should().BeTrue();
        }

        [TestMethod]
        public void Get()
        {
            byte[] result = null;
            try
            {
                result = this.db.Get(this.default_key, this.read_option);
            }
            catch (System.Exception e)
            {
                e.Should().BeNull();
            }

            result.Should().NotBeNull();
            result.SequenceEqual(this.default_value).Should().BeTrue();
        }

        [TestMethod]
        public void Delete()
        {
            try
            {
                this.db.Delete(this.default_key, this.write_option);
            }
            catch (System.Exception e)
            {
                e.Should().BeNull();
            }

            this.db.Get(this.default_key).Should().BeNull();
        }

        [TestMethod]
        public void CreateIterator()
        {
            using (Iterator it = this.db.CreateIterator(this.read_option))
            {
                it.SeekToFirst();
                it.Key().SequenceEqual(this.default_key).Should().BeTrue();
                it.Value().SequenceEqual(this.default_value).Should().BeTrue();
            }
        }

        [TestMethod]
        public void Enumerator()
        {
            IEnumerator<KeyValuePair<byte[], byte[]>> it = this.db.GetEnumerator();

            byte[] result = null;
            while (it.MoveNext())
            {
                result = it.Current.Value;
            }

            result.SequenceEqual(this.default_value).Should().BeTrue();
        }

        [TestMethod]
        public void WriteBatch()
        {
            try
            {
                WriteBatch batch = new WriteBatch();

                batch.Put(key, value);
                this.db.Write(batch, this.write_option);
            }
            catch
            {
            }

            this.db.Get(key, this.read_option).Should().NotBeNull();
        }
    }
}
