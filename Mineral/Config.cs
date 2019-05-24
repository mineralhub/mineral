using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Mineral.Converter;
using System.Net.NetworkInformation;
using System.Linq;
using Mineral.Utils;
using static Mineral.Core.Config.Arguments.Account;
using Mineral.Core.Config.Arguments;
using Mineral.Common.Stroage.LevelDB;

namespace Mineral
{
    public class Property
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public Options Option { get; set; }
    }

    public class NetConfig
    {
        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class WitnessConfig
    {
        [JsonProperty("local_witness")]
        public List<string> LocalWitness { get; set; }
        [JsonProperty("local_witness_account_address")]
        public string LocalWitnessAccountAddress { get; set; }
        [JsonProperty("local_witness_keystore")]
        public List<string> LocalWitnessKeyStore { get; set; }
    }

    public class StorageConfig
    {
        [JsonProperty("directory")]
        public string Directory { get; set; }
        [JsonProperty("version")]
        public int? Version { get; set; }
        [JsonProperty("engine")]
        public string Engine { get; set; }
        [JsonProperty("sync")]
        public bool? Sync { get; set; }
        [JsonProperty("index_directory")]
        public string IndexDirectory { get; set; }
        [JsonProperty("index_switch")]
        public string IndexSwitch { get; set; }
        [JsonProperty("transaction_history_switch")]
        public string TransactionHistorySwitch { get; set; }
        [JsonProperty("on")]
        public string On { get; set; }
        [JsonProperty("need_to_update_asset")]
        public bool? NeedToUpdateAsset { get; set; }
        [JsonProperty("properties")]
        public List<Property> Properties { get; set; }
    }

    public class SeedNodeConfig
    {
        [JsonProperty("ip_list")]
        public List<string> IpList { get; set; }
    }

    public class GenesisBlockConfig
    {
        [JsonProperty("assets")]
        public List<Account> Assets { get; set; }
        [JsonProperty("witness")]
        public List<Witness> Witnesses { get; set; }
        [JsonProperty("timestamp")]
        public uint Timestamp { get; set; }
        [JsonProperty("parent_hash")]
        [JsonConverter(typeof(JsonUInt160Converter))]
        public UInt256 ParentHash { get; set; }

        public uint Height { get { return 0; } }

        public static GenesisBlockConfig DefaultGenesisBlock
        {
            get
            {
                GenesisBlockConfig block = new GenesisBlockConfig();
                block.Assets = new List<Account>();
                block.Witnesses = new List<Witness>();
                block.ParentHash = UInt256.Zero;
                block.Timestamp = 0;
                return block;
            }
        }
    }

    public class DiscoveryConfig
    {
        [JsonProperty("enable")]
        public bool? Enable { get; set; }
        [JsonProperty("persist")]
        public bool? Persist { get; set; }
        [JsonProperty("bind_ip")]
        public string BindIP { get; set; }
        [JsonProperty("persist")]
        public string ExternalIP { get; set; }
        [JsonProperty("home_node")]
        public bool? HomeNode { get; set; }
    }

    public class P2PConfig
    {
        [JsonProperty("version")]
        public int? Version { get; set; }
        [JsonProperty("ping_interval")]
        public long? PingInterval { get; set; }
    }

    public class HttpConfig
    {
        [JsonProperty("full_node_port")]
        public int? FullNodePort { get; set; }
        [JsonProperty("solidity_port")]
        public int? SolidityPort { get; set; }
    }

    public class RPCConfig
    {
        [JsonProperty("Port")]
        public int? Port { get; set; }
        [JsonProperty("solidity_port")]
        public int? SolidityPort { get; set; }
        [JsonProperty("thread")]
        public int? Thread { get; set; }
        [JsonProperty("max_concurrent_call_per_connection")]
        public int? MaxConcurrentCallPerConnection { get; set; }
        [JsonProperty("flow_control_window")]
        public int? FlowControlWindow { get; set; }
        [JsonProperty("max_connection_idle")]
        public long? MaxConnectionIdle { get; set; }
        [JsonProperty("max_connection_age")]
        public long? MaxConnectionAge { get; set; }
        [JsonProperty("max_message_size")]
        public int? MaxMessageSize { get; set; }
        [JsonProperty("max_header_list_size")]
        public int? MaxHeaderListSize { get; set; }
        [JsonProperty("min_effective_connection")]
        public int? MinEffectiveConnection { get; set; }
    }

    public class BackupConfig
    {
        [JsonProperty("port")]
        public int? Port { get; set; }
        [JsonProperty("priority")]
        public int? Priority { get; set; }
        [JsonProperty("members")]
        public List<string> Members { get; set; }
    }

    public class NodeConfig
    {
        [JsonProperty("trust_node")]
        public string TrustNode { get; set; }
        [JsonProperty("listen_port")]
        public int? ListenPort { get; set; }
        [JsonProperty("connection_timout")]
        public int? ConnectionTimeout { get; set; }
        [JsonProperty("channel_read_timeout")]
        public int? ChannelReadTimeout { get; set; }
        [JsonProperty("validate_sing_thread_num")]
        public int? ValidateSignThreadNum { get; set; }
        [JsonProperty("wallet_extension_api")]
        public bool? WalletExtensionAPI { get; set; }
        [JsonProperty("connect_factor")]
        public double? ConnectFactor { get; set; }
        [JsonProperty("active_connect_factor")]
        public double? ActiveConnectFactor { get; set; }
        [JsonProperty("disconnect_number_factor")]
        public double? DisconnectNumberFactor { get; set; }
        [JsonProperty("max_connect_number_factor")]
        public double? MaxconnectNumberFactor { get; set; }
        [JsonProperty("receive_tcp_min_data_length")]
        public long? ReceiveTcpMinDataLength { get; set; }
        [JsonProperty("is_open_full_tcp_disconnect")]
        public bool? IsOpenFullTcpDisconnect { get; set; }
        [JsonProperty("max_active_nodes")]
        public int? MaxActiveNodes { get; set; }
        [JsonProperty("max_active_nodes_same_ip")]
        public int? MaxActiveNodeSameIP { get; set; }
        [JsonProperty("min_participation_rate")]
        public int? MinParticipationRate { get; set; }
        [JsonProperty("block_produced_timeout")]
        public int? BlockProducedTimeout { get; set; }
        [JsonProperty("solidity_thread")]
        public int? SolidityThread { get; set; }
        [JsonProperty("net_max_trx_per_second")]
        public int? NetMaxTrxPerSecond { get; set; }

        [JsonProperty("active")]
        public List<string> Active { get; set; }
        [JsonProperty("passive")]
        public List<string> Passive { get; set; }
        [JsonProperty("fast_forward")]
        public List<string> FastForward { get; set; }

        [JsonProperty("discovery")]
        public DiscoveryConfig Discovery { get; set; }
        [JsonProperty("backup")]
        public BackupConfig Backup { get; set; }
        [JsonProperty("p2p")]
        public P2PConfig P2P { get; set; }
        [JsonProperty("http")]
        public HttpConfig HTTP { get; set; }
        [JsonProperty("rpc")]
        public RPCConfig RPC { get; set; }
    }

    public class BlockConfig
    {
        [JsonProperty("need_sync_check")]
        public bool? NeedSyncCheck { get; set; }
        [JsonProperty("maintenance_time_interval")]
        public int? MaintenanceTimeInterval { get; set; }
        [JsonProperty("proposal_expire_time")]
        public int? ProposalExpireTime { get; set; }
        [JsonProperty("check_frozen_time")]
        public int? CheckFrozenTime { get; set; }
    }

    public class CommitteConfig
    {
        [JsonProperty("allow_creation_of_contracts")]
        public int? AllowCreationOfContracts { get; set; }
        [JsonProperty("allow_multi_sign")]
        public int? AllowMultiSign { get; set; }
        [JsonProperty("allow_adaptive_energy")]
        public int? AllowAdaptiveEnergy { get; set; }
        [JsonProperty("allow_delegate_resource")]
        public int? AllowDelegateResource { get; set; }
        [JsonProperty("allow_same_token_name")]
        public int? AllowSameTokenName { get; set; }
        [JsonProperty("allow_vm_transfer_tc10")]
        public int? AllowVMTransferTC10 { get; set; }
        [JsonProperty("allow_vm_constantinople")]
        public int? AllowVMConstantinople { get; set; }
        [JsonProperty("allow_proto_filter_num")]
        public int? AllowProtoFilterNum { get; set; }
        [JsonProperty("allow_account_state_root")]
        public int? AllowAccountStateRoot { get; set; }
    }

    public class TransactionConfig
    {
        [JsonProperty("reference_block")]
        public string ReferenceBlock { get; set; }
        [JsonProperty("expire_time_in_millis")]
        public long? ExpireTimeInMillis { get; set; }
    }

    public class EventConfig
    {
        [JsonProperty("contract_parse")]
        public bool? ContractParse { get; set; }
    }



    public class NetworkConfig
    {
        [JsonProperty("listen_address")]
        public string ListenAddress { get; set; }
        [JsonProperty("tcp_port")]
        public ushort TcpPort { get; set; }
        [JsonProperty("ws_port")]
        public ushort WsPort { get; set; }
        [JsonProperty("rpc_port")]
        public ushort RpcPort { get; set; }
        [JsonProperty("seed_list")]
        public string[] SeedList { get; set; }
    }

    //public class BlockConfig
    //{
    //    [JsonProperty("next_block_time_sec")]
    //    public uint NextBlockTimeSec { get; set; }
    //    [JsonProperty("cache_capacity")]
    //    public uint CacheCapacity { get; set; }
    //    [JsonProperty("payload_capacity")]
    //    public uint PayloadCapacity { get; set; }
    //    [JsonProperty("sync_check")]
    //    public bool SyncCheck { get; set; } = true;
    //}

    //public class TransactionConfig
    //{
    //    [JsonProperty("payload_capacity")]
    //    public uint PayloadCapacity { get; set; }
    //}

    //public class DelegateConfig
    //{
    //    [JsonProperty("name")]
    //    public string Name { get; set; }
    //    [JsonProperty("address")]
    //    [JsonConverter(typeof(JsonUInt160Converter))]
    //    public UInt160 Address { get; set; }
    //}

    //public class AccountConfig
    //{
    //    [JsonProperty("address")]
    //    [JsonConverter(typeof(JsonUInt160Converter))]
    //    public UInt160 Address { get; set; }
    //    [JsonProperty("balance")]
    //    [JsonConverter(typeof(JsonFixed8Converter))]
    //    public Fixed8 Balance { get; set; }
    //}

    //public class GenesisBlockConfig
    //{
    //    [JsonProperty("account")]
    //    public List<AccountConfig> Accounts { get; set; }
    //    [JsonProperty("delegate")]
    //    public List<DelegateConfig> Delegates { get; set; }
    //    [JsonProperty("timestamp")]
    //    public uint Timestamp { get; set; }
    //}

    public class Config
    {
        private Config() { }

        [JsonProperty("net")]
        public NetConfig Net { get; set; }
        [JsonProperty("witness")]
        public WitnessConfig Witness { get; set;}
        [JsonProperty("stroage")]
        public StorageConfig Storage { get; set; }
        [JsonProperty("seed_node")]
        public SeedNodeConfig SeedNode { get; set; }
        [JsonProperty("genesis_block")]
        public GenesisBlockConfig GenesisBlock { get; set; }
        [JsonProperty("node")]
        public NodeConfig Node { get; set; }
        [JsonProperty("block")]
        public BlockConfig Block { get; set; }
        [JsonProperty("committe")]
        public CommitteConfig Committe { get; set; }
        [JsonProperty("transaction")]
        public TransactionConfig Transaction { get; set; }
        [JsonProperty("event")]
        public EventConfig Event { get; set; }




        [JsonProperty("block_version")]
        public short BlockVersion { get; set; }
        public short TransactionVersion { get; set; }
        [JsonProperty("address_version")]
        public byte AddressVersion { get; set; }
        [JsonProperty("state_version")]
        public byte StateVersion { get; set; }

        public uint TTLMinute;
        public uint TTLHour;
        public uint TTLDay;
        public uint LockTTL;
        public uint VoteTTL;

        public readonly int ProtocolVersion = 0;
        public readonly int ConnectPeerMax = 10;
        public readonly int WaitPeerMax = 20;
        public readonly uint MagicNumber = 16;
        public readonly int MaxDelegate = 5;
        public readonly uint RoundBlock = 100;
        public readonly int DelegateNameMaxLength = 20;
        public readonly int OtherSignMaxLength = 10;
        public readonly int OtherSignToMaxLength = 10;
        public readonly int TransferToMaxLength = 10;
        public readonly int MaxTransactions = 2000;
        public readonly int VoteMaxLength = 10;
        public readonly int LockRedoTimes = 10;

        [JsonConverter(typeof(JsonFixed8Converter))]
        public Fixed8 DefaultFee = Fixed8.One;
        [JsonConverter(typeof(JsonFixed8Converter))]
        public Fixed8 RegisterDelegateFee = Fixed8.One * 10000;
        [JsonConverter(typeof(JsonFixed8Converter))]
        public Fixed8 VoteFee = Fixed8.One;
        [JsonConverter(typeof(JsonFixed8Converter))]
        public Fixed8 BlockReward = Fixed8.One * 250;

        [JsonProperty("log-level")]
        [JsonConverter(typeof(JsonLogLevelConverter))]
        public LogLevel WriteLogLevel = LogLevel.INFO;

        [JsonProperty("log-console")]
        public bool WriteLogConsole = false;

        public uint Nonce = (uint)(new Random().Next());

        public HashSet<IPAddress> LocalAddresses { get; private set; }

        private static Config instance = null;
        public static Config Instance { get { return instance = instance ?? new Config(); } }

        public bool Initialize(string path)
        {
            bool result = false;

            try
            {
                if (File.Exists(path))
                {
                    using (var file = File.OpenText(path))
                    {
                        instance = JsonConvert.DeserializeObject<Config>(file.ReadToEnd());

                        //instance.TTLMinute = 60 / instance.Block.NextBlockTimeSec;
                        //instance.TTLHour = instance.TTLMinute * 60;
                        //instance.TTLDay = instance.TTLHour * 24;
                        //instance.LockTTL = instance.TTLDay;
                        //instance.VoteTTL = instance.TTLDay;
                        //instance.LocalAddresses = new HashSet<IPAddress>();
                        //instance.LocalAddresses.UnionWith(NetworkInterface.GetAllNetworkInterfaces().SelectMany(p => p.GetIPProperties().UnicastAddresses).Select(p => p.Address.MapToIPv6()));
                        //foreach (string addr in instance.Network.SeedList)
                        //{
                        //    IPAddress iaddr;
                        //    if (IPAddress.TryParse(addr, out iaddr))
                        //        instance.LocalAddresses.Add(iaddr);
                        //}
                    }
                    result = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Logger.WriteConsole = Instance.WriteLogConsole;
            Logger.WriteLogLevel = Instance.WriteLogLevel;

            return result;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public JObject ToJson()
        {
            return JObject.Parse(this.ToString());
        }

        /*
        if (Mineral.Network.UPNP.Discovery() && Mineral.Network.UPNP.Enable)
        {
            LocalAddresses.Add(Mineral.Network.UPNP.GetExternalIP());
        }
        */
    }
}
