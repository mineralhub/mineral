using Mineral.Core.Net.RpcHandler;
using MineralCLI.Util;
using Newtonsoft.Json.Linq;
using Protocol;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
