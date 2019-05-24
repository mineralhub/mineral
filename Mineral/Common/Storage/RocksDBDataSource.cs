using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Utils;
using RocksDbSharp;

namespace Mineral.Common.Storage
{
    public class RocksDBDataSource : IDBSourceInter<byte[]>
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

        public Iterator GetEnumerator()
        {
            return this.db.NewIterator();
        }
        #endregion
    }
}
