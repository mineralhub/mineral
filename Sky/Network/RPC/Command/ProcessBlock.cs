using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using Sky.Core;

namespace Sky.Network.RPC.Command
{
    public partial class ProcessCommand
    {
        public static JObject OnGetBlock(JArray parameters)
        {
            Block block = null;
            if (parameters[0].Type == JTokenType.Integer)
                block = Blockchain.Instance.GetBlock(parameters[0].Value<int>());
            else
                block = Blockchain.Instance.GetBlock(UInt256.FromHexString(parameters[0].Value<string>()));
            BlockHeader nextHeader = Blockchain.Instance.GetNextHeader(block.Hash);
            JObject json = block.ToJson();
            json["nextblockhash"] = nextHeader.Hash.ToString();

            return json;
        }

        public static JObject OnGetHeight(JArray parameters)
        {
            JObject json = new JObject();
            json["blockheight"] = Blockchain.Instance.CurrentBlockHeight;
            json["headerheight"] = Blockchain.Instance.CurrentHeaderHeight;
            return json;
        }

        public static JObject OnGetCurrentBlockHash(JArray parameters)
        {
            JObject json = new JObject();
            json["hash"] = Blockchain.Instance.CurrentBlockHash.ToString();
            return json;
        }
    }
}
