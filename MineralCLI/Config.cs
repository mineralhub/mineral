using Newtonsoft.Json;
using System;
using System.IO;

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
                string path = "config.json";
                if (File.Exists(path))
                {
                    using (var file = File.OpenText(path))
                    {
                        instance = JsonConvert.DeserializeObject<Config>(file.ReadToEnd());
                    }
                    result = true;
                }
                else
                {
                    throw new FileNotFoundException(
                        string.Format("Not found {0} file.", path));
                }
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (System.Exception e)
            {
                Console.WriteLine("Json invalid format. " + e.Message);
            }

            return result;
        }
    }
}
