using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MineralCLI
{
    public class Config
    {
        public class NetworkInfo
        {
            [JsonProperty("listen_address")]
            public string ListenAddress { get; set; }
            [JsonProperty("tcp_port")]
            public ushort TcpPort { get; set; }
            [JsonProperty("ws_port")]
            public ushort WsPort { get; set; }
            [JsonProperty("rpc_port")]
            public ushort RpcPort { get; set; }
        }

        public readonly string Version = "1.0";
        [JsonProperty("block_version")]
        public double BlockVersion { get; set; }
        [JsonProperty("network")]
        public NetworkInfo Network { get; set; }

        private static Config instance = null;
        public static Config Instance { get { return instance = instance ?? new Config(); } }


        public string GetVersion()
        {
            return "MINERAL CLI " + Version;
        }

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
                    }
                    result = true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Json invalid format");
            }
            
            return result;
        }
    }
}
