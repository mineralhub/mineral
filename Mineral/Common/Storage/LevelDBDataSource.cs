using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LevelDB;
using Mineral.Core.Config.Arguments;
using Mineral.Utils;

namespace Mineral.Common.Storage
{
    public class LevelDBDataSource : IDBSourceInter<byte[]>, IEnumerable<KeyValuePair<byte[], byte[]>>
    {
        #region Field
        private string database_name;
        private string parent = "";
        private DB db;
        #endregion


        #region Property
        public string DataBaseName { get { return this.database_name; } set { this.parent = value; } }
        public string DataBasePath { get { return this.parent + Path.DirectorySeparatorChar + this.database_name; } }
        public bool IsAlive { get; set; } = false;
        #endregion


        #region Constructor
        public LevelDBDataSource(string parent, string name)
        {
            this.database_name = name;
            this.parent = parent.IsNullOrEmpty() ?
                Args.Instance.Storage.Directory : parent + Path.DirectorySeparatorChar + Args.Instance.Storage.Directory;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Init()
        {
            if (!IsAlive)
            {
                try
                {
                    Options options = Args.Instance.Storage.GetOptionsByDbName(DataBaseName);
                    this.db = new DB(options, DataBasePath);
                    IsAlive = this.db != null ? true : false;
                }
                catch (System.Exception e)
                {
                    Logger.Error("Can't initialize database source", e);
                    throw e;
                }
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
            using (Iterator it = this.db.CreateIterator(new ReadOptions()))
            {
                for (it.SeekToFirst(); it.IsValid(); it.Next())
                {
                    result.Add(it.Key());
                }
            }

            return result;
        }

        public HashSet<byte[]> AllValue()
        {
            if (!IsAlive) return null;

            HashSet<byte[]> result = new HashSet<byte[]>();
            using (Iterator it = this.db.CreateIterator(new ReadOptions()))
            {
                for (it.SeekToFirst(); it.IsValid(); it.Next())
                {
                    result.Add(it.Value());
                }
            }

            return result;
        }

        public void DeleteData(byte[] key)
        {
            if (IsAlive)
            {
                this.db.Delete(key, new WriteOptions());
            }
        }

        public void DeleteData(byte[] key, WriteOptions options)
        {
            if (IsAlive)
            {
                this.db.Delete(key, options);
            }
        }

        public bool Flush()
        {
            return false;
        }

        public byte[] GetData(byte[] key)
        {
            if (!IsAlive) return null;
            return this.db.Get(key, new ReadOptions());
        }

        public long GetTotal()
        {
            if (IsAlive) return 0;

            long result = 0L;
            using (Iterator it = this.db.CreateIterator(new ReadOptions()))
            {
                for (it.SeekToFirst(); it.IsValid(); it.Next())
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
                PutData(key, value, new WriteOptions());
            }
        }

        public void PutData(byte[] key, byte[] value, WriteOptions options)
        {
            if (IsAlive)
            {
                this.db.Put(key, value, options);
            }
        }

        public void UpdateByBatch(Dictionary<byte[], byte[]> rows)
        {
            if (IsAlive)
            {
                UpdateByBatch(rows, new WriteOptions());
            }
        }

        public void UpdateByBatch(Dictionary<byte[], byte[]> rows, WriteOptions options)
        {
            if (!IsAlive) return;

            WriteBatch batch = new WriteBatch();
            foreach (KeyValuePair<byte[], byte[]> row in rows)
            {
                batch.Put(row.Key, row.Value);
            }
            this.db.Write(batch, new WriteOptions());
        }

        public HashSet<byte[]> GetLatestValues(long limit)
        {
            HashSet<byte[]> result = new HashSet<byte[]>();

            if (limit <= 0)
                return result;

            Iterator it = this.db.CreateIterator(new ReadOptions());
            // TODO 빠진부분 확인해야함

            long i = 0;
            for (it.SeekToLast(); it.IsValid() && i++ < limit; it.Prev())
            {
                result.Add(it.Value());
                i++;
            }

            return result;
        }

        public Dictionary<byte[], byte[]> GetNext(byte[] key, long limit)
        {
            Dictionary<byte[], byte[]> result = new Dictionary<byte[], byte[]>();
            if (limit <= 0)
                return result;

            Iterator it = this.db.CreateIterator(new ReadOptions());
            long i = 0;
            for (it.Seek(key); it.IsValid() && i++ < limit; it.Next())
            {
                result.Add(it.Key(), it.Value());
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
            Iterator it = this.db.CreateIterator(new ReadOptions());
            for (it.SeekToFirst(); it.IsValid() && i++ < limit; it.Next())
            {
                if (it.Key().Length >= precision)
                {
                    if (ByteUtil.Compare(
                            ArrayUtil.GetRange(key, 0, precision),
                            ArrayUtil.GetRange(it.Key(), 0, precision))
                            < 0)
                    {
                        break;
                    }
                    result.Add(it.Key(), it.Value());
                }
            }

            return result;
        }

        public Dictionary<byte[], byte[]> GetAll()
        {
            Dictionary<byte[], byte[]> result = new Dictionary<byte[], byte[]>();

            Iterator it = this.db.CreateIterator(new ReadOptions());
            for (it.SeekToFirst(); it.IsValid(); it.Next())
            {
                result.Add(it.Key(), it.Value());
            }

            return result;
        }

        public IEnumerator<KeyValuePair<byte[], byte[]>> GetEnumerator()
        {
            Iterator it = this.db.CreateIterator(new ReadOptions());
            for (; it.IsValid(); it.Next())
            {
                yield return new KeyValuePair<byte[], byte[]>(it.Key(), it.Value());
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator<KeyValuePair<byte[], byte[]>>)GetEnumerator();
        }
        #endregion
    }
}
