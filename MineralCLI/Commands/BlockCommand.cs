using Mineral.Core.Net.RpcHandler;
using MineralCLI.Util;
using Newtonsoft.Json.Linq;
using Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace MineralCLI.Commands
{
    public sealed class BlockCommand : BaseCommand
    {
        public static bool GetBlock(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <path>\n", RpcCommandType.GetBlock) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length > 2)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }


            JObject receive = null;
            if (parameters.Length == 1)
            {
                Console.WriteLine("Get current block.");
                receive = SendCommand(RpcCommandType.GetBlockByLatestNum, new JArray() { });
            }
            else
            {
                if (long.TryParse(parameters[1], out long block_num))
                {
                    Console.WriteLine("Invalid block number");
                    return true;
                }
                receive = SendCommand(RpcCommandType.GetBlock, new JArray() { block_num });
            }

            BlockExtention block = BlockExtention.Parser.ParseFrom(receive["result"].ToObject<byte[]>());
            if (receive.TryGetValue("error", out JToken value))
            {
                OutputErrorMessage(value["code"].ToObject<int>(), value["message"].ToObject<string>());
                return true;
            }

            Console.WriteLine(PrintUtil.PrintBlockExtention(block));

            return true;
        }

        public static bool GetBlockByLatestNum(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <path>\n", RpcCommandType.GetBlock) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 1)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            JObject receive = SendCommand(RpcCommandType.GetBlockByLatestNum, new JArray() { });
            BlockExtention block = BlockExtention.Parser.ParseFrom(receive["result"].ToObject<byte[]>());
            if (receive.TryGetValue("error", out JToken value))
            {
                OutputErrorMessage(value["code"].ToObject<int>(), value["message"].ToObject<string>());
                return true;
            }

            Console.WriteLine(PrintUtil.PrintBlockExtention(block));

            return true;
        }
    }
}
