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
                block = BlockChain.Instance.GetBlock(parameters[0].Value<uint>());
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

            uint start = parameters[0].Value<uint>();
            uint end = parameters[1].Value<uint>();
            for (uint i = start; i < end; ++i) 
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
            uint height = 0;
            if (uint.TryParse(parameters[0].ToString(), out height))
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
