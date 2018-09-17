using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using Sky.Core;
using Sky.Wallets;

namespace Sky.Network.RPC.Command
{
    public partial class ProcessCommand
    {
        public delegate JObject ProcessHandler(JArray parameters);
    }
}
