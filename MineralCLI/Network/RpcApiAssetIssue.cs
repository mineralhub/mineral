using Google.Protobuf;
using Mineral;
using Mineral.Common.Net.RPC;
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
        public static RpcApiResult CreateAssetIssueContract(byte[] owner_address,
                                                            string name,
                                                            string description,
                                                            string url,
                                                            int tx_num,
                                                            int num,
                                                            int precision,
                                                            int vote_score,
                                                            long total_supply,
                                                            long free_limit,
                                                            long public_free_limit,
                                                            long start_time,
                                                            long end_time,
                                                            Dictionary<long, long> frozen_supply,
                                                            out AssetIssueContract contract)
        {
            contract = new AssetIssueContract();

            if (tx_num <= 0)
                return new RpcApiResult(false, RpcMessage.INVALID_PARAMS, "transaction count must be > 0");

            if (num <= 0)
                return new RpcApiResult(false, RpcMessage.INVALID_PARAMS, "count  must be > 0");

            if (precision < 0)
                return new RpcApiResult(false, RpcMessage.INVALID_PARAMS, "precision  must be >= 0");

            if (total_supply <= 0)
                return new RpcApiResult(false, RpcMessage.INVALID_PARAMS, "total supply must be > 0");

            if (free_limit < 0)
                return new RpcApiResult(false, RpcMessage.INVALID_PARAMS, "free net limit  must be >= 0");

            if (public_free_limit < 0)
                return new RpcApiResult(false, RpcMessage.INVALID_PARAMS, "public free net limit  must be >= 0");

            long now = Helper.CurrentTimeMillis();
            if (start_time <= now)
                return new RpcApiResult(false, RpcMessage.INVALID_PARAMS, "start time must be <= now");

            if (start_time >= end_time)
                return new RpcApiResult(false, RpcMessage.INVALID_PARAMS, "start time mus be <= end time");

            contract.OwnerAddress = ByteString.CopyFrom(owner_address);
            contract.Name = ByteString.CopyFromUtf8(name);
            contract.Description = ByteString.CopyFromUtf8(description);
            contract.Url = ByteString.CopyFromUtf8(url);
            contract.TrxNum = tx_num;
            contract.Num = num;
            contract.Precision = precision;
            contract.VoteScore = vote_score;
            contract.TotalSupply = total_supply;
            contract.FreeAssetNetLimit = free_limit;
            contract.PublicFreeAssetNetLimit = public_free_limit;
            contract.StartTime = start_time;
            contract.EndTime = end_time;

            foreach (var frozen in frozen_supply)
            {
                AssetIssueContract.Types.FrozenSupply entry = new AssetIssueContract.Types.FrozenSupply();
                entry.FrozenAmount = frozen.Key;
                entry.FrozenDays = frozen.Value;
                contract.FrozenSupply.Add(entry);
            }

            return RpcApiResult.Success;
        }


        public static RpcApiResult AssetIssueByAccount(string account, out AssetIssueList asset_issues)
        {
            asset_issues = null;

            JObject receive = SendCommand(RpcCommand.AssetIssue.AssetIssueByAccount, new JArray() { account });
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

            JObject receive = SendCommand(RpcCommand.AssetIssue.AssetIssueById, new JArray() { id });
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

            JObject receive = SendCommand(RpcCommand.AssetIssue.AssetIssueByName, new JArray() { name });
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

            JObject receive = SendCommand(RpcCommand.AssetIssue.AssetIssueListByName, new JArray() { name });
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
