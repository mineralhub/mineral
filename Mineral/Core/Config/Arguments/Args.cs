using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Commander.NET;
using Commander.NET.Attributes;
using Mineral.CommandLine;
using Mineral.Common.Overlay.Discover;
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
        public class GenesisBlockArgs : GenesisBlockConfig { }
        public class BlockArgs : BlockConfig { }
        public class CommitteArgs : CommitteConfig { }
        public class TransactionArgs : TransactionConfig { }

        public class NodeArgs
        {
            public class DiscoveryArgs : DiscoveryConfig { }
            public class P2PArgs : P2PConfig { }
            public class HttpArgs : HttpConfig { }
            public class RPCArgs : RPCConfig { }

            public string TrustNode { get; set; }
            public int ConnectionTimeout { get; set; }
            public int ChannelReadTimeout { get; set; }
            public int ValidateSignThreadNum { get; set; }
            public bool WalletExtensionAPI { get; set; }
            public double ConnectFactor { get; set; }
            public double ActiveConnectFactor { get; set; }
            public double DisconnectNumberFactor { get; set; }
            public double MaxconnectNumberFactor { get; set; }
            public long ReceiveTcpMinDataLength { get; set; }
            public bool IsOpenFullTcpDisconnect { get; set; }
            public int MaxActiveNodes { get; set; }
            public int MaxActiveNodeSameIP { get; set; }
            public int MinParticipationRate { get; set; }
            public int ListenPort { get; set; }
            public int BlockProducedTimeout { get; set; }
            public int SolidityThread { get; set; }
            public long NetMaxTrxPerSecond { get; set; }
            public List<Node> Active { get; set; } = new List<Node>();
            public List<Node> Passive { get; set; } = new List<Node>();
            public List<Node> FastForward { get; set; } = new List<Node>();
            public DiscoveryArgs Discovery { get; set; }
            public BackupConfig Backup { get; set; }
            public P2PArgs P2P { get; set; }
            public HttpArgs HTTP { get; set; }
            public RPCArgs RPC { get; set; }
        }


        #region Field
        private static Args instance = null;

        public static readonly string Version = "1.0.0";

        [Parameter("-p", "==private-key", Description = "Private key")]
        private string privatekey = "";
        [Parameter("-v", "--version", Description = "Version")]
        private bool version = false;
        [Parameter(Description = "--seed-nodes")]
        private List<String> seed_nodes = new List<string>();
        [Parameter("--storage-db-directory", Description = "Storage db directory")]
        private string storage_directory = "";
        [Parameter("--storage-db-engine", Description = "Storage db engine.(leveldb or rocksdb)")]
        private string storage_engine = "";
        [Parameter("--storage-db-synchronous", Description = "Storage db is synchronous or not.(true or flase)")]
        private string storage_sync = "";
        [Parameter("--contract-parse-switch", Description = "enable contract parses in java-tron or not.(true or flase)")]
        private string contract_parse_switch = "";
        [Parameter("--d", "--output-directory", Description = "Directory")]
        private string output_directory = "";
        [Parameter("storage-index-directory", Description = "Storage index directory")]
        private string storage_index_directory = "";
        [Parameter("storage-index-switch", Description = "Storage index switch.(on or off)")]
        private string storage_index_switch = "";
        [Parameter("--storage-transactionHistory-switch", Description = "Storage transaction history switch.(on or off)")]
        private string storage_transaction_history_switch = "";
        [Parameter("--storage-db-version", Description = "Storage db version.(1 or 2)")]
        private String storage_version = "";
        [Parameter("-w", "--witness", Description = "Version")]
        private bool witness = false;
        [Parameter("--witness-address", Description = "Witness address")]
        private string witness_address = "";
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

        public SeedNode Seed { get; private set; } = new SeedNode();
        public Storage Storage { get; private set; } = new Storage();
        public GenesisBlockArgs Genesisblock { get; private set; } = new GenesisBlockArgs();
        public LocalWitness LocalWitness { get; private set; } = new LocalWitness();
        public NodeArgs Node { get; private set; } = new NodeArgs();
        public BlockArgs Block { get; set; } = new BlockArgs();
        public CommitteArgs Committe { get; set; } = new CommitteArgs();
        public TransactionArgs Transaction { get; set; } = new TransactionArgs();

        public bool IsWitness { get { return this.witness; } set { this.witness = value; } }
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private static void OutputLogConfig()
        {
            Logger.Info(string.Format("\n"));
            Logger.Info(string.Format("************************ Net config ************************"));
            Logger.Info(string.Format("P2P version: {0}", instance.Node.P2P.Version));
            Logger.Info(string.Format("Bind IP: {0}", instance.Node.Discovery.BindIP));
            Logger.Info(string.Format("External IP: {0}", instance.Node.Discovery.ExternalIP));
            Logger.Info(string.Format("Listen port: {0}", instance.Node.ListenPort));
            Logger.Info(string.Format("Discover enable: {0}", instance.Node.Discovery.Enable));
            Logger.Info(string.Format("Active node size: {0}", instance.Node.Active.Count));
            Logger.Info(string.Format("Passive node size: {0}", instance.Node.Passive.Count));
            Logger.Info(string.Format("Seed node size: {0}", instance.Seed.IpList.Count));
            Logger.Info(string.Format("Max connection: {0}", instance.Node.MaxActiveNodes));
            Logger.Info(string.Format("Max connection with same IP: {0}", instance.Node.MaxActiveNodeSameIP));
            Logger.Info(string.Format("Solidity threads: {0}", instance.Node.SolidityThread));
            Logger.Info(string.Format("************************ Backup config ************************"));
            Logger.Info(string.Format("Backup listen port: {0}", instance.Node.Backup.Port));
            Logger.Info(string.Format("Backup member size: {0}", instance.Node.Backup.Members.Count));
            Logger.Info(string.Format("Backup priority: {0}", instance.Node.Backup.Priority));
            Logger.Info(string.Format("************************ Code version *************************"));
            Logger.Info(string.Format("Code version : {0}", Version));
            Logger.Info(string.Format("************************ DB config *************************"));
            Logger.Info(string.Format("DB version : {0}", instance.Storage.Version));
            Logger.Info(string.Format("DB engine : {0}", instance.Storage.Engine));
            Logger.Info(string.Format("***************************************************************"));
            Logger.Info(string.Format("\n"));
        }
        #endregion


        #region External Method
        public void SetParam(string[] args, string config_path)
        {
            CommanderParser<Args> parser = new CommanderParser<Args>();

            try
            {
                instance = parser.Add(args).Parse();
            }
            catch (System.Exception e)
            {
                Logger.Error(e.Message);
                return;
            }

            if (instance.version)
            {
                Console.WriteLine(Version);
                return;
            }

            Config.Instance.Initialize(config_path);

            #region Wallet prefix
            if (Config.Instance.Net.Type.Equals("mainnet"))
            {
                // TODO : Wallet address prefix mainnet
            }
            else
            {
                // TODO : Wallet address prefix testnet
            }
            #endregion

            #region  Local witness
            if (instance.privatekey.Length > 0)
            {
                instance.LocalWitness = new LocalWitness(instance.privatekey);
                if (!string.IsNullOrEmpty(instance.witness_address))
                {
                    UInt160 address_hash = AccountHelper.ToAddressHash(instance.witness_address);
                    if (!address_hash.IsNullOrEmpty())
                    {
                        instance.LocalWitness.SetWitnessAccountAddress(address_hash);
                        Logger.Debug("local witness account address from command.");
                    }
                    else
                    {
                        instance.witness_address = "";
                        Logger.Warning("local witness account address is incorrect.");
                    }
                }
                instance.LocalWitness.InitWitnessAccountAddress();
            }
            else if (CollectionHelper.IsNotNullOrEmpty(Config.Instance.Witness.LocalWitness))
            {
                instance.LocalWitness = new LocalWitness();

                List<string> witness_list = new List<string>();
                if (witness_list.Count > 1)
                {
                    Logger.Warning("Local witness count must be one. get the first witness");
                    witness_list = witness_list.GetRange(0, 1);
                }
                instance.LocalWitness.SetPrivateKeys(witness_list);

                if (StringHelper.IsNotNullOrEmpty(Config.Instance.Witness.LocalWitnessAccountAddress))
                {
                    UInt160 address = AccountHelper.ToAddressHash(Config.Instance.Witness.LocalWitnessAccountAddress);
                    if (address != UInt160.Zero)
                    {
                        instance.LocalWitness.SetWitnessAccountAddress(address);
                        Logger.Debug("local witness account address from \'config.conf\' file.");
                    }
                    else
                    {
                        Logger.Warning("local witness account address is incorrect.");
                    }
                }
                instance.LocalWitness.InitWitnessAccountAddress();
            }
            else if (CollectionHelper.IsNotNullOrEmpty(Config.Instance.Witness.LocalWitnessKeyStore))
            {
                List<string> privatekey_list = new List<string>();

                instance.LocalWitness = new LocalWitness();
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
                instance.LocalWitness.SetPrivateKeys(privatekey_list);
                Logger.Debug("local witness account address from keystore file.");
            }

            if (instance.witness && CollectionHelper.IsNullOrEmpty(instance.LocalWitness.GetPrivateKey()))
            {
                Logger.Warning("local witness null");
            }
            #endregion

            #region Storage
            instance.Storage = new Storage();

            instance.Storage.Version = StringHelper.IsNotNullOrEmpty(instance.storage_version) ?
                int.Parse(instance.storage_version) : Storage.GetVersionFromConfig();

            instance.Storage.Engine = StringHelper.IsNotNullOrEmpty(instance.storage_engine) ?
                instance.storage_engine : Storage.GetEngineFromConfig();

            if (instance.Storage.Version == 1 &&
                instance.Storage.Engine.ToUpper().Equals("ROCKSDB"))
            {
                throw new ConfigrationException("database version = 1 is not suppoerted ROCKSDB engine");
            }

            instance.Storage.Sync = StringHelper.IsNotNullOrEmpty(instance.storage_sync) ?
                bool.Parse(instance.storage_sync) : Storage.GetSyncFromConfig();

            instance.Storage.ContractParseSwitch = StringHelper.IsNotNullOrEmpty(instance.contract_parse_switch) ?
                bool.Parse(instance.contract_parse_switch) : Storage.GetContractParseSwitchFromConfig();

            instance.Storage.Directory = StringHelper.IsNotNullOrEmpty(instance.storage_directory) ?
                instance.storage_directory : Storage.GetDirectoryFromConfig();

            instance.Storage.IndexDirectory = StringHelper.IsNotNullOrEmpty(instance.storage_index_directory) ?
                instance.storage_index_directory : Storage.GetIndexDirectoryFromConfig();

            instance.Storage.IndexSwitch = StringHelper.IsNotNullOrEmpty(instance.storage_index_switch) ?
                instance.storage_index_switch : Storage.GetIndexSwitchFromConfig();

            instance.Storage.IndexDirectory = StringHelper.IsNotNullOrEmpty(instance.storage_transaction_history_switch) ?
                instance.storage_transaction_history_switch : Storage.GetTransactionHistorySwitchFromConfig();

            instance.Storage.NeedToUpdateAsset = Config.Instance.Storage.NeedToUpdateAsset ?? true;

            instance.Storage.SetPropertyFromConfig();

            instance.Seed.IpList = CollectionHelper.IsNotNullOrEmpty(instance.seed_nodes) ?
                instance.seed_nodes : Config.Instance.SeedNode.IpList;
            #endregion

            #region Genesis block
            instance.Genesisblock = (GenesisBlockArgs)(Config.Instance.GenesisBlock ?? GenesisBlockConfig.DefaultGenesisBlock);
            #endregion

            #region Node
            instance.Node.TrustNode = Config.Instance.Node.TrustNode;
            instance.Node.ListenPort = Config.Instance.Node.ListenPort ?? 0;
            instance.Node.ConnectionTimeout = Config.Instance.Node.ConnectionTimeout ?? 0;
            instance.Node.ChannelReadTimeout = Config.Instance.Node.ChannelReadTimeout ?? 0;
            instance.Node.ValidateSignThreadNum = Config.Instance.Node.ValidateSignThreadNum ?? Environment.ProcessorCount / 2;
            instance.Node.WalletExtensionAPI = Config.Instance.Node.WalletExtensionAPI ?? false;
            instance.Node.ConnectFactor = Config.Instance.Node.ConnectFactor ?? 0.3;
            instance.Node.ActiveConnectFactor = Config.Instance.Node.ActiveConnectFactor ?? 0.1;
            instance.Node.DisconnectNumberFactor = Config.Instance.Node.DisconnectNumberFactor ?? 0.4;
            instance.Node.MaxconnectNumberFactor = Config.Instance.Node.MaxconnectNumberFactor ?? 0.8;
            instance.Node.ReceiveTcpMinDataLength = Config.Instance.Node.ReceiveTcpMinDataLength ?? 2048;
            instance.Node.IsOpenFullTcpDisconnect = Config.Instance.Node.IsOpenFullTcpDisconnect ?? true;
            instance.Node.MaxActiveNodes = Config.Instance.Node.MaxActiveNodes ?? 30;
            instance.Node.MaxActiveNodeSameIP = Config.Instance.Node.MaxActiveNodeSameIP ?? 2;
            instance.Node.MinParticipationRate = Config.Instance.Node.MinParticipationRate ?? 0;
            instance.Node.BlockProducedTimeout = Config.Instance.Node.BlockProducedTimeout ?? Parameter.ChainParameters.BLOCK_PRODUCED_TIME_OUT;
            instance.Node.BlockProducedTimeout = Math.Max(instance.Node.BlockProducedTimeout, 30);
            instance.Node.BlockProducedTimeout = Math.Min(instance.Node.BlockProducedTimeout, 100);
            instance.Node.RPC.Thread = Config.Instance.Node.RPC.Thread ?? Environment.ProcessorCount / 2;
            instance.Node.NetMaxTrxPerSecond = Config.Instance.Node.NetMaxTrxPerSecond ?? Parameter.NetParameters.NET_MAX_TRX_PER_SECOND;

            instance.Node.Active = Config.Instance.Node.Active?.Select(uri => Mineral.Common.Overlay.Discover.Node.InstanceOf(uri)).ToList();
            instance.Node.Passive = Config.Instance.Node.Passive?.Select(uri => Mineral.Common.Overlay.Discover.Node.InstanceOf(uri)).ToList();
            instance.Node.FastForward = Config.Instance.Node.FastForward?.Select(uri => Mineral.Common.Overlay.Discover.Node.InstanceOf(uri)).ToList();

            instance.Node.Discovery.Enable = Config.Instance.Node.Discovery.Enable ?? false;
            instance.Node.Discovery.Persist = Config.Instance.Node.Discovery.Persist ?? false;

            instance.Node.Discovery.BindIP = StringHelper.IsNotNullOrEmpty(Config.Instance.Node.Discovery.BindIP) ?
                Config.Instance.Node.Discovery.BindIP : "0.0.0.0";
            instance.Node.Discovery.ExternalIP = StringHelper.IsNotNullOrEmpty(Config.Instance.Node.Discovery.ExternalIP) ?
                Config.Instance.Node.Discovery.ExternalIP : "0.0.0.0";

            instance.Node.Discovery.HomeNode = Config.Instance.Node.Discovery.Persist ?? false;

            instance.Node.Backup.Port = Config.Instance.Node.Backup.Port ?? 10001;
            instance.Node.Backup.Priority = Config.Instance.Node.Backup.Priority ?? 0;
            instance.Node.Backup.Members = Config.Instance.Node.Backup.Members?.Select(member => member).ToList();

            instance.Node.P2P.Version = Config.Instance.Node.P2P.Version ?? 0;
            instance.Node.P2P.PingInterval = Config.Instance.Node.P2P.PingInterval ?? 0;

            instance.Node.HTTP.FullNodePort = Config.Instance.Node.HTTP.FullNodePort ?? 11265;
            instance.Node.HTTP.SolidityPort = Config.Instance.Node.HTTP.SolidityPort ?? 11256;

            instance.Node.RPC.Port = Config.Instance.Node.RPC.Port ?? 11275;
            instance.Node.RPC.SolidityPort = Config.Instance.Node.RPC.SolidityPort ?? 11276;
            instance.Node.RPC.MaxConcurrentCallPerConnection = Config.Instance.Node.RPC.MaxConcurrentCallPerConnection ?? int.MaxValue;
            instance.Node.RPC.FlowControlWindow = Config.Instance.Node.RPC.FlowControlWindow ?? 1048576;
            instance.Node.RPC.MaxConnectionIdle = Config.Instance.Node.RPC.MaxConnectionIdle ?? long.MaxValue;

            instance.Node.RPC.MaxConnectionAge = Config.Instance.Node.RPC.MaxConnectionAge ?? long.MaxValue;
            instance.Node.RPC.MaxMessageSize = Config.Instance.Node.RPC.MaxMessageSize ?? 4 * 1024 * 1024; // The default maximum uncompressed size (in bytes) for inbound messages. Defaults to 4 MiB.
            instance.Node.RPC.MaxHeaderListSize = Config.Instance.Node.RPC.MaxHeaderListSize ?? 8192; //he default maximum size (in bytes) for inbound header/trailer.
            instance.Node.RPC.MinEffectiveConnection = Config.Instance.Node.RPC.MinEffectiveConnection ?? 1;
            #endregion

            #region Block
            instance.Block.NeedSyncCheck = Config.Instance.Block.NeedSyncCheck ?? false;
            instance.Block.MaintenanceTimeInterval = Config.Instance.Block.MaintenanceTimeInterval ?? 21600000;
            instance.Block.ProposalExpireTime = Config.Instance.Block.ProposalExpireTime ?? 259200000;
            instance.Block.CheckFrozenTime = Config.Instance.Block.CheckFrozenTime ?? 1;
            #endregion

            #region Committee
            instance.Committe.AllowCreationOfContracts = Config.Instance.Committe.AllowCreationOfContracts ?? 0;
            instance.Committe.AllowMultiSign = Config.Instance.Committe.AllowMultiSign ?? 0;
            instance.Committe.AllowAdaptiveEnergy = Config.Instance.Committe.AllowAdaptiveEnergy ?? 0;
            instance.Committe.AllowDelegateResource = Config.Instance.Committe.AllowDelegateResource ?? 0;
            instance.Committe.AllowSameTokenName = Config.Instance.Committe.AllowSameTokenName ?? 0;
            instance.Committe.AllowVMTransferTC10 = Config.Instance.Committe.AllowVMTransferTC10 ?? 0;
            instance.Committe.AllowVMConstantinople = Config.Instance.Committe.AllowVMConstantinople ?? 0;
            instance.Committe.AllowProtoFilterNum = Config.Instance.Committe.AllowProtoFilterNum ?? 0;
            instance.Committe.AllowAccountStateRoot = Config.Instance.Committe.AllowAccountStateRoot ?? 0;
            #endregion

            #region Transaction
            instance.Transaction.ReferenceBlock = StringHelper.IsNotNullOrEmpty(Config.Instance.Transaction.ReferenceBlock) ?
                Config.Instance.Transaction.ReferenceBlock : "head";
            instance.Transaction.ExpireTimeInMillis =
                Config.Instance.Transaction.ExpireTimeInMillis != null ?
                (
                    Config.Instance.Transaction.ExpireTimeInMillis > 0 ?
                    Config.Instance.Transaction.ExpireTimeInMillis : DefineParameter.TRANSACTION_DEFAULT_EXPIRATION_TIME
                ) : DefineParameter.TRANSACTION_DEFAULT_EXPIRATION_TIME;
            #endregion
        }

        public string GetOutputDirectoryByDBName(string db_name)
        {
            string path = Storage.GetPathByDbName(db_name);
            if (StringHelper.IsNotNullOrEmpty(path))
                return path;
            return GetOutputDirectory();
        }

        public string GetOutputDirectory()
        {
            if (this.output_directory.Equals("") || this.output_directory.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                return this.output_directory + Path.DirectorySeparatorChar;
            }
            return this.output_directory;
        }
        #endregion
    }
}
