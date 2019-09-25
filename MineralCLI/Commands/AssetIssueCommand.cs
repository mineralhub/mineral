using Mineral;
using Mineral.Core;
using Mineral.Core.Net.RpcHandler;
using MineralCLI.Network;
using MineralCLI.Util;
using Newtonsoft.Json.Linq;
using Protocol;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using static Protocol.Transaction.Types.Contract.Types;

namespace MineralCLI.Commands
{
    public class AssetIssueCommand : BaseCommand
    {

        /// <summary>
        /// Create asset issue
        /// </summary>
        /// <param name="parameters">
        /// Parameter Index
        /// [0] : Name
        /// [1] : Description
        /// [2] : Url
        /// [3] : transaction count
        /// [4] : count
        /// [5] : Precision
        /// [6] : Total supply
        /// [7] : Free net limit
        /// [8] : public free net limit
        /// [9] : Start time
        /// [10] : End time
        /// [11-] : Pair frozen supply
        /// </param>
        /// <returns></returns>
        public static bool CreateAssetIssue(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] " +
                              "<name> <description> <url>" + 
                              "<transaction count> <count>" +
                              "<precision>" +
                              "<total supply>" +
                              "<free net limit> <public free net limit>" +
                              "<start time> <end time>" +
                              "<amount 1> <days 1> <amount 2> <days 2> ...\n"
                              , command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length < 11 || parameters.Length % 2 == 0)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            if (!RpcApi.IsLogin)
            {
                return true;
            }

