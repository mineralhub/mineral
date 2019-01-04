using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Database.LevelDB;
using FluentAssertions;
using System.Linq;

namespace Mineral.UnitTests.Database
{
    [TestClass]
    public class UT_LevelDB
    {
        private DB db = null;
        private byte prefix = 0x02;
        private byte[] key = Encoding.Default.GetBytes("mineral");
        private byte[] value = Encoding.Default.GetBytes("LevelDB Test");
        private WriteOptions write_option = WriteOptions.Default;
        private ReadOptions read_option = ReadOptions.Default;

        [TestInitialize]
        public void TestSetup()
        {
            this.db = DB.Open("./output-database", new Options { CreateIfMissing = true });
            this.db.Put(write_option, SliceBuilder.Begin(this.prefix).Add(this.key), this.value);
        }

        [TestCleanup]
        public void TestClean()
        {
            this.db.Dispose();
        }

        [TestMethod]
        public void Put()
        {
            bool result = false;
            try
            {
                this.db.Put(write_option, SliceBuilder.Begin(this.prefix).Add(this.key), this.value);
                result = true;
            }
            catch
            {
                result = false;
            }
            result.Should().BeTrue();
        }

        [TestMethod]
        public void TryGet()
        {
            bool result = false;
            try
            {
                Slice slice;
                if (this.db.TryGet(read_option, SliceBuilder.Begin(this.prefix).Add(this.key), out slice))
                {
                    result = this.value.SequenceEqual(slice.ToArray());
                }
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
            bool result = false;
            try
            {
                Slice slice = this.db.Get(read_option, SliceBuilder.Begin(this.prefix).Add(this.key));
                result = this.value.SequenceEqual(slice.ToArray());
            }
            catch
            {
                result = false;
            }
            result.Should().BeTrue();
        }

        [TestMethod]
        public void Delete()
        {
            bool result = false;
            try
            {
                this.db.Delete(this.write_option, SliceBuilder.Begin(prefix).Add(this.key));
                result = true;
            }
            catch
            {
                result = false;
            }
            result.Should().BeTrue();
        }

        [TestMethod]
        public void Find()
        {
            IEnumerable<byte[]> result = this.db.Find<byte[]>(this.read_option, SliceBuilder.Begin(prefix).Add(this.key), (k, v) =>
            {
                return v.ToArray();
            }).ToArray();
            result.Should().NotBeNull();
        }

        [TestMethod]
        public void WriteBatch()
        {
            bool result = false;
            try
            {
                WriteBatch batch = new WriteBatch();
                batch.Put(SliceBuilder.Begin(this.prefix).Add(this.key), this.value);
                this.db.Write(this.write_option, batch);
                result = true;
            }
            catch
            {
                result = false;
            }
            result.Should().BeTrue();
        }
    }
}
