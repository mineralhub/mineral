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
        public static RpcApiResult AssetIssueByAccount(string account, out AssetIssueList asset_issues)
        {
            asset_issues = null;

            JObject receive = SendCommand(RpcCommandType.AssetIssueByAccount, new JArray() { account });
            if (receive.TryGetValue("error", out JToken value))
            {
                return new RpcApiResult(false, value["code"].ToObject<int>(), value["message"].ToObject<string>());
            }

            AssetIssueList asset_issue_list = AssetIssueList.Parser.ParseFrom(receive["result"].ToObject<byte[]>());

            return RpcApiResult.Success;
        }

        public static RpcApiResult AssetIssueById(string id, out AssetIssueContract contract)
        {
            contract = null;

            JObject receive = SendCommand(RpcCommandType.AssetIssueById, new JArray() { id });
            if (receive.TryGetValue("error", out JToken value))
            {
                return new RpcApiResult(false, value["code"].ToObject<int>(), value["message"].ToObject<string>());
            }

            contract = AssetIssueContract.Parser.ParseFrom(receive["result"].ToObject<byte[]>());

            return RpcApiResult.Success;
        }

        public static RpcApiResult AssetIssueByName(string name, out AssetIssueContract contract)
        {
            contract = null;

            JObject receive = SendCommand(RpcCommandType.AssetIssueByName, new JArray() { name });
            if (receive.TryGetValue("error", out JToken value))
            {
                return new RpcApiResult(false, value["code"].ToObject<int>(), value["message"].ToObject<string>());
            }

            contract = AssetIssueContract.Parser.ParseFrom(receive["result"].ToObject<byte[]>());

            return RpcApiResult.Success;
        }

        public static RpcApiResult AssetIssueListByName(string name, out AssetIssueList contracts)
        {
            contracts = null;

            JObject receive = SendCommand(RpcCommandType.AssetIssueListByName, new JArray() { name });
            if (receive.TryGetValue("error", out JToken value))
            {
                return new RpcApiResult(false, value["code"].ToObject<int>(), value["message"].ToObject<string>());
            }

            contracts = AssetIssueList.Parser.ParseFrom(receive["result"].ToObject<byte[]>());

            return RpcApiResult.Success;
        }
        #endregion
    }
}
