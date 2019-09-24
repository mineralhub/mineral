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
        /// [0] : Command
        /// [1] : Block number (optional)
        /// </param>
        /// <returns></returns>
        public static bool GetBlock(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <block number>\n", RpcCommand.Block.GetBlock) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length > 2)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {

                RpcApiResult result = null;
                BlockExtention block = null;
                if (parameters.Length == 1)
                {
                    Console.WriteLine("Get current block.");
                    result = RpcApi.GetBlockByLatestNum(out block);
                }
                else
                {
                    if (!long.TryParse(parameters[1], out long block_num))
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

                OutputResultMessage(RpcCommand.Block.GetBlock, result.Result, result.Code, result.Message);
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
                string.Format("{0} [command option]\n", RpcCommand.Block.GetBlockByLatestNum) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 1)
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

                OutputResultMessage(RpcCommand.Block.GetBlockByLatestNum, result.Result, result.Code, result.Message);
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
                string.Format("{0} [command option] <block id>\n", RpcCommand.Block.GetBlockById) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 2)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                RpcApiResult result = RpcApi.GetBlockById(parameters[1], out BlockExtention block);
                if (result.Result)
                {
                    Console.WriteLine(PrintUtil.PrintBlockExtention(block));
                }

                OutputResultMessage(RpcCommand.Block.GetBlockById, result.Result, result.Code, result.Message);
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
                string.Format("{0} [command option] <start number> <end number>\n", RpcCommand.Block.GetBlockByLimitNext) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 3)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                long start = long.Parse(parameters[1]);
                long end = long.Parse(parameters[2]);

                RpcApiResult result = RpcApi.GetBlockByLimitNext(start, end, out BlockListExtention blocks);
                if (result.Result)
                {
                    Console.WriteLine(PrintUtil.PrintBlockListExtention(blocks));
                }

                OutputResultMessage(RpcCommand.Block.GetBlockByLimitNext, result.Result, result.Code, result.Message);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message + "\n\n" + e.StackTrace);
            }

            return true;
        }
    }
}
