using Mineral.Core.Net.RpcHandler;
using Newtonsoft.Json.Linq;
using Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace MineralCLI.Network
{
    public partial class RpcApi
    {
        public static RpcApiResult ListNode(out NodeList nodes)
        {
            nodes = null;

            JObject receive = SendCommand(RpcCommand.Block.GetBlock, new JArray() { });
            if (receive.TryGetValue("error", out JToken value))
            {
                return new RpcApiResult(false, value["code"].ToObject<int>(), value["message"].ToObject<string>());
            }

            nodes = NodeList.Parser.ParseFrom(receive["result"].ToObject<byte[]>());

            return RpcApiResult.Success;
        }
    }
}
