using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Storage
{
    public class RocksDbSettings
    {
        #region Field
        private static RocksDbSettings settings;

        private int level_number = 0;
        private int max_open_file = 0;
        private int compact_thread = 0;
        private ulong block_size = 0;
        private ulong max_bytes_for_level_base = 0;
        private double max_bytes_for_level_multiplier = 0;
        private int level0_file_num_compaction_trigger = 0;
        private ulong target_file_size_Base = 0;
        private int target_file_size_multiplier = 0;
        private bool enable_statistics = false;
        #endregion


        #region Property
        public int LevelNumber { get { return level_number; } }
        public int MaxOpenFiles { get { return max_open_file; } }
        public int CompactThread { get { return compact_thread; } }
        public ulong BlockSize { get { return block_size; } }
        public ulong MaxBytesForLevelBase { get { return max_bytes_for_level_base; } }
        public double MaxBytesForLevelMultiplier { get { return max_bytes_for_level_multiplier; } }
        public int Level0FileNumCompactionTrigger { get { return level0_file_num_compaction_trigger; } }
        public ulong TargetFileSizeBase { get { return target_file_size_Base; } }
        public int TargetFileSizeMultiplier { get { return target_file_size_multiplier; } }
        public bool EnableStatistics { get { return enable_statistics; } }
        #endregion


        #region Constructor
        private RocksDbSettings()
        {
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion



        #region External Method
        public static RocksDbSettings GetSettings()
        {
            if (settings == null)
                return GetDefaultSettings();
            return settings;
        }

        public static RocksDbSettings GetDefaultSettings()
        {
            RocksDbSettings settings = new RocksDbSettings();
            return settings.WithLevelNumber(7).WithBlockSize(54).WithCompactThreads(32)
                .WithTargetFileSizeBase(256).WithMaxBytesForLevelMultiplier(10)
                .WithTargetFileSizeMultiplier(1).WithMaxBytesForLevelBase(256)
                .WithMaxOpenFiles(-1).WithEnableStatistics(false);
        }

        public static RocksDbSettings IntiCustomSettings(
                            int level_number, int compact_thread,
                            ulong block_size, ulong max_bytes_for_level_base,
                            double max_bytes_for_level_multiplier, int leve_file_num_compaction_trigger,
                            ulong target_file_size_base, int target_file_size_multiplier)
        {
            settings = new RocksDbSettings()
                .WithMaxOpenFiles(-1)
                .WithEnableStatistics(false)
                .WithLevelNumber(level_number)
                .WithCompactThreads(compact_thread)
                .WithBlockSize(block_size)
                .WithMaxBytesForLevelBase(max_bytes_for_level_base)
                .WithMaxBytesForLevelMultiplier(max_bytes_for_level_multiplier)
                .WithLevel0FileNumCompactionTrigger(leve_file_num_compaction_trigger)
                .WithTargetFileSizeBase(target_file_size_base)
                .WithTargetFileSizeMultiplier(target_file_size_multiplier);

            return settings;
        }

        public RocksDbSettings WithMaxOpenFiles(int max_open_file)
        {
            this.max_open_file = max_open_file;
            return this;
        }

        public RocksDbSettings WithCompactThreads(int compact_thread)
        {
            this.compact_thread = compact_thread;
            return this;
        }

        public RocksDbSettings WithBlockSize(ulong block_size)
        {
            this.block_size = block_size * 1024;
            return this;
        }

        public RocksDbSettings WithMaxBytesForLevelBase(ulong max_bytes_for_level_base)
        {
            this.max_bytes_for_level_base = max_bytes_for_level_base * 1024 * 1024;
            return this;
        }

        public RocksDbSettings WithMaxBytesForLevelMultiplier(double max_bytes_for_level_multiplier)
        {
            this.max_bytes_for_level_multiplier = max_bytes_for_level_multiplier;
            return this;
        }

        public RocksDbSettings WithLevel0FileNumCompactionTrigger(int level0_file_num_compaction_trigger)
        {
            this.level0_file_num_compaction_trigger = level0_file_num_compaction_trigger;
            return this;
        }

        public RocksDbSettings WithEnableStatistics(bool enable)
        {
            this.enable_statistics = enable;
            return this;
        }

        public RocksDbSettings WithLevelNumber(int level_number)
        {
            this.level_number = level_number;
            return this;
        }

        public RocksDbSettings WithTargetFileSizeBase(ulong target_file_size_Base)
        {
            this.target_file_size_Base = target_file_size_Base * 1024 * 1024;
            return this;
        }

        public RocksDbSettings WithTargetFileSizeMultiplier(int target_file_size_multiplier)
        {
            this.target_file_size_multiplier = target_file_size_multiplier;
            return this;
        }

        public static void OutputLogSettings()
        {
            Logger.Info(string.Format(
                "level number: {0}, CompactThreads: {1}, Blocksize: {2}, maxBytesForLevelBase: {3},"
                + "withMaxBytesForLevelMultiplier: {4}, level0FileNumCompactionTrigger: {5}, "
                + "withTargetFileSizeBase: {6}, withTargetFileSizeMultiplier: {7}",
                settings.LevelNumber,
                settings.CompactThread, settings.BlockSize, settings.MaxBytesForLevelBase,
                settings.MaxBytesForLevelMultiplier, settings.Level0FileNumCompactionTrigger,
                settings.TargetFileSizeBase, settings.TargetFileSizeMultiplier));
        }
        #endregion
    }
}
