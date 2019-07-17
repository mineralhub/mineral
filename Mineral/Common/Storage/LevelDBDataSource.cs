using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Stroage.LevelDB;
using Mineral.Core.Config.Arguments;
using Mineral.Utils;

namespace Mineral.Common.Storage
{
    public class LevelDBDataSource : IDBSourceInter<byte[]>, IEnumerable<KeyValuePair<byte[], byte[]>>
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

        public HashSet<byte[]> GetLatestValues(long limit)
        {
            HashSet<byte[]> result = new HashSet<byte[]>();

            if (limit <= 0)
                return result;

            Iterator it = this.db.NewIterator(new ReadOptions());
            // TODO 빠진부분 확인해야함

            long i = 0;
            for (it.SeekToLast(); it.Valid() && i++ < limit; it.Prev())
            {
                result.Add(it.Value().ToArray());
                i++;
            }

            return result;
        }

        public Dictionary<byte[], byte[]> GetNext(byte[] key, long limit)
        {
            Dictionary<byte[], byte[]> result = new Dictionary<byte[], byte[]>();
            if (limit <= 0)
                return result;

            Iterator it = this.db.NewIterator(new ReadOptions());
            long i = 0;
            for (it.Seek(key); it.Valid() && i++ < limit; it.Next())
            {
                result.Add(it.Key().ToArray(), it.Value().ToArray());
            }

            return result;
        }

        public Dictionary<byte[], byte[]> GetPrevious(byte[] key, long limit, int precision)
        {
            Dictionary<byte[], byte[]> result = new Dictionary<byte[], byte[]>();

            if (limit <= 0 || key.Length < precision)
            {
                return result;
            }

            long i = 0;
            Iterator it = this.db.NewIterator(new ReadOptions());
            for (it.SeekToFirst(); it.Valid() && i++ < limit; it.Next())
            {
                if (it.Key().buffer.Length >= precision)
                {
                    if (ByteUtil.Compare(
                            ArrayUtil.GetRange(key, 0, precision),
                            ArrayUtil.GetRange(it.Key().ToArray(), 0, precision))
                            < 0)
                    {
                        break;
                    }
                    result.Add(it.Key().ToArray(), it.Value().ToArray());
                }
            }

            return result;
        }

        public Dictionary<byte[], byte[]> GetAll()
        {
            Dictionary<byte[], byte[]> result = new Dictionary<byte[], byte[]>();

            Iterator it = this.db.NewIterator(new ReadOptions());
            for (it.SeekToFirst(); it.Valid(); it.Next())
            {
                result.Add(it.Key().ToArray(), it.Value().ToArray());
            }

            return result;
        }

        public IEnumerator<KeyValuePair<byte[], byte[]>> GetEnumerator()
        {
            Iterator it = this.db.NewIterator(new ReadOptions());
            for (; it.Valid(); it.Next())
            {
                yield return new KeyValuePair<byte[], byte[]>(it.Key().ToArray(), it.Value().ToArray());
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator<KeyValuePair<byte[], byte[]>>)GetEnumerator();
        }
        #endregion
    }
}
