using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Database.LevelDB;
using FluentAssertions;
using System.Linq;
using Mineral.Core.Database.LevelDB;

namespace Mineral.UnitTests.Database
{
    [TestClass]
    public class UT_LevelDB
    {
        private DB _db = null;
        private byte _prefix = 0x02;
        private byte[] _key = Encoding.Default.GetBytes("mineral");
        private byte[] _value = Encoding.Default.GetBytes("LevelDB Test");
        private WriteOptions _write_option = WriteOptions.Default;
        private ReadOptions _read_option = ReadOptions.Default;

        [TestInitialize]
        public void TestSetup()
        {
            _db = DB.Open("./output-database", new Options { CreateIfMissing = true });
            _db.Put(_write_option, SliceBuilder.Begin(_prefix).Add(_key), _value);
        }

        [TestCleanup]
        public void TestClean()
        {
            _db.Dispose();
        }

        [TestMethod]
        public void Put()
        {
            bool result = false;
            try
            {
                _db.Put(_write_option, SliceBuilder.Begin(_prefix).Add(_key), _value);
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
                if (_db.TryGet(_read_option, SliceBuilder.Begin(_prefix).Add(_key), out slice))
                {
                    result = _value.SequenceEqual(slice.ToArray());
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
                Slice slice = _db.Get(_read_option, SliceBuilder.Begin(_prefix).Add(_key));
                result = _value.SequenceEqual(slice.ToArray());
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
                _db.Delete(_write_option, SliceBuilder.Begin(_prefix).Add(_key));
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
            IEnumerable<byte[]> result = _db.Find<byte[]>(_read_option, SliceBuilder.Begin(_prefix).Add(_key), (k, v) =>
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
                batch.Put(SliceBuilder.Begin(_prefix).Add(_key), _value);
                _db.Write(_write_option, batch);
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
