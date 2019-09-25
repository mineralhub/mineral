using Google.Protobuf;
using Mineral;
using Mineral.Core.Net.RpcHandler;
using MineralCLI.Network;
using MineralCLI.Util;
using Newtonsoft.Json.Linq;
using Protocol;
using System;

namespace MineralCLI.Commands
{
    public sealed class BlockCommand : BaseCommand
    {
        /// <summary>
        /// Get block information
        /// </summary>
        /// <param name="parameters">
        /// Parameter Index
        /// [0] : Block number (optional)
        /// </param>
        /// <returns></returns>
        public static bool GetBlock(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <block number>\n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters != null &&  parameters.Length > 1)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                RpcApiResult result = null;
                BlockExtention block = null;
                if (parameters.Length == 0)
                {
                    Console.WriteLine("Get current block.");
                    result = RpcApi.GetBlockByLatestNum(out block);
                }
                else
                {
                    if (!long.TryParse(parameters[0], out long block_num))
                    {
                        Console.WriteLine("Invalid block number");
                        return true;
                    }
                    result = RpcApi.GetBlock(block_num, out block);
                }

                if (result.Result)
                {
                    Console.WriteLine(PrintUtil.PrintBlockExtention(block));
                }

                OutputResultMessage(command, result.Result, result.Code, result.Message);
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
        /// </param>
        /// <returns></returns>
        public static bool GetBlockByLatestNum(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option]\n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters != null)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                RpcApiResult result = RpcApi.GetBlockByLatestNum(out BlockExtention block);
                if (result.Result)
                {
                    Console.WriteLine(PrintUtil.PrintBlockExtention(block));
                }

                OutputResultMessage(command, result.Result, result.Code, result.Message);
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
        /// [0] : Block Id
        /// </param>
        /// <returns></returns>
        public static bool GetBlockById(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <block id>\n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 1)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                RpcApiResult result = RpcApi.GetBlockById(parameters[0], out BlockExtention block);
                if (result.Result)
                {
                    Console.WriteLine(PrintUtil.PrintBlockExtention(block));
                }

                OutputResultMessage(command, result.Result, result.Code, result.Message);
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
        /// [0] : Start block number
        /// [1] : Block limit count
        /// </param>
        /// <returns></returns>
        public static bool GetBlockByLimitNext(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <start number> <end number>\n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 2)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                long start = long.Parse(parameters[0]);
                long end = long.Parse(parameters[1]);

                RpcApiResult result = RpcApi.GetBlockByLimitNext(start, end, out BlockListExtention blocks);
                if (result.Result)
                {
                    Console.WriteLine(PrintUtil.PrintBlockListExtention(blocks));
                }

                OutputResultMessage(command, result.Result, result.Code, result.Message);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message + "\n\n" + e.StackTrace);
            }

            return true;
        }
    }
}
