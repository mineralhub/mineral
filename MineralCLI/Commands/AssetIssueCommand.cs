using Mineral.Core;
using Mineral.Core.Net.RpcHandler;
using MineralCLI.Network;
using MineralCLI.Util;
using Newtonsoft.Json.Linq;
using Protocol;
using System;
using System.Text;
using static Protocol.Transaction.Types.Contract.Types;

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
                string.Format("{0} [command option] <asset issue address>\n", RpcCommandType.AssetIssueByAccount) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 2)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                string address = parameters[1];
                RpcApiResult result = RpcApi.AssetIssueByAccount(address, out AssetIssueList asset_issues);

                if (result.Result)
                {
                    Console.WriteLine(PrintUtil.PrintAssetIssueList(asset_issues));
                }

                OutputResultMessage(RpcCommandType.AssetIssueByAccount, result.Result, result.Code, result.Message);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message + "\n\n" + e.StackTrace);
            }

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
                string.Format("{0} [command option] <asset issue id>\n", RpcCommandType.AssetIssueById) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 2)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                string id = parameters[1];
                RpcApiResult result = RpcApi.AssetIssueById(id, out AssetIssueContract contract);
                if (result.Result)
                {
                    Console.WriteLine(PrintUtil.PrintAssetIssue(contract));
                }

                OutputResultMessage(RpcCommandType.AssetIssueById, result.Result, result.Code, result.Message);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message + "\n\n" + e.StackTrace);
            }

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
                string.Format("{0} [command option] <asset issue name>\n", RpcCommandType.AssetIssueByName) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 3)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                string name = parameters[1];
                RpcApiResult result = RpcApi.AssetIssueByName(name, out AssetIssueContract contract);
                if (result.Result)
                {
                    Console.WriteLine(PrintUtil.PrintAssetIssue(contract));
                }

                OutputResultMessage(RpcCommandType.AssetIssueByName, result.Result, result.Code, result.Message);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message + "\n\n" + e.StackTrace);
            }

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
                string.Format("{0} [command option] <asset issue name>\n", RpcCommandType.AssetIssueListByName) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 3)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                string name = parameters[1];
                RpcApiResult result = RpcApi.AssetIssueListByName(name, out AssetIssueList contracts);
                if (result.Result)
                {
                    Console.WriteLine(PrintUtil.PrintAssetIssueList(contracts));
                }

                OutputResultMessage(RpcCommandType.AssetIssueListByName, result.Result, result.Code, result.Message);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message + "\n\n" + e.StackTrace);
            }

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
                string.Format("{0} [command option] <to address> <asset name> <amount>\n", RpcCommandType.TransferAsset) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 4)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            if (!RpcApi.IsLogin)
                return true;

            try
            {
                byte[] to_address = Encoding.UTF8.GetBytes(parameters[1]);
                byte[] asset_name = Encoding.UTF8.GetBytes(parameters[2]);
                byte[] owner_address = Wallet.Base58ToAddress(RpcApi.KeyStore.Address);
                long amount = long.Parse(parameters[3]);


                RpcApiResult result = RpcApi.CreateTransferAssetContract(to_address,
                                                                          owner_address,
                                                                          asset_name,
                                                                          amount,
                                                                          out TransferAssetContract contract);

                TransactionExtention transaction_extention = null;
                if (result.Result)
                {
                    result = RpcApi.CreateTransaction(contract,
                                                      ContractType.TransferAssetContract,
                                                      RpcCommandType.TransferAsset,
                                                      out transaction_extention);
                }

                if (result.Result)
                {
                    result = RpcApi.ProcessTransactionExtention(transaction_extention);
                }

                OutputResultMessage(RpcCommandType.TransferAsset, result.Result, result.Code, result.Message);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message + "\n\n" + e.StackTrace);
            }

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
        public static bool UnfreezeAsset(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <to address> <asset name> <amount>\n", RpcCommandType.UnfreezeAsset) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 4)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            if (!RpcApi.IsLogin)
                return true;

            try
            {
                byte[] owner_address = Wallet.Base58ToAddress(RpcApi.KeyStore.Address);

                RpcApiResult result = RpcApi.CreateUnfreezeAssetContract(owner_address,
                                                                         out UnfreezeAssetContract contract);

                TransactionExtention transaction_extention = null;
                if (result.Result)
                {
                    result = RpcApi.CreateTransaction(contract,
                                                      ContractType.UnfreezeAssetContract,
                                                      RpcCommandType.UnfreezeAsset,
                                                      out transaction_extention);
                }

                if (result.Result)
                {
                    result = RpcApi.ProcessTransactionExtention(transaction_extention);
                }

                OutputResultMessage(RpcCommandType.UnfreezeAsset, result.Result, result.Code, result.Message);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message + "\n\n" + e.StackTrace);
            }

            return true;
        }
    }
}
