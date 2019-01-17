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

namespace Mineral
{
    public class ConfigClassAttribute : Attribute
    {
    }

    [ConfigClass]
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

    [ConfigClass]
    public class BlockConfig
    {
        [JsonProperty("next_block_time_sec")]
        public uint NextBlockTimeSec { get; set; }
        [JsonProperty("cache_capacity")]
        public uint CacheCapacity { get; set; }
        [JsonProperty("payload_capacity")]
        public uint PayloadCapacity { get; set; }
        [JsonProperty("sync_check")]
        public bool SyncCheck { get; set; } = true;
    }

    [ConfigClass]
    public class TransactionConfig
    {
        [JsonProperty("payload_capacity")]
        public uint PayloadCapacity { get; set; }
    }

    [ConfigClass]
    public class DelegateConfig
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("address")]
        [JsonConverter(typeof(JsonUInt160Converter))]
        public UInt160 Address { get; set; }
    }

    [ConfigClass]
    public class AccountConfig
    {
        [JsonProperty("address")]
        [JsonConverter(typeof(JsonUInt160Converter))]
        public UInt160 Address { get; set; }
        [JsonProperty("balance")]
        [JsonConverter(typeof(JsonFixed8Converter))]
        public Fixed8 Balance { get; set; }
    }

    [ConfigClass]
    public class GenesisBlockConfig
    {
        [JsonProperty("account")]
        public List<AccountConfig> Accounts { get; set; }
        [JsonProperty("delegate")]
        public List<DelegateConfig> Delegates { get; set; }
        [JsonProperty("timestamp")]
        public uint Timestamp { get; set; }
    }

    public class Config
    {
        private Config() { }

        [JsonProperty("network")]
        public NetworkConfig Network { get; set; }
        [JsonProperty("block")]
        public BlockConfig Block { get; set; }
        [JsonProperty("transaction")]
        public TransactionConfig Transaction { get; set; }
        [JsonProperty("genesis_block")]
        public GenesisBlockConfig GenesisBlock { get; set; }

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

                        instance.TTLMinute = 60 / instance.Block.NextBlockTimeSec;
                        instance.TTLHour = instance.TTLMinute * 60;
                        instance.TTLDay = instance.TTLHour * 24;
                        instance.LockTTL = instance.TTLDay;
                        instance.VoteTTL = instance.TTLDay;
                        instance.LocalAddresses = new HashSet<IPAddress>();
                        instance.LocalAddresses.UnionWith(NetworkInterface.GetAllNetworkInterfaces().SelectMany(p => p.GetIPProperties().UnicastAddresses).Select(p => p.Address.MapToIPv6()));
                        foreach (string addr in instance.Network.SeedList)
                        {
                            IPAddress iaddr;
                            if (IPAddress.TryParse(addr, out iaddr))
                                instance.LocalAddresses.Add(iaddr);
                        }
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
