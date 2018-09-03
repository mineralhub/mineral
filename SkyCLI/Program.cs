using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace SkyCLI
{
    public class NetworkConfig
    {
        public string ListenAddress;
        public ushort TcpPort;
        public ushort WsPort;
        public ushort RpcPort;
        public string[] SeedList;
    }

    public class Config
    {
        public const string config_file = @"./config.json";
        public static NetworkConfig Network;
        public static bool Initialzie()
        {
            if (!File.Exists(config_file))
            {
                Console.WriteLine(@"Not found 'config.json' file.");
                return false;
            }
            JObject jobj = JObject.Parse(File.ReadAllText("./config.json"));

            JToken net = jobj["network"];
            Network = new NetworkConfig();
            Network.ListenAddress = net["listen_address"].Value<string>();
            Network.TcpPort = net["tcp_port"].Value<ushort>();
            Network.WsPort = net["ws_port"].Value<ushort>();
            Network.RpcPort = net["rpc_port"].Value<ushort>();
            Network.SeedList = net["seed_list"].Values<string>().ToArray();

            return true;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Initialize(args);
            Shell.ConsoleService service = new Shell.ConsoleService();
            service.Run(args);
        }

        private static void Initialize(string[] args)
        {
            Config.Initialzie();
        }
    }
}
