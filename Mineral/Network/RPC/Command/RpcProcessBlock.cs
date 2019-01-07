using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using Mineral.Core;
using Mineral.Core.Transactions;
using Mineral.Utils;

namespace Mineral.Network.RPC.Command
{
    public partial class RpcProcessCommand
    {
        public static JObject OnGetBlock(object obj, JArray parameters)
        {
            Block block = null;
            if (parameters[0].Type == JTokenType.Integer)
                block = BlockChain.Instance.GetBlock(parameters[0].Value<int>());
            else
                block = BlockChain.Instance.GetBlock(UInt256.FromHexString(parameters[0].Value<string>()));
            BlockHeader nextHeader = BlockChain.Instance.GetNextHeader(block.Hash);
            JObject json = block.ToJson();
            json["nextblockhash"] = nextHeader == null ? "" : nextHeader.Hash.ToString();

            return json;
        }

        public static JObject OnGetBlocks(object obj, JArray parameters)
        {
            JObject json = new JObject();
            JObject jobj = null;
            JArray jarr = new JArray();
            Block prevBlock = null, currBlock = null;

            int start = parameters[0].Value<int>();
            int end = parameters[1].Value<int>();
            for (int i = start; i < end; ++i) 
            {
                prevBlock = currBlock;
                currBlock = BlockChain.Instance.GetBlock(i);
                if (prevBlock != null)
                {
                    jobj = prevBlock.ToJson();
                    jobj["nextblockhash"] = currBlock == null ? "" : currBlock.Hash.ToString();
                    jarr.Add(jobj);
                }

                if (currBlock == null)
                    break;
            }

            if (currBlock != null)
            {
                BlockHeader nextHeader = BlockChain.Instance.GetNextHeader(currBlock.Hash);
                jobj = currBlock.ToJson();
                jobj["nextblockhash"] = nextHeader == null ? "" : nextHeader.Hash.ToString();
                jarr.Add(jobj);
            }
            json["blocks"] = jarr;

            return json;
        }

        public static JObject OnGetBlockHash(object obj, JArray parameters)
        {
            JObject json = new JObject();
            int height = -1;
            if (int.TryParse(parameters[0].ToString(), out height))
            {
                Block block = BlockChain.Instance.GetBlock(height);
                json["hash"] = block.Hash.ToString();
            }

            return json;
        }

        public static JObject OnGetHeight(object obj, JArray parameters)
        {
            JObject json = new JObject();
            json["blockheight"] = BlockChain.Instance.CurrentBlockHeight;
            json["headerheight"] = BlockChain.Instance.CurrentHeaderHeight;
            return json;
        }

        public static JObject OnGetCurrentBlockHash(object obj, JArray parameters)
        {
            JObject json = new JObject();
            json["hash"] = BlockChain.Instance.CurrentBlockHash.ToString();
            return json;
        }

        public static JObject OnGetTransaction(object obj, JArray parameters)
        {
            Transaction tx = BlockChain.Instance.GetTransaction(UInt256.FromHexString(parameters[0].Value<string>()));
            return tx.ToJson();
        }
    }
}
