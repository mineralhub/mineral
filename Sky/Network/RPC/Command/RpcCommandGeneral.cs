using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sky.Network.RPC.Command
{
    public partial class RpcProcessCommand
    {
        public static JObject OnGetConfig(object obj, JArray parameters)
        {
            return Config.Instance.ToJson();
        }
    }
}
