using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sky.Converter;

namespace Sky
{
    public class ConfigClassAttribute : System.Attribute
    {
    }

    [ConfigClassAttribute]
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

    [ConfigClassAttribute]
    public class BlockConfig
    {
        [JsonProperty("next_block_time_sec")]
        public int NextBlockTimeSec { get; set; }
    }

    [ConfigClassAttribute]
    public class UserConfig
    {
        [JsonProperty("private_key")]
        [JsonConverter(typeof(JsonByteArrayConverter))]
        public byte[] PrivateKey { get; set; }
        [JsonProperty("witness")]
        public bool Witness { get; set; }
    }

    [ConfigClassAttribute]
    public class DelegateConfig
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("address")]
        [JsonConverter(typeof(JsonUInt160Converter))]
        public UInt160 Address { get; set; }
    }

    [ConfigClassAttribute]
    public class AccountConfig
    {
        [JsonProperty("address")]
        [JsonConverter(typeof(JsonUInt160Converter))]
        public UInt160 Address { get; set; }
        [JsonProperty("balance")]
        [JsonConverter(typeof(JsonFixed8Converter))]
        public Fixed8 Balance { get; set; }
    }

    [ConfigClassAttribute]
    public class GenesisBlockConfig
    {
        [JsonProperty("account")]
        public List<AccountConfig> Accounts { get; set; }
        [JsonProperty("delegate")]
        public List<DelegateConfig> Delegates { get; set; }
        [JsonProperty("timestamp")]
        public int Timestamp { get; set; }
    }

    public class Config
    {
        private Config() { }

        [JsonProperty("network")]
        public NetworkConfig Network { get; set; }
        [JsonProperty("block")]
        public BlockConfig Block { get; set; }
        [JsonProperty("user")]
        public UserConfig User { get; set; }
        [JsonProperty("genesisBlock")]
        public GenesisBlockConfig GenesisBlock { get; set; }

        [JsonProperty("block_version")]
        public short BlockVersion { get; set; }
        public short TransactionVersion { get; set; }
        [JsonProperty("address_version")]
        public byte AddressVersion { get; set; }
        [JsonProperty("state_version")]
        public byte StateVersion { get; set; }

        public int TTLMinute;
        public int TTLHour;
        public int TTLDay;
        public int LockTTL;
        public int VoteTTL;

        public readonly int ProtocolVersion = 0;
        public readonly int ConnectPeerMax = 10;
        public readonly int WaitPeerMax = 20;
        public readonly uint MagicNumber = 16;
        public readonly int MaxDelegate = 5;
        public readonly int RoundBlock = 100;
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

        public uint Nonce = (uint)(new Random().Next());
        public string[] SeedList { get; private set; }
        public HashSet<IPAddress> LocalAddresses { get; private set; }

        private static Config instance = null;
        public static Config Instance { get { return instance = instance ?? new Config(); } }

        public bool Initialize()
        {
            bool result = false;

            try
            {
                string path = "./Config.json";
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
                    }

                    result = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return result;


        }

        public string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public JObject ToJson()
        {
            return JObject.Parse(this.ToString());
        }

        /*
        LocalAddresses.UnionWith(NetworkInterface.GetAllNetworkInterfaces().SelectMany(p => p.GetIPProperties().UnicastAddresses).Select(p => p.Address.MapToIPv6()));
        if (Sky.Network.UPNP.Discovery() && Sky.Network.UPNP.Enable)
        {
            LocalAddresses.Add(Sky.Network.UPNP.GetExternalIP());
        }
        */
    }
}
