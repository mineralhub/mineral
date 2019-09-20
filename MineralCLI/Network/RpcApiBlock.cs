using Google.Protobuf;
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
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public static RpcApiResult GetBlock(long block_num, out BlockExtention block)
        {
            block = null;

            JObject receive = SendCommand(RpcCommandType.GetBlock, new JArray() { block_num });
            if (receive.TryGetValue("error", out JToken value))
            {
                return new RpcApiResult(false, value["code"].ToObject<int>(), value["message"].ToObject<string>());
            }

            block = BlockExtention.Parser.ParseFrom(receive["result"].ToObject<byte[]>());

            return RpcApiResult.Success;
        }

        public static RpcApiResult GetBlockByLatestNum(out BlockExtention block)
        {
            block = null;

            JObject receive = SendCommand(RpcCommandType.GetBlockByLatestNum, new JArray() { });
            if (receive.TryGetValue("error", out JToken value))
            {
                return new RpcApiResult(false, value["code"].ToObject<int>(), value["message"].ToObject<string>());
            }

            block = BlockExtention.Parser.ParseFrom(receive["result"].ToObject<byte[]>());

            return RpcApiResult.Success;
        }

        public static RpcApiResult GetBlockById(string id, out BlockExtention block)
        {
            block = null;

            JObject receive = SendCommand(RpcCommandType.GetBlockById, new JArray() { id });
            if (receive.TryGetValue("error", out JToken value))
            {
                return new RpcApiResult(false, value["code"].ToObject<int>(), value["message"].ToObject<string>());
            }

            return RpcApiResult.Success;
        }

        public static RpcApiResult GetBlockByLimitNext(long start, long end, out BlockListExtention blocks)
        {
            blocks = null;

            BlockLimit limit = new BlockLimit();
            limit.StartNum = start;
            limit.EndNum = end;

            JObject receive = SendCommand(RpcCommandType.GetBlockByLimitNext, new JArray() { limit.ToByteArray() });
            if (receive.TryGetValue("error", out JToken value))
            {
                return new RpcApiResult(false, value["code"].ToObject<int>(), value["message"].ToObject<string>());
            }

            blocks = BlockListExtention.Parser.ParseFrom(receive["result"].ToObject<byte[]>());

            return RpcApiResult.Success;
        }
        #endregion
    }
}
