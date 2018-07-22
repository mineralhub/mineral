using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

using Newtonsoft.Json.Linq;

namespace Sky
{
	public class NetworkConfig
	{
		public string ListenAddress;
		public ushort TcpPort;
		public ushort WsPort;
		public string[] SeedList;
	}

	public class BlockConfig
	{
		public int NextBlockTimeSec;
	}

	public class UserConfig
	{
		public byte[] PrivateKey;
		public bool Witness;
	}

	public class DelegateConfig
	{
		public string Name;
		public UInt160 Address;
	}

	public class AccountConfig
	{
		public UInt160 Address;
		public Fixed8 Balance;
	}

	public class GenesisBlockConfig
	{
		public List<AccountConfig> Accounts;
		public List<DelegateConfig> Delegates;
		public int TimeStamp;
	}

	public class Config
	{
		public static NetworkConfig Network;
		public static BlockConfig Block;
		public static UserConfig User;
		public static GenesisBlockConfig GenesisBlock;
		public static byte AddressVersion = 0;
		public static byte StateVersion = 0;

		public const int ProtocolVersion = 0;
		public const int ConnectPeerMax = 10;
		public const int WaitPeerMax = 20;
		public const uint MagicNumber = 16;
		public static Fixed8 DefaultFee = Fixed8.One;
		public static Fixed8 RegisterDelegateFee = Fixed8.One * 10000;
		public static Fixed8 VoteFee = Fixed8.One;
		public static Fixed8 BlockReward = Fixed8.One * 250;

		public static uint Nonce = (uint)(new Random().Next());
		public static string[] SeedList { get; private set; }
		public static HashSet<IPAddress> LocalAddresses { get; private set; }

		public static void Initialize()
		{
			try
			{
				JObject jobj = JObject.Parse(File.ReadAllText("./config.json"));
				AddressVersion = jobj["address_version"].Value<byte>();
				StateVersion = jobj["state_version"].Value<byte>();

				// Network
				JToken net = jobj["network"];
				Network = new NetworkConfig();
				Network.ListenAddress = net["listen_address"].Value<string>();
				Network.TcpPort = net["tcp_port"].Value<ushort>();
				Network.WsPort = net["ws_port"].Value<ushort>();
				Network.SeedList = net["seed_list"].Values<string>().ToArray();

				LocalAddresses = new HashSet<IPAddress>();
				if (!string.IsNullOrEmpty(Network.ListenAddress))
					LocalAddresses.Add(IPAddress.Parse(Network.ListenAddress));

				// Block
				JToken block = jobj["block"];
				Block = new BlockConfig();
				Block.NextBlockTimeSec = block["next_block_time_sec"].Value<int>();

				// User
				JToken user = jobj["user"];
				User = new UserConfig();
				User.PrivateKey = user["private_key"].Value<string>().HexToBytes();
				User.Witness = user["witness"].Value<bool>();

				// GenesisBlock
				JToken genesisBlock = jobj["genesisBlock"];
				GenesisBlock = new GenesisBlockConfig();
				GenesisBlock.Accounts = new List<AccountConfig>();
				GenesisBlock.Delegates = new List<DelegateConfig>();
				foreach (var v in genesisBlock["account"].ToArray())
				{
					AccountConfig conf = new AccountConfig();
					conf.Address = Wallets.WalletAccount.ToAddressHash(v["address"].Value<string>());
					if (Fixed8.TryParse(v["balance"].ToString(), out conf.Balance) == false)
						throw new FormatException();
					GenesisBlock.Accounts.Add(conf);
				}
				foreach (var v in genesisBlock["delegate"].ToArray())
				{
					DelegateConfig conf = new DelegateConfig();
					conf.Name = v["name"].Value<string>();
					conf.Address = Wallets.WalletAccount.ToAddressHash(v["address"].Value<string>());
					GenesisBlock.Delegates.Add(conf);
				}
				GenesisBlock.TimeStamp = genesisBlock["TimeStamp"].Value<int>();
			}
			catch (Exception e)
			{
				Logger.Log("error. check config.json : " + e.Message);
				throw e;
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
}