            try
            {
                int i = 0;
                byte[] owner_address = Wallet.Base58ToAddress(RpcApi.KeyStore.Address);
                string name = parameters[i++];
                string description = parameters[i++];
                string url = parameters[i++];
                long total_supply = long.Parse(parameters[i++]);
                int tx_num = int.Parse(parameters[i++]);
                int num = int.Parse(parameters[i++]);
                int precision = int.Parse(parameters[i++]);
                long free_limit = long.Parse(parameters[i++]);
                long public_free_limit = long.Parse(parameters[i++]);
                DateTime start_time = DateTime.ParseExact(parameters[i++], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None);
                DateTime end_time = DateTime.ParseExact(parameters[i++], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None);

                Dictionary<long, long> frozen_supply = new Dictionary<long, long>();
                for (int j = i; j < parameters.Length; j += 2)
                {
                    frozen_supply.Add(
                        long.Parse(parameters[j + 0]),
                        long.Parse(parameters[j + 1])
                        );
                }

                RpcApiResult result = RpcApi.CreateAssetIssueContract(owner_address,
                                                                      name,
                                                                      description,
                                                                      url,
                                                                      tx_num,
                                                                      num,
                                                                      precision,
                                                                      0,
                                                                      total_supply,
                                                                      free_limit,
                                                                      public_free_limit,
                                                                      start_time.ToTimestamp(),
                                                                      end_time.ToTimestamp(),
                                                                      frozen_supply,
                                                                      out AssetIssueContract contract);

                TransactionExtention transaction_extention = null;
                if (result.Result)
                {
                    result = RpcApi.CreateTransaction(contract,
                                                      ContractType.AssetIssueContract,
                                                      command,
                                                      out transaction_extention);
                }

                if (result.Result)
                {
                    result = RpcApi.ProcessTransactionExtention(transaction_extention);
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
        /// Update asset
        /// </summary>
        /// <param name="parameters"></param>
        /// /// Parameter Index
        /// [0] : Limit
        /// [1] : Public limit
        /// [2] : Description
        /// [3] : url
        /// <returns></returns>
        public static bool UpdateAsset(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <limit> <public limit> <description> <url>\n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 4)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            if (!RpcApi.IsLogin)
            {
                return true;
            }

            try
            {
                long limit = long.Parse(parameters[0]);
                long public_limit = long.Parse(parameters[1]);
                byte[] owner_address = Wallet.Base58ToAddress(RpcApi.KeyStore.Address);
                byte[] description = Encoding.UTF8.GetBytes(parameters[2]);
                byte[] url = Encoding.UTF8.GetBytes(parameters[3]);

                RpcApiResult result = RpcApi.CreateUpdateAssetContract(owner_address,
                                                                       description,
                                                                       url,
                                                                       limit,
                                                                       public_limit,
                                                                       out UpdateAssetContract contract);

                TransactionExtention transaction_extention = null;
                if (result.Result)
                {
                    result = RpcApi.CreateTransaction(contract,
                                                      ContractType.UpdateAssetContract,
                                                      command,
                                                      out transaction_extention);
                }

                if (result.Result)
                {
                    result = RpcApi.ProcessTransactionExtention(transaction_extention);
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
        /// Get infomation asset issue by account 
        /// </summary>
        /// <param name="parameters">
        /// Parameter Index
        /// [0] : asset issue address
        /// </param>
        /// <returns></returns>
        public static bool AssetIssueByAccount(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <asset issue address>\n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 1)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                string address = parameters[0];
                RpcApiResult result = RpcApi.AssetIssueByAccount(address, out AssetIssueList asset_issues);

                if (result.Result)
                {
                    Console.WriteLine(PrintUtil.PrintAssetIssueList(asset_issues));
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
        /// Get infomation asset issue by id
        /// </summary>
        /// <param name="parameters">
        /// Parameter Index
        /// [0] : Asset issue id
        /// </param>
        /// <returns></returns>
        public static bool AssetIssueById(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <asset issue id>\n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 1)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                string id = parameters[0];
                RpcApiResult result = RpcApi.AssetIssueById(id, out AssetIssueContract contract);
                if (result.Result)
                {
                    Console.WriteLine(PrintUtil.PrintAssetIssue(contract));
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
        /// Get infomation asset issue list by name
        /// </summary>
        /// <param name="parameters">
        /// Parameter Index
        /// [0] : Asset issue name
        /// </param>
        /// <returns></returns>
        public static bool AssetIssueByName(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <asset issue name>\n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 1)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                string name = parameters[0];
                RpcApiResult result = RpcApi.AssetIssueByName(name, out AssetIssueContract contract);
                if (result.Result)
                {
                    Console.WriteLine(PrintUtil.PrintAssetIssue(contract));
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
        /// Get infomation asset issue by name
        /// </summary>
        /// <param name="parameters">
        /// Parameter Index
        /// [0] : Asset issue name
        /// </param>
        /// <returns></returns>
        public static bool AssetIssueListByName(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <asset issue name>\n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 1)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                string name = parameters[0];
                RpcApiResult result = RpcApi.AssetIssueListByName(name, out AssetIssueList contracts);
                if (result.Result)
                {
                    Console.WriteLine(PrintUtil.PrintAssetIssueList(contracts));
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
        /// Get information asset issue list
        /// </summary>
        /// <param name="parameters">
        /// Parameter Index
        /// </param>
        /// <returns></returns>
        public static bool ListAssetIssue(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] \n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters != null)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                RpcApiResult result = RpcApi.ListAssetIssue(out AssetIssueList contracts);
                if (result.Result)
                {
                    Console.WriteLine(PrintUtil.PrintAssetIssueList(contracts));
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
        /// Get information exchange list
        /// </summary>
        /// <param name="parameters">
        /// Parameter Index
        /// </param>
        /// <returns></returns>
        public static bool ListExchange(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] \n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters != null)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                RpcApiResult result = RpcApi.ListExchange(out ExchangeList exchanges);
                if (result.Result)
                {
                    Console.WriteLine(PrintUtil.PrintExchangeList(exchanges));
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
        /// Get information exchange list
        /// </summary>
        /// <param name="parameters">
        /// Parameter Index
        /// [0] : Offset
        /// [1] : Limit
        /// </param>
        /// <returns></returns>
        public static bool ListExchangePaginated(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <offset> <limit> \n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 2)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                int offset = int.Parse(parameters[0]);
                int limit = int.Parse(parameters[1]);

                RpcApiResult result = RpcApi.ListExchangePaginated(offset,
                                                                   limit,
                                                                   out ExchangeList exchanges);
                if (result.Result)
                {
                    Console.WriteLine(PrintUtil.PrintExchangeList(exchanges));
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
        /// Transfer asset
        /// </summary>
        /// <param name="parameters">
        /// Parameter Index
        /// [0] : To address
        /// [1] : Asset name
        /// [2] : Amount
        /// </param>
        /// <returns></returns>
        public static bool TransferAsset(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <to address> <asset name> <amount>\n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 3)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            if (!RpcApi.IsLogin)
                return true;

            try
            {
                byte[] to_address = Encoding.UTF8.GetBytes(parameters[0]);
                byte[] asset_name = Encoding.UTF8.GetBytes(parameters[1]);
                byte[] owner_address = Wallet.Base58ToAddress(RpcApi.KeyStore.Address);
                long amount = long.Parse(parameters[2]);


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
                                                      command,
                                                      out transaction_extention);
                }

                if (result.Result)
                {
                    result = RpcApi.ProcessTransactionExtention(transaction_extention);
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
        /// Transfer asset
        /// </summary>
        /// <param name="parameters">
        /// Parameter Index
        /// </param>
        /// <returns></returns>
        public static bool UnfreezeAsset(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <to address> <asset name> <amount>\n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters != null)
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
                                                      command,
                                                      out transaction_extention);
                }

                if (result.Result)
                {
                    result = RpcApi.ProcessTransactionExtention(transaction_extention);
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
