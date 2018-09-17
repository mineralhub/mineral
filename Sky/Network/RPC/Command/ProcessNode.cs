using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Sky.Network.RPC.Command
{
    public partial class ProcessCommand
    {
        public static JObject OnNodeList(JArray parameters)
        {
            JObject json = new JObject();
            return json;
        }
    }
}
