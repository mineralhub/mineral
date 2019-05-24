using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Stroage.LevelDB;
using Mineral.Core.Config.Arguments;
using Mineral.Utils;

namespace Mineral.Common.Storage
{
    public class LevelDBDataSource : IDBSourceInter<byte[]>
    {
        #region Field
        private static readonly string ENGINE = "ENGINE";

        private string database_name;
        private string parent = "";
        private DB db;
        #endregion


        #region Property
        public string DataBaseName { get { return this.database_name; } set { this.parent = value; } }
        public string DataBasePath { get { return this.parent + @"\" + this.database_name; } }
        public bool IsAlive { get; set; }
        #endregion


        #region Constructor
        public LevelDBDataSource(string parent, string name)
        {
            this.database_name = name;
            this.parent = parent + @"\" + Args.Instance.Storage.Directory;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Init()
        {
            if (IsAlive)
            {
                Options options = Args.Instance.Storage.GetOptionsByDbName(DataBaseName);
                this.db = DB.Open(DataBasePath, options);
                IsAlive = this.db != null ? true : false;

            }
        }

        public void Close()
        {
            this.db.Dispose();
            this.db = null;
            IsAlive = false;
        }

        public void Reset()
        {
            Close();
            FileUtils.RecursiveDelete(DataBasePath);
            Init();
        }

        public HashSet<byte[]> AllKeys()
        {
            if (!IsAlive) return null;

            HashSet<byte[]> result = new HashSet<byte[]>();
            using (Iterator it = this.db.NewIterator(new ReadOptions()))
            {
                for (it.SeekToFirst(); it.Valid(); it.Next())
                {
                    result.Add(it.Key().ToArray());
                }
            }

            return result;
        }

        public HashSet<byte[]> AllValue()
        {
            if (!IsAlive) return null;

            HashSet<byte[]> result = new HashSet<byte[]>();
            using (Iterator it = this.db.NewIterator(new ReadOptions()))
            {
                for (it.SeekToFirst(); it.Valid(); it.Next())
                {
                    result.Add(it.Value().ToArray());
                }
            }

            return result;
        }

        public void DeleteData(byte[] key)
        {
            if (IsAlive)
            {
                this.db.Delete(new WriteOptions(), key);
            }
        }

        public void DeleteData(byte[] key, WriteOptionWrapper options)
        {
            if (IsAlive)
            {
                this.db.Delete(options.Level, key);
            }
        }

        public bool Flush()
        {
            return false;
        }

        public byte[] GetData(byte[] key)
        {
            if (!IsAlive) return null;
            return this.db.Get(new ReadOptions(), key).ToArray();
        }

        public long GetTotal()
        {
            if (IsAlive) return 0;

            long result = 0L;
            using (Iterator it = this.db.NewIterator(new ReadOptions()))
            {
                for (it.SeekToFirst(); it.Valid(); it.Next())
                {
                    result++;
                }
            }

            return result;
        }

        public void PutData(byte[] key, byte[] value)
        {
            if (IsAlive)
            {
                PutData(key, value, new WriteOptionWrapper());
            }
        }

        public void PutData(byte[] key, byte[] value, WriteOptionWrapper options)
        {
            if (IsAlive)
            {
                this.db.Put(options.Level, key, value);
            }
        }

        public void UpdateByBatch(Dictionary<byte[], byte[]> rows)
        {
            if (IsAlive)
            {
                UpdateByBatch(rows, new WriteOptionWrapper());
            }
        }

        public void UpdateByBatch(Dictionary<byte[], byte[]> rows, WriteOptionWrapper options)
        {
            if (!IsAlive) return;

            WriteBatch batch = new WriteBatch();
            foreach (KeyValuePair<byte[], byte[]> row in rows)
            {
                batch.Put(SliceBuilder.Begin().Add(row.Key), SliceBuilder.Begin().Add(row.Value));
            }
            this.db.Write(new WriteOptions(), batch);
        }

        public Iterator GetEnumerator()
        {
            return this.db.NewIterator(new ReadOptions());
        }
        #endregion
    }
}
