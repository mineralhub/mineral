using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mineral.Common.Storage;
using Mineral.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Mineral.UnitTests.Database
{
    [TestClass]
    public class UT_LevelDBDataSource
    {
        private LevelDBDataSource db = null;

        private string parnet_name = "database";
        private string database_name = "UT_LevelDBDataSource";

        private Dictionary<byte[], byte[]> default_data = new Dictionary<byte[], byte[]>(new ByteArrayEqualComparer())
        {
            { Encoding.Default.GetBytes("key_1"),  Encoding.Default.GetBytes("value_1") },
            { Encoding.Default.GetBytes("key_2"),  Encoding.Default.GetBytes("value_2") },
            { Encoding.Default.GetBytes("key_3"),  Encoding.Default.GetBytes("value_3") },
            { Encoding.Default.GetBytes("key_4"),  Encoding.Default.GetBytes("value_4") },
            { Encoding.Default.GetBytes("key_5"),  Encoding.Default.GetBytes("value_5") },
        };

        private byte[] key = Encoding.Default.GetBytes("mineral");
        private byte[] value = Encoding.Default.GetBytes("mineral_value");

        [TestInitialize]
        public void TestSetup()
        {
            this.db = new LevelDBDataSource(this.parnet_name, this.database_name);
            this.db.Init();

            foreach (var data in this.default_data)
            {
                this.db.PutData(data.Key, data.Value);
            }
        }

        [TestCleanup]
        public void CleanUp()
        {
            this.db.Close();

            DirectoryInfo di = new DirectoryInfo(this.db.DataBasePath);
            if (di.Exists)
                di.Delete(true);
        }

        [TestMethod]
        public void PutData()
        {
            bool result = false;
            try
            {
                this.db.PutData(this.key, this.value);
                result = true;
            }
            catch (System.Exception e)
            {
                e.Should().BeNull();
            }

            result.Should().BeTrue();
        }

        [TestMethod]
        public void GetData()
        {
            byte[] result = null;

            try
            {
                result = this.db.GetData(this.default_data.First().Key);
            }
            catch (System.Exception e)
            {
                e.Should().BeNull();
            }

            result.SequenceEqual(this.default_data.First().Value).Should().BeTrue();
        }

        [TestMethod]
        public void DeleteData()
        {
            try
            {
                this.db.DeleteData(this.default_data.First().Key);
            }
            catch (System.Exception e)
            {
                e.Should().BeNull();
            }

            this.db.GetData(this.default_data.First().Key).Should().BeNull();
        }


        [TestMethod]
        public void AllKeys()
        {
            this.db.AllKeys().Count.Should().Be(this.default_data.Count);
        }

        [TestMethod]
        public void AllValues()
        {
            this.db.AllValue().Count.Should().Be(this.default_data.Count);
        }

        [TestMethod]
        public void GetPrevious()
        {
            int index = this.default_data.Count - 2;
            long limit = 1;
            List<byte[]> keys = new List<byte[]>(this.default_data.Keys);

            var result = this.db.GetPrevious(keys[index], limit);

            result.Should().NotBeNull();
            result.Count.Should().Be((int)limit);

            result.First().Key.SequenceEqual(keys[index - 1]).Should().BeTrue();
        }

        [TestMethod]
        public void GetNext()
        {
            int index = this.default_data.Count - 2;
            long limit = 1;
            List<byte[]> keys = new List<byte[]>(this.default_data.Keys);

            var result = this.db.GetNext(keys[index], limit);

            result.Should().NotBeNull();
            result.Count.Should().Be((int)limit);

            result.First().Key.SequenceEqual(keys[index + 1]).Should().BeTrue();
        }

        [TestMethod]
        public void GetAll()
        {
            this.db.GetAll().Count().Equals(this.default_data.Count).Should().BeTrue();
        }
    }
}
