using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Commander.NET;
using Commander.NET.Attributes;
using Mineral.CommandLine;
using Mineral.Core.Exception;
using Mineral.Utils;
using Mineral.Wallets.KeyStore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mineral.Core.Config.Arguments
{
    using Config = Mineral.Config;

    public class Args
    {
        #region Field
        private static Args instance = null;

        private Storage storage = new Storage();
        private LocalWitness local_witness = new LocalWitness();


        public static readonly string Version = "1.0.0";

        [Parameter("-p", "==private-key", Description = "Private key")]
        private string privatekey = "";
        [Parameter("-v", "--version", Description = "Version")]
        private bool version = false;
        [Parameter("-w", "--witness", Description = "Version")]
        private bool witness = false;
        [Parameter("--witness-address", Description = "Witness address")]
        private string witness_address = "";
        [Parameter("--storage-db-directory", Description = "Storage db directory")]
        private string storage_directory = "";
        [Parameter("--storage-db-engine", Description = "Storage db engine.(leveldb or rocksdb)")]
        private string storage_engine = "";
        [Parameter("--storage-db-synchronous", Description = "Storage db is synchronous or not.(true or flase)")]
        private string storage_sync = "";
        [Parameter("--contract-parse-enable", Description = "enable contract parses in java-tron or not.(true or flase)")]
        private string contract_parse_enable = "";
        [Parameter("storage-index-directory", Description = "Storage index directory")]
        private string storage_index_directory = "";
        [Parameter("storage-index-switch", Description = "Storage index switch.(on or off)")]
        private string storage_index_switch = "";
        [Parameter("--storage-transactionHistory-switch", Description = "Storage transaction history switch.(on or off)")]
        private string storage_transaction_history_switch = "";
        [Parameter("--storage-db-version", Description = "Storage db version.(1 or 2)")]
        private String storage_version = "";
        #endregion


        #region Property
        public static Args Instance
        {
            get
            {
                if (instance == null)
                    instance = new Args();
                return instance;
            }
        }
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public static void SetParam(string[] args, string config_path)
        {
            CommanderParser<Args> parser = new CommanderParser<Args>();
            instance = parser.Add(args).Parse();

            if (instance.version)
            {
                Console.WriteLine(Version);
                return;
            }

            Config.Instance.Initialize(config_path);

            if (Config.Instance.Net.Type.Equals("mainnet"))
            {
                // TODO : Wallet address prefix mainnet
            }
            else
            {
                // TODO : Wallet address prefix testnet
            }

            if (instance.privatekey.Length > 0)
            {
                instance.local_witness = new LocalWitness(instance.privatekey);
                if (!string.IsNullOrEmpty(instance.witness_address))
                {
                    UInt160 address_hash = AccountHelper.ToAddressHash(instance.witness_address);
                    if (!address_hash.IsNullOrEmpty())
                    {
                        instance.local_witness.SetWitnessAccountAddress(address_hash);
                        Logger.Debug("local witness account address from command.");
                    }
                    else
                    {
                        instance.witness_address = "";
                        Logger.Warning("local witness account address is incorrect.");
                    }
                }
                instance.local_witness.InitWitnessAccountAddress();
            }
            else if (CollectionHelper.IsNotNullOrEmpty(Config.Instance.Witness.LocalWitness))
            {
                instance.local_witness = new LocalWitness();

                List<string> witness_list = new List<string>();
                if (witness_list.Count > 1)
                {
                    Logger.Warning("Local witness count must be one. get the first witness");
                    witness_list = witness_list.GetRange(0, 1);
                }
                instance.local_witness.SetPrivateKeys(witness_list);

                if (StringHelper.IsNotNullOrEmpty(Config.Instance.Witness.LocalWitnessAccountAddress))
                {
                    UInt160 address = AccountHelper.ToAddressHash(Config.Instance.Witness.LocalWitnessAccountAddress);
                    if (address != UInt160.Zero)
                    {
                        instance.local_witness.SetWitnessAccountAddress(address);
                        Logger.Debug("local witness account address from \'config.conf\' file.");
                    }
                    else
                    {
                        Logger.Warning("local witness account address is incorrect.");
                    }
                }
                instance.local_witness.InitWitnessAccountAddress();
            }
            else if (CollectionHelper.IsNotNullOrEmpty(Config.Instance.Witness.LocalWitnessKeyStore))
            {
                List<string> privatekey_list = new List<string>();

                instance.local_witness = new LocalWitness();
                if (instance.witness)
                {
                    // TODO : Keystore 로드 CLI와 함께 정리해서 중복제거
                    if (Config.Instance.Witness.LocalWitnessKeyStore.Count > 0)
                    {
                        string file_path = Config.Instance.Witness.LocalWitnessKeyStore[0];

                        JObject json;
                        using (var file = File.OpenText(file_path))
                        {
                            string data = file.ReadToEnd();
                            json = JObject.Parse(data);
                        }

                        string password = CommandLineUtil.ReadPasswordString("Password: ");
                        if (string.IsNullOrEmpty(password))
                        {
                            throw new ArgumentNullException(
                                "Invalid password."
                                );
                        }

                        KeyStore keystore = new KeyStore();
                        keystore = JsonConvert.DeserializeObject<KeyStore>(json.ToString());

                        byte[] privatekey = null;
                        if (!KeyStoreService.DecryptKeyStore(password, keystore, out privatekey))
                        {
                            throw new KeyStoreException(
                                "Fail to decrypt keystore file."
                                );
                        }
                        privatekey_list.Add(privatekey.ToHexString());
                    }
                }
                instance.local_witness.SetPrivateKeys(privatekey_list);
                Logger.Debug("local witness account address from keystore file.");
            }

            if (instance.witness && CollectionHelper.IsNullOrEmpty(instance.local_witness.GetPrivateKey()))
            {
                Logger.Warning("local witness null");
            }

            instance.storage = new Storage();
            //instance.storage.Version = 
        }
        #endregion
    }
}
