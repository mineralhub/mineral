using System;
using System.Collections.Generic;
using System.Text;
using LevelDB;
using Mineral.Utils;

namespace Mineral.Core.Config.Arguments
{
    using Config = Mineral.Config;

    public class Storage
    {
        #region Field
        private static readonly bool DEFAULT_DB_SYNC = false;
        private static readonly bool DEFAULT_EVENT_SUBSCRIB_CONTRACT_PARSE = true;
        private static readonly string DEFAULT_DB_DIRECTORY = "database";
        private static readonly string DEFAULT_INDEX_DIRECTORY = "index";
        private static readonly string DEFAULT_INDEX_SWTICH = "on";
        private static readonly string DEFAULT_TRANSACTION_HISTORY_SWITCH = "on";

        private static readonly CompressionLevel DEFAULT_COMPRESSION_TYPE = CompressionLevel.SnappyCompression;
        private static readonly int DEFAULT_BLOCK_SIZE = 4 * 1024;
        private static readonly int DEFAULT_WRITE_BUFFER_SIZE = 10 * 1024 * 1024;
        private static readonly long DEFAULT_CACHE_SIZE = 10 * 1024 * 1024L;
        private static readonly int DEFAULT_MAX_OPEN_FILES = 100;

        private Dictionary<string, Property> properties = new Dictionary<string, Property>();
        #endregion


        #region Property
        public bool Sync { get; set; }
        public bool ContractParseSwitch { get; set; }
        public string Directory { get; set; }
        public string IndexDirectory { get; set; }
        public string IndexSwitch { get; set; }
        public string TransactionHistorySwitch { get; set; }
        public bool NeedToUpdateAsset { get; set; }
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

            options.CompressionLevel = CompressionLevel.SnappyCompression;
            options.BlockSize = DEFAULT_BLOCK_SIZE;
            options.WriteBufferSize = DEFAULT_WRITE_BUFFER_SIZE;
            options.Cache = new LevelDB.Cache((int)DEFAULT_CACHE_SIZE);
            options.MaxOpenFiles = DEFAULT_MAX_OPEN_FILES;

            return options;
        }
        #endregion


        #region External Method
        public static bool GetSyncFromConfig()
        {
            return Config.Instance.Storage?.Sync ?? DEFAULT_DB_SYNC;
        }

        public static bool GetContractParseSwitchFromConfig()
        {
            return Config.Instance.Event?.ContractParse ?? DEFAULT_EVENT_SUBSCRIB_CONTRACT_PARSE;
        }

        public static string GetDirectoryFromConfig()
        {
            return CollectionUtil.IsNotNullOrEmpty(Config.Instance.Storage?.Directory) ? Config.Instance.Storage.Directory : DEFAULT_DB_DIRECTORY;
        }

        public static string GetIndexDirectoryFromConfig()
        {
            return CollectionUtil.IsNotNullOrEmpty(Config.Instance.Storage?.IndexDirectory) ? Config.Instance.Storage.IndexDirectory : DEFAULT_INDEX_DIRECTORY;
        }

        public static string GetIndexSwitchFromConfig()
        {
            return CollectionUtil.IsNotNullOrEmpty(Config.Instance.Storage?.IndexSwitch) ? Config.Instance.Storage.IndexSwitch : DEFAULT_INDEX_SWTICH;
        }

        public static string GetTransactionHistorySwitchFromConfig()
        {
            return CollectionUtil.IsNotNullOrEmpty(Config.Instance.Storage?.TransactionHistorySwitch) ? Config.Instance.Storage.TransactionHistorySwitch : DEFAULT_TRANSACTION_HISTORY_SWITCH;
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
