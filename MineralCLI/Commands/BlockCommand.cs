using Google.Protobuf;
using Mineral;
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
        /// <summary>
        /// Get block information
        /// </summary>
        /// <param name="parameters">
        /// Parameter Index
        /// [0] : Command
        /// [1] : Block number (optional)
        /// </param>
        /// <returns></returns>
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

            try
            {
                BlockExtention block = BlockExtention.Parser.ParseFrom(receive["result"].ToObject<byte[]>());
                if (receive.TryGetValue("error", out JToken value))
                {
                    OutputErrorMessage(value["code"].ToObject<int>(), value["message"].ToObject<string>());
                    return true;
                }

                Console.WriteLine(PrintUtil.PrintBlockExtention(block));
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message + "\n\n" + e.StackTrace);
            }

            return true;
        }

        /// <summary>
        /// Get Latest block informattion
        /// </summary>
        /// <param name="parameters">
        /// Parameter Index
        /// [0] : Command
        /// </param>
        /// <returns></returns>
        public static bool GetBlockByLatestNum(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <path>\n", RpcCommandType.GetBlockByLatestNum) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 1)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                JObject receive = SendCommand(RpcCommandType.GetBlockByLatestNum, new JArray() { });

                BlockExtention block = BlockExtention.Parser.ParseFrom(receive["result"].ToObject<byte[]>());
                if (receive.TryGetValue("error", out JToken value))
                {
                    OutputErrorMessage(value["code"].ToObject<int>(), value["message"].ToObject<string>());
                    return true;
                }

                Console.WriteLine(PrintUtil.PrintBlockExtention(block));
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message + "\n\n" + e.StackTrace);
            }

            return true;
        }

        /// <summary>
        /// Get block by id
        /// </summary>
        /// <param name="parameters">
        /// Parameter Index
        /// [0] : Command
        /// [1] : Block Id
        /// </param>
        /// <returns></returns>
        public static bool GetBlockById(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <path>\n", RpcCommandType.GetBlockById) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 2)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                JObject receive = SendCommand(RpcCommandType.GetBlockById, new JArray() { parameters[1] });

                BlockExtention block = BlockExtention.Parser.ParseFrom(receive["result"].ToObject<byte[]>());
                if (receive.TryGetValue("error", out JToken value))
                {
                    OutputErrorMessage(value["code"].ToObject<int>(), value["message"].ToObject<string>());
                    return true;
                }

                Console.WriteLine(PrintUtil.PrintBlockExtention(block));
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message + "\n\n" + e.StackTrace);
            }

            return true;
        }

        /// <summary>
        /// Get Blocks
        /// </summary>
        /// <param name="parameters">
        /// Parameter Index
        /// [0] : Command
        /// [1] : Start block number
        /// [2] : Block limit count
        /// </param>
        /// <returns></returns>
        public static bool GetBlockByLimitNext(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <path>\n", RpcCommandType.GetBlockByLimitNext) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 1)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                BlockLimit limit = new BlockLimit();
                limit.StartNum = long.Parse(parameters[1]);
                limit.EndNum = long.Parse(parameters[2]);

                JObject receive = SendCommand(RpcCommandType.GetBlockByLimitNext, new JArray() { limit.ToByteArray() });
                if (receive.TryGetValue("error", out JToken value))
                {
                    OutputErrorMessage(value["code"].ToObject<int>(), value["message"].ToObject<string>());
                    return true;
                }

                BlockListExtention blocks = BlockListExtention.Parser.ParseFrom(receive["result"].ToObject<byte[]>());

                Console.WriteLine(PrintUtil.PrintBlockListExtention(blocks));
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message + "\n\n" + e.StackTrace);
            }

            return true;
        }
    }
}
