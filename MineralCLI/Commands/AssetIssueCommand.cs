using Google.Protobuf;
using Mineral.Core;
using Mineral.Core.Net.RpcHandler;
using MineralCLI.Api;
using MineralCLI.Util;
using Newtonsoft.Json.Linq;
using Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace MineralCLI.Commands
{
    public class AssetIssueCommand : BaseCommand
    {
        /// <summary>
        /// Get infomation asset issue by account 
        /// </summary>
        /// <param name="parameters">
        /// Parameter Index
        /// [0] : Command
        /// [1] : asset issue address
        /// </param>
        /// <returns></returns>
        public static bool AssetIssueByAccount(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <address> <amount>\n", RpcCommandType.AssetIssueByAccount) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 3)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            JObject receive = SendCommand(RpcCommandType.AssetIssueByAccount, new JArray() { parameters[1] });
            if (receive.TryGetValue("error", out JToken value))
            {
                OutputErrorMessage(value["code"].ToObject<int>(), value["message"].ToObject<string>());
                return true;
            }

            AssetIssueList asset_issue_list = AssetIssueList.Parser.ParseFrom(receive["result"].ToObject<byte[]>());
            Console.WriteLine(PrintUtil.PrintAssetIssueList(asset_issue_list));

            return true;
        }

        /// <summary>
        /// Get infomation asset issue by id
        /// </summary>
        /// <param name="parameters">
        /// Parameter Index
        /// [0] : Command
        /// [1] : Asset issue id
        /// </param>
        /// <returns></returns>
        public static bool AssetIssueById(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <address> <amount>\n", RpcCommandType.AssetIssueById) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 3)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            JObject receive = SendCommand(RpcCommandType.AssetIssueById, new JArray() { parameters[1] });
            if (receive.TryGetValue("error", out JToken value))
            {
                OutputErrorMessage(value["code"].ToObject<int>(), value["message"].ToObject<string>());
                return true;
            }

            AssetIssueContract asset_issue_contract = AssetIssueContract.Parser.ParseFrom(receive["result"].ToObject<byte[]>());
            Console.WriteLine(PrintUtil.PrintAssetIssue(asset_issue_contract));

            return true;
        }

        /// <summary>
        /// Get infomation asset issue list by name
        /// </summary>
        /// <param name="parameters">
        /// Parameter Index
        /// [0] : Command
        /// [1] : Asset issue name
        /// </param>
        /// <returns></returns>
        public static bool AssetIssueByName(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <address> <amount>\n", RpcCommandType.AssetIssueById) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 3)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            JObject receive = SendCommand(RpcCommandType.AssetIssueByName, new JArray() { parameters[1] });
            if (receive.TryGetValue("error", out JToken value))
            {
                OutputErrorMessage(value["code"].ToObject<int>(), value["message"].ToObject<string>());
                return true;
            }

            AssetIssueContract asset_issue_contract = AssetIssueContract.Parser.ParseFrom(receive["result"].ToObject<byte[]>());
            Console.WriteLine(PrintUtil.PrintAssetIssue(asset_issue_contract));

            return true;
        }

        /// <summary>
        /// Get infomation asset issue by name
        /// </summary>
        /// <param name="parameters">
        /// Parameter Index
        /// [0] : Command
        /// [1] : Asset issue name
        /// </param>
        /// <returns></returns>
        public static bool AssetIssueListByName(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <address> <amount>\n", RpcCommandType.AssetIssueById) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 3)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            JObject receive = SendCommand(RpcCommandType.AssetIssueByName, new JArray() { parameters[1] });
            if (receive.TryGetValue("error", out JToken value))
            {
                OutputErrorMessage(value["code"].ToObject<int>(), value["message"].ToObject<string>());
                return true;
            }

            AssetIssueList asset_issue_list = AssetIssueList.Parser.ParseFrom(receive["result"].ToObject<byte[]>());
            Console.WriteLine(PrintUtil.PrintAssetIssueList(asset_issue_list));

            return true;
        }

        /// <summary>
        /// Transfer asset
        /// </summary>
        /// <param name="parameters">
        /// Parameter Index
        /// [0] : Command
        /// [1] : To address
        /// [2] : Asset name
        /// [3] : Amount
        /// </param>
        /// <returns></returns>
        public static bool TransferAsset(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <address> <amount>\n", RpcCommandType.AssetIssueById) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 4)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            if (!WalletApi.IsLogin)
                return true;

            try
            {
                byte[] to_address = Encoding.UTF8.GetBytes(parameters[1]);
                byte[] asset_name = Encoding.UTF8.GetBytes(parameters[2]);
                byte[] owner_address = Wallet.Base58ToAddress(WalletApi.KeyStore.Address);
                long amount = long.Parse(parameters[3]);

                TransferAssetContract contract = WalletApi.CreateTransferAssetContract(to_address,
                                                                                       owner_address,
                                                                                       asset_name,
                                                                                       amount);

                JObject receive = SendCommand(RpcCommandType.TransferAsset, new JArray { contract.ToByteArray() });
                if (receive.TryGetValue("error", out JToken value))
                {
                    OutputErrorMessage(value["code"].ToObject<int>(), value["message"].ToObject<string>());
                    return true;
                }

                TransactionExtention transaction_extention = TransactionExtention.Parser.ParseFrom(receive["result"].ToObject<byte[]>());
                if (transaction_extention == null || !transaction_extention.Result.Result)
                {
                    Console.WriteLine("Invalid transaction extention data");
                    return true;
                }

                if (transaction_extention.Transaction == null || transaction_extention.Transaction.RawData.Contract.Count == 0)
                {
                    Console.WriteLine("Transaction is empty");
                    return true;
                }

                receive = SendCommand(RpcCommandType.BroadcastTransaction, new JArray() { transaction_extention.Transaction.ToByteArray() });
                Return ret = Return.Parser.ParseFrom(receive["result"].ToObject<byte[]>());

                int retry = 10;
                while (ret.Result == false && ret.Code == Return.Types.response_code.ServerBusy && retry > 0)
                {
                    retry--;
                    receive = SendCommand(RpcCommandType.BroadcastTransaction, new JArray() { transaction_extention.Transaction.ToByteArray() });
                    ret = Return.Parser.ParseFrom(receive["result"].ToObject<byte[]>());
                    Console.WriteLine("Retry broadcast : " + (11 - retry));

                    Thread.Sleep(1000);
                }

                if (ret.Result)
                {
                    Console.WriteLine(
                        string.Format("Send {0} drop to {1} + successful. ", long.Parse(parameters[2]), parameters[1]));
                }
                else
                {
                    Console.WriteLine("Code : " + ret.Code);
                    Console.WriteLine("Message : " + ret.Message);
                    Console.WriteLine(
                        string.Format("Send {0} drop to {1} + failed. ", long.Parse(parameters[2]), parameters[1]));
                }
            }
            catch (System.Exception e)
            {
            }
            
            return true;
        }
    }
}
