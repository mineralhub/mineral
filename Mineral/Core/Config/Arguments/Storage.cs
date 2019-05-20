using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Database.LevelDB;
using Mineral.Utils;

namespace Mineral.Core.Config.Arguments
{
    using Config = Mineral.Config;

    public class Storage
    {
        #region Field
        private static readonly string DB_DIRECTORY_CONFIG_KEY = "storage.db.directory";
        private static readonly string DB_VERSION_CONFIG_KEY = "storage.db.version";
        private static readonly string DB_ENGINE_CONFIG_KEY = "storage.db.engine";
        private static readonly string DB_SYNC_CONFIG_KEY = "storage.db.sync";
        private static readonly string INDEX_DIRECTORY_CONFIG_KEY = "storage.index.directory";
        private static readonly string INDEX_SWITCH_CONFIG_KEY = "storage.index.switch";
        private static readonly string TRANSACTIONHISTORY_SWITCH_CONFIG_KEY = "storage.transHistory.switch";
        private static readonly string PROPERTIES_CONFIG_KEY = "storage.properties";
        private static readonly string DEFAULT_TRANSACTION_HISTORY_SWITCH = "on";

        private static readonly string NAME_CONFIG_KEY = "name";
        private static readonly string PATH_CONFIG_KEY = "path";
        private static readonly string CREATE_IF_MISSING_CONFIG_KEY = "createIfMissing";
        private static readonly string PARANOID_CHECKS_CONFIG_KEY = "paranoidChecks";
        private static readonly string VERITY_CHECK_SUMS_CONFIG_KEY = "verifyChecksums";
        private static readonly string COMPRESSION_TYPE_CONFIG_KEY = "compressionType";
        private static readonly string BLOCK_SIZE_CONFIG_KEY = "blockSize";
        private static readonly string WRITE_BUFFER_SIZE_CONFIG_KEY = "writeBufferSize";
        private static readonly string CACHE_SIZE_CONFIG_KEY = "cacheSize";
        private static readonly string MAX_OPEN_FILES_CONFIG_KEY = "maxOpenFiles";
        private static readonly string EVENT_SUBSCRIB_CONTRACT_PARSE = "event.subscribe.contractParse";

        private static readonly int DEFAULT_DB_VERSION = 2;
        private static readonly string DEFAULT_DB_ENGINE = "LEVELDB";
        private static readonly bool DEFAULT_DB_SYNC = false;
        private static readonly bool DEFAULT_EVENT_SUBSCRIB_CONTRACT_PARSE = true;
        private static readonly string DEFAULT_DB_DIRECTORY = "database";
        private static readonly string DEFAULT_INDEX_DIRECTORY = "index";
        private static readonly string DEFAULT_INDEX_SWTICH = "on";

        private static readonly CompressionType DEFAULT_COMPRESSION_TYPE = CompressionType.kSnappyCompression;
        private static readonly int DEFAULT_BLOCK_SIZE = 4 * 1024;
        private static readonly int DEFAULT_WRITE_BUFFER_SIZE = 10 * 1024 * 1024;
        private static readonly long DEFAULT_CACHE_SIZE = 10 * 1024 * 1024L;
        private static readonly int DEFAULT_MAX_OPEN_FILES = 100;

        private Dictionary<string, Property> properties = null;
        #endregion


        #region Property
        public int Version { get; set; }
        public bool Sync { get; set; }
        public bool ContractParseSwitch { get; set; }
        public string Directory { get; set; }
        public string Engine { get; set; }
        public string IndexDirectory { get; set; }
        public string IndexSwitch { get; set; }
        public string TransactionHistorySwitch { get; set; }
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private Options CreateDefaultOption()
        {
            Options options = new Options();

            options.CreateIfMissing = true;
            options.ParanoidChecks = true;

            options.Compression = CompressionType.kSnappyCompression;
            options.BlockSize = DEFAULT_BLOCK_SIZE;
            options.WriteBufferSize = DEFAULT_WRITE_BUFFER_SIZE;
            options.CacheSize = DEFAULT_CACHE_SIZE;
            options.MaxOpenFiles = DEFAULT_MAX_OPEN_FILES;

            return options;
        }
        #endregion


        #region External Method
        public static int GetVersionFromConfig()
        {
            return Config.Instance.Storage?.Version ?? DEFAULT_DB_VERSION;
        }

        public static string GetEngineFromConfig()
        {
            return StringHelper.IsNotNullOrEmpty(Config.Instance.Storage.Engine) ? Config.Instance.Storage.Engine : DEFAULT_DB_ENGINE;
        }

        public static bool GetSyncFromConfig()
        {
            return Config.Instance.Storage?.Sync ?? DEFAULT_DB_SYNC;
        }

        public static string GetDirectoryFromConfig()
        {
            return StringHelper.IsNotNullOrEmpty(Config.Instance.Storage?.Directory) ? Config.Instance.Storage.Directory : DEFAULT_DB_DIRECTORY;
        }

        public static string GetIndexDirectoryFromConfig()
        {
            return StringHelper.IsNotNullOrEmpty(Config.Instance.Storage?.IndexDirectory) ? Config.Instance.Storage.IndexDirectory : DEFAULT_INDEX_DIRECTORY;
        }

        public static string GetIndexSwitchFromConfig()
        {
            return StringHelper.IsNotNullOrEmpty(Config.Instance.Storage?.IndexSwitch) ? Config.Instance.Storage.IndexSwitch : DEFAULT_INDEX_SWTICH;
        }

        public static string GetTransactionHistorySwitchFromConfig()
        {
            return StringHelper.IsNotNullOrEmpty(Config.Instance.Storage?.TransactionHistorySwitch) ? Config.Instance.Storage.TransactionHistorySwitch : DEFAULT_TRANSACTION_HISTORY_SWITCH;
        }

        public void SetPropertyFromConfig()
        {
            if (Config.Instance.Storage != null && Config.Instance.Storage.Properties != null)
            {
                foreach (Property property in Config.Instance.Storage.Properties)
                {
                    properties.Add(property.Name, property);
                }
            }
        }

        public string GetPathByDbName(string name)
        {
            return properties.ContainsKey(name) ? properties[name].Path : "";
        }

        public Options GetOptionsByDbName(string name)
        {
            return properties.ContainsKey(name) ? properties[name].Option : CreateDefaultOption();
        }
        #endregion
    }
}
