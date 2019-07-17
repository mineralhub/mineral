using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Mineral.Utils;
using RocksDbSharp;

namespace Mineral.Common.Storage
{
    public class RocksDBDataSource : IDBSourceInter<byte[]>, IEnumerable<KeyValuePair<byte[], byte[]>>
    {
        #region Field
        private static readonly string ENGINE = "ENGINE";

        private string database_name;
        private string parent = "";
        private RocksDb db;
        private ReadOptions read_options;
        #endregion


        #region Property
        public string DataBaseName { get { return this.database_name; } set { this.parent = value; } }
        public string DataBasePath { get { return this.parent + @"\" + this.database_name; } }
        public bool IsAlive { get; set; }
        #endregion


        #region Constructor
        public RocksDBDataSource(string parent, string name)
        {
            this.parent = parent;
            this.database_name = name;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public HashSet<byte[]> AllKeys()
        {
            if (!IsAlive) return null;

            HashSet<byte[]> result = new HashSet<byte[]>();
            using (Iterator it = this.db.NewIterator())
            {
                for (it.SeekToFirst(); it.Valid(); it.Next())
                {
                    result.Add(it.Key());
                }
            }
            return result;
        }

        public HashSet<byte[]> AllValue()
        {
            return null;
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

        public void DeleteData(byte[] key)
        {
            if (IsAlive)
            {
                this.db.Remove(key);
            }
        }

        public void DeleteData(byte[] key, WriteOptionWrapper options)
        {
            if (IsAlive)
            {
                this.db.Remove(key, null, options.Rocks);
            }
        }

        public bool Flush()
        {
            return false;
        }

        public byte[] GetData(byte[] key)
        {
            if (!IsAlive) return null;

            return this.db.Get(key);
        }

        public long GetTotal()
        {
            if (IsAlive) return 0;

            long result = 0;
            using (Iterator it = this.db.NewIterator())
            {
                for (it.SeekToFirst(); it.Valid(); it.Next())
                {
                    result++;
                }
            }
            return result;
        }

        public void Init()
        {
            if (IsAlive) return;

            DbOptions options = new DbOptions();
            RocksDbSettings settings = RocksDbSettings.GetSettings();
            if (settings.EnableStatistics)
            {
                options.SetStatsDumpPeriodSec(60);
            }

            options.SetCreateIfMissing(true);
            options.IncreaseParallelism(1);
            options.SetLevelCompactionDynamicLevelBytes(true);
            options.SetMaxOpenFiles(settings.MaxOpenFiles);

            options.SetNumLevels(settings.LevelNumber);
            options.SetMaxBytesForLevelBase(settings.MaxBytesForLevelBase);
            options.SetMaxBytesForLevelMultiplier(settings.MaxBytesForLevelMultiplier);
            options.SetLevel0FileNumCompactionTrigger(settings.Level0FileNumCompactionTrigger);
            options.SetTargetFileSizeBase(settings.TargetFileSizeBase);
            options.SetTargetFileSizeMultiplier(settings.TargetFileSizeMultiplier);

            BlockBasedTableOptions table_options;
            options.SetBlockBasedTableFactory(table_options = new BlockBasedTableOptions());
            table_options.SetBlockSize(settings.BlockSize);
            // setBlockCacheSize(32 * 1024 * 1024);
            table_options.SetCacheIndexAndFilterBlocks(true);
            table_options.SetPinL0FilterAndIndexBlocksInCache(true);
            table_options.SetFilterPolicy(BloomFilterPolicy.Create(10, false));

            read_options = new ReadOptions();
            read_options = read_options.SetPrefixSameAsStart(true).SetVerifyChecksums(false);

            this.db = RocksDb.Open(options, DataBasePath);
            IsAlive = this.db != null ? true : false;
        }

        public void PutData(byte[] key, byte[] value)
        {
            if (IsAlive)
            {
                this.db.Put(key, value);
            }
        }

        public void PutData(byte[] key, byte[] value, WriteOptionWrapper options)
        {
            if (IsAlive)
            {
                this.db.Put(key, value, null, options.Rocks);
            }
        }

        public void UpdateByBatch(Dictionary<byte[], byte[]> rows)
        {
            UpdateByBatch(rows, null);
        }

        public void UpdateByBatch(Dictionary<byte[], byte[]> rows, WriteOptionWrapper options)
        {
            WriteBatch batch = new WriteBatch();
            foreach (KeyValuePair<byte[], byte[]> row in rows)
            {
                batch.Put(row.Key, row.Value);
            }
            this.db.Write(batch, options.Rocks);
        }

        public HashSet<byte[]> GetLatestValues(long limit)
        {
            HashSet<byte[]> result = new HashSet<byte[]>();

            if (limit <= 0)
                return result;

            Iterator it = this.db.NewIterator();
            long i = 0;
            for (it.SeekToLast(); it.Valid(); it.Prev())
            {
                result.Add(it.Value());
            }

            return result;
        }

        public Dictionary<byte[], byte[]> GetNext(byte[] key, long limit)
        {
            Dictionary<byte[], byte[]> result = new Dictionary<byte[], byte[]>();
            if (limit <= 0)
                return result;

            Iterator it = this.db.NewIterator();
            long i = 0;
            for (it.Seek(key); it.Valid() && i++ < limit; it.Next())
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
            Iterator it = this.db.NewIterator();
            for (it.SeekToFirst(); it.Valid() && i++ < limit; it.Next())
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

            Iterator it = this.db.NewIterator();
            for (it.SeekToFirst(); it.Valid(); it.Next())
            {
                result.Add(it.Key(), it.Value());
            }

            return result;
        }

        public IEnumerator<KeyValuePair<byte[], byte[]>> GetEnumerator()
        {
            Iterator it = this.db.NewIterator();
            for (; it.Valid(); it.Next())
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
