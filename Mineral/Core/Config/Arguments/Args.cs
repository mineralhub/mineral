using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Commander.NET;
using Commander.NET.Attributes;
using Mineral.CommandLine;
using Mineral.Common.Overlay.Discover.Node;
using Mineral.Core.Database;
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
        public class VMArgs : VMConfig { }

        public class NodeArgs
        {
            public class DiscoveryArgs : DiscoveryConfig { }
            public class P2PArgs : P2PConfig { }
            public class HttpArgs : HttpConfig { }
            public class RPCArgs : RPCConfig { }
            public class P2pArgs : P2PConfig { }
            public class BackupArgs : BackupConfig { }

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
            public int TcpNettyWorkThreadNum { get; set; }
            public int UdpNettyWorkThreadNum { get; set; }
            public List<Node> Active { get; set; } = new List<Node>();
            public List<Node> Passive { get; set; } = new List<Node>();
            public List<Node> FastForward { get; set; } = new List<Node>();
            public DiscoveryArgs Discovery { get; set; } = new DiscoveryArgs();
            public BackupArgs Backup { get; set; } = new BackupArgs();
            public P2PArgs P2P { get; set; } = new P2PArgs();
            public HttpArgs HTTP { get; set; } = new HttpArgs();
            public RPCArgs RPC { get; set; } = new RPCArgs();
        }


        #region Field
        private static Args instance = null;

        public static readonly string Version = "1.0.0";

        #region Arguments
        [Parameter("-p", "--private-key", Description = "Private key")]
        private string privatekey = "";

        [Parameter("-v", "--version", Description = "Version")]
        private bool version = false;

        [Parameter("-c", "--config", Description = "Config file")]
        private string config_file = "";

        [Parameter("--es")]
        private bool event_subscribe = false;

        [Parameter("--fast-forward")]
        private bool fast_forward = false;

        [Parameter("--long-running-time")]
        private int long_running_time = 10;

        [Parameter("--min-time-ratio")]
        private double min_time_ratio = 0.0F;

        [Parameter("--max-time-ratio")]
        private double max_time_ratio = 0.0F;

        [Parameter("--save-internaltx")]
        private bool save_internal_tx = false;

        [Parameter("--seed-nodes")]
        private List<String> seed_nodes = new List<string>();

        [Parameter("--storage-db-directory", Description = "Storage db directory")]
        private string storage_directory = "";

        [Parameter("--storage-db-synchronous", Description = "Storage db is synchronous or not.(true or flase)")]
        private string storage_sync = "";

        [Parameter("--contract-parse-switch", Description = "enable contract parses in java-tron or not.(true or flase)")]
        private string contract_parse_switch = "";

        [Parameter("-d", "--output-directory", Description = "Directory")]
        private string output_directory = "";

        [Parameter("storage-index-directory", Description = "Storage index directory")]
        private string storage_index_directory = "";

        [Parameter("storage-index-switch", Description = "Storage index switch.(on or off)")]
        private string storage_index_switch = "";

        [Parameter("--storage-transactionHistory-switch", Description = "Storage transaction history switch.(on or off)")]
        private string storage_transaction_history_switch = "";

        [Parameter("--storage-db-version", Description = "Storage db version.(1 or 2)")]
        private String storage_version = "";

        [Parameter("--support-constant")]
        private bool support_constanct = false;

        [Parameter("-w", "--witness", Description = "Version")]
        private bool witness = false;

        [Parameter("--witness-address", Description = "Witness address")]
        private string witness_address = "";
        #endregion

        private long block_num_Energy_limit = 0;
        private bool is_solidity_node = false;
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
        public GenesisBlockArgs GenesisBlock { get; private set; } = new GenesisBlockArgs();
        public LocalWitness LocalWitness { get; private set; } = new LocalWitness();
        public NodeArgs Node { get; private set; } = new NodeArgs();
        public BlockArgs Block { get; private set; } = new BlockArgs();
        public CommitteArgs Committe { get; private set; } = new CommitteArgs();
        public TransactionArgs Transaction { get; private set; } = new TransactionArgs();
        public VMArgs VM { get; private set; } = new VMArgs();

        public long BlockNumEnergyLimit
        {
            get { return this.block_num_Energy_limit; }
            set { this.block_num_Energy_limit = value; }
        }

        public int LongRunningTime
        {
            get { return this.long_running_time; }
        }

        public bool IsEventSubscribe
        {
            get { return this.event_subscribe; }
        }

        public bool IsWitness
        {
            get { return this.witness; }
            set { this.witness = value; }
        }

        public bool IsSolidityNode
        {
            get { return this.is_solidity_node; }
            set { this.is_solidity_node = value; }
        }

        public bool IsFastForward
        {
            get { return this.fast_forward; }
        }
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
            Logger.Info(string.Format("***************************************************************"));
            Logger.Info(string.Format("\n"));
        }
        #endregion


        #region External Method
        public bool SetParam(string[] args, string config_path)
        {
            CommanderParser<Args> parser = new CommanderParser<Args>();

            try
            {
                instance = parser.Add(args).Parse();
            }
            catch (System.Exception e)
            {
                Logger.Error(e.Message);
                return false;
            }

            if (instance.version)
            {
                Console.WriteLine(Version);
                return false;
            }

            string config_filename = instance.config_file.IsNotNullOrEmpty() ? instance.config_file : config_path;
            if (!Config.Instance.Initialize(config_filename))
            {
                Logger.Error(
                    string.Format("Failed to initialize config. please check config : {0} file",
                                  config_path));

                return false;
            }

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
                    byte[] address = Wallet.Base58ToAddress(instance.witness_address);
                    if (address.IsNotNullOrEmpty())
                    {
                        instance.LocalWitness.SetWitnessAccountAddress(address);
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
            else if (Utils.CollectionUtil.IsNotNullOrEmpty(Config.Instance.Witness.LocalWitness))
            {
                instance.LocalWitness = new LocalWitness();

                List<string> witness_list = new List<string>();
                if (witness_list.Count > 1)
                {
                    Logger.Warning("Local witness count must be one. get the first witness");
                    witness_list = witness_list.GetRange(0, 1);
                }
                instance.LocalWitness.SetPrivateKeys(witness_list);

                if (Config.Instance.Witness.LocalWitnessAccountAddress.IsNotNullOrEmpty())
                {
                    byte[] address = Wallet.Base58ToAddress(Config.Instance.Witness.LocalWitnessAccountAddress);
                    if (address.IsNotNullOrEmpty())
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
            else if (Utils.CollectionUtil.IsNotNullOrEmpty(Config.Instance.Witness.LocalWitnessKeyStore))
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

            if (instance.witness && instance.LocalWitness.GetPrivateKey().IsNullOrEmpty())
            {
                Logger.Warning("local witness null");
            }
            #endregion

            #region Storage
            instance.Storage = new Storage();

            instance.Storage.Sync = CollectionUtil.IsNotNullOrEmpty(instance.storage_sync) ?
                bool.Parse(instance.storage_sync) : Storage.GetSyncFromConfig();

            instance.Storage.ContractParseSwitch = CollectionUtil.IsNotNullOrEmpty(instance.contract_parse_switch) ?
                bool.Parse(instance.contract_parse_switch) : Storage.GetContractParseSwitchFromConfig();

            instance.Storage.Directory = CollectionUtil.IsNotNullOrEmpty(instance.storage_directory) ?
                instance.storage_directory : Storage.GetDirectoryFromConfig();

            instance.Storage.IndexDirectory = CollectionUtil.IsNotNullOrEmpty(instance.storage_index_directory) ?
                instance.storage_index_directory : Storage.GetIndexDirectoryFromConfig();

            instance.Storage.IndexSwitch = CollectionUtil.IsNotNullOrEmpty(instance.storage_index_switch) ?
                instance.storage_index_switch : Storage.GetIndexSwitchFromConfig();

            instance.Storage.IndexDirectory = CollectionUtil.IsNotNullOrEmpty(instance.storage_transaction_history_switch) ?
                instance.storage_transaction_history_switch : Storage.GetTransactionHistorySwitchFromConfig();

            instance.Storage.NeedToUpdateAsset = Config.Instance.Storage?.NeedToUpdateAsset ?? true;

            instance.Seed.IpList = Utils.CollectionUtil.IsNotNullOrEmpty(instance.seed_nodes) ?
                instance.seed_nodes : Config.Instance.SeedNode.IpList;

            instance.Storage.SetPropertyFromConfig();
            #endregion


            #region Genesis block
            if (Config.Instance.GenesisBlock != null)
            {
                if (Config.Instance.GenesisBlock.Assets != null)
                {
                    instance.GenesisBlock.Assets = Config.Instance.GenesisBlock.Assets;
                    AccountStore.SetAccount(Config.Instance.GenesisBlock);
                }
                else
                {
                    throw new ArgumentNullException("Missed to assets in genesis_block");
                }

                if (Config.Instance.GenesisBlock.Witnesses != null)
                {
                    instance.GenesisBlock.Witnesses = Config.Instance.GenesisBlock.Witnesses;
                }
                else
                {
                    throw new ArgumentNullException("Missed to witness in genesis_block");
                }

                instance.GenesisBlock.Timestamp = Config.Instance.GenesisBlock.Timestamp;
                instance.GenesisBlock.ParentHash = Config.Instance.GenesisBlock.ParentHash;
            }
            else
            {
                instance.GenesisBlock = (GenesisBlockArgs)GenesisBlockConfig.DefaultGenesisBlock;
            }
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
            instance.Node.TcpNettyWorkThreadNum = Config.Instance.Node.TcpNettyWorkThreadNum ?? 0;
            instance.Node.UdpNettyWorkThreadNum = Config.Instance.Node.UdpNettyWorkThreadNum ?? 1;

            instance.Node.Active = Config.Instance.Node.Active?.Select(uri => Mineral.Common.Overlay.Discover.Node.Node.InstanceOf(uri)).ToList();
            instance.Node.Passive = Config.Instance.Node.Passive?.Select(uri => Mineral.Common.Overlay.Discover.Node.Node.InstanceOf(uri)).ToList();
            instance.Node.FastForward = Config.Instance.Node.FastForward?.Select(uri => Mineral.Common.Overlay.Discover.Node.Node.InstanceOf(uri)).ToList();

            instance.Node.Discovery.Enable = Config.Instance.Node.Discovery?.Enable ?? false;
            instance.Node.Discovery.Persist = Config.Instance.Node.Discovery?.Persist ?? false;

            instance.Node.Discovery.BindIP = CollectionUtil.IsNotNullOrEmpty(Config.Instance.Node.Discovery.BindIP) ?
                Config.Instance.Node.Discovery.BindIP : "0.0.0.0";
            instance.Node.Discovery.ExternalIP = CollectionUtil.IsNotNullOrEmpty(Config.Instance.Node.Discovery.ExternalIP) ?
                Config.Instance.Node.Discovery.ExternalIP : "0.0.0.0";

            instance.Node.Discovery.HomeNode = Config.Instance.Node.Discovery?.Persist ?? false;

            instance.Node.Backup.Port = Config.Instance.Node.Backup?.Port ?? 10001;
            instance.Node.Backup.Priority = Config.Instance.Node.Backup?.Priority ?? 0;
            instance.Node.Backup.Members = Config.Instance.Node.Backup?.Members?.Select(member => member).ToList();

            instance.Node.P2P.Version = Config.Instance.Node.P2P?.Version ?? 0;
            instance.Node.P2P.PingInterval = Config.Instance.Node.P2P?.PingInterval ?? 0;

            instance.Node.HTTP.FullNodePort = Config.Instance.Node.HTTP?.FullNodePort ?? 11265;
            instance.Node.HTTP.SolidityPort = Config.Instance.Node.HTTP?.SolidityPort ?? 11256;

            instance.Node.RPC.Port = Config.Instance.Node.RPC?.Port ?? 11275;
            instance.Node.RPC.SolidityPort = Config.Instance.Node.RPC?.SolidityPort ?? 11276;
            instance.Node.RPC.MaxConcurrentCallPerConnection = Config.Instance.Node.RPC?.MaxConcurrentCallPerConnection ?? int.MaxValue;
            instance.Node.RPC.FlowControlWindow = Config.Instance.Node.RPC?.FlowControlWindow ?? 1048576;
            instance.Node.RPC.MaxConnectionIdle = Config.Instance.Node.RPC?.MaxConnectionIdle ?? long.MaxValue;

            instance.Node.RPC.MaxConnectionAge = Config.Instance.Node.RPC?.MaxConnectionAge ?? long.MaxValue;
            instance.Node.RPC.MaxMessageSize = Config.Instance.Node.RPC?.MaxMessageSize ?? 4 * 1024 * 1024; // The default maximum uncompressed size (in bytes) for inbound messages. Defaults to 4 MiB.
            instance.Node.RPC.MaxHeaderListSize = Config.Instance.Node.RPC?.MaxHeaderListSize ?? 8192; //he default maximum size (in bytes) for inbound header/trailer.
            instance.Node.RPC.MinEffectiveConnection = Config.Instance.Node.RPC?.MinEffectiveConnection ?? 1;
            #endregion

            #region Block
            instance.Block.NeedSyncCheck = Config.Instance.Block.NeedSyncCheck ?? false;
            instance.Block.MaintenanceTimeInterval = Config.Instance.Block.MaintenanceTimeInterval ?? 21600000;
            instance.Block.ProposalExpireTime = Config.Instance.Block.ProposalExpireTime ?? 259200000;
            instance.Block.CheckFrozenTime = Config.Instance.Block.CheckFrozenTime ?? 1;
            #endregion

            #region Committee
            instance.Committe.AllowCreationOfContracts = Config.Instance.Committe?.AllowCreationOfContracts ?? 0;
            instance.Committe.AllowMultiSign = Config.Instance.Committe?.AllowMultiSign ?? 0;
            instance.Committe.AllowAdaptiveEnergy = Config.Instance.Committe?.AllowAdaptiveEnergy ?? 0;
            instance.Committe.AllowDelegateResource = Config.Instance.Committe?.AllowDelegateResource ?? 0;
            instance.Committe.AllowSameTokenName = Config.Instance.Committe?.AllowSameTokenName ?? 0;
            instance.Committe.AllowVMTransferTC10 = Config.Instance.Committe?.AllowVMTransferTC10 ?? 0;
            instance.Committe.AllowVMConstantinople = Config.Instance.Committe?.AllowVMConstantinople ?? 0;
            instance.Committe.AllowProtoFilterNum = Config.Instance.Committe?.AllowProtoFilterNum ?? 0;
            instance.Committe.AllowAccountStateRoot = Config.Instance.Committe?.AllowAccountStateRoot ?? 0;
            #endregion

            #region Transaction
            instance.Transaction.ReferenceBlock = CollectionUtil.IsNotNullOrEmpty(Config.Instance.Transaction.ReferenceBlock) ?
                Config.Instance.Transaction.ReferenceBlock : "head";
            instance.Transaction.ExpireTimeInMillis =
                Config.Instance.Transaction.ExpireTimeInMillis != null ?
                (
                    Config.Instance.Transaction.ExpireTimeInMillis > 0 ?
                    Config.Instance.Transaction.ExpireTimeInMillis : DefineParameter.TRANSACTION_DEFAULT_EXPIRATION_TIME
                ) : DefineParameter.TRANSACTION_DEFAULT_EXPIRATION_TIME;
            #endregion

            #region VM
            instance.VM.VMTrace = Config.Instance.VM.VMTrace ?? false;
            instance.VM.SaveInternalTx = Config.Instance.VM.SaveInternalTx ?? this.save_internal_tx;
            instance.VM.SupportConstant = Config.Instance.VM.SupportConstant ?? this.support_constanct;
            instance.VM.MinTimeRatio = Config.Instance.VM.MinTimeRatio ?? this.min_time_ratio;
            instance.VM.MaxTimeRatio = Config.Instance.VM.MaxTimeRatio ?? this.max_time_ratio;
            #endregion

            return true;
        }

        public string GetOutputDirectoryByDBName(string db_name)
        {
            string path = Storage.GetPathByDbName(db_name);
            if (CollectionUtil.IsNotNullOrEmpty(path))
                return path;
            return GetOutputDirectory();
        }

        public string GetOutputDirectory()
        {
            if (!this.output_directory.Equals("") && !this.output_directory.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                return this.output_directory + Path.DirectorySeparatorChar;
            }
            return this.output_directory;
        }
        #endregion
    }
}
