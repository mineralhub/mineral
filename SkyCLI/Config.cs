using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SkyCLI
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

        public static readonly string Version = "1.0";
        [JsonProperty("block_version")]
        public static double BlockVersion { get; set; }
        [JsonProperty("network")]
        public static NetworkInfo Network { get; set; }

        public static string GetVersion()
        {
            return "Sky CLI " + Version;
        }

        public static bool Initialize()
        {
            bool result = false;

            string path = "./config.json";
            if (result = File.Exists(path))
            {
                JObject jobj = JObject.Parse(File.ReadAllText("./config.json"));

                BlockVersion = jobj["block_version"].Value<double>();

                JToken net = jobj["network"];
                Network = new NetworkInfo
                {
                    ListenAddress = net["listen_address"].ToString(),
                    TcpPort = net["tcp_port"].Value<ushort>(),
                    WsPort = net["ws_port"].Value<ushort>(),
                    RpcPort = net["rpc_port"].Value<ushort>()
                };
            }
            else
            {
                Console.WriteLine("Not found \"config.json\" file.");
            }

            return result;
        }
    }
}
