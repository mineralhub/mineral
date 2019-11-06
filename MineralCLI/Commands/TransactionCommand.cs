using Mineral;
using Mineral.Core;
using Mineral.Core.Net.RpcHandler;
using Mineral.Utils;
using MineralCLI.Network;
using MineralCLI.Util;
using Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using static Protocol.Transaction.Types.Contract.Types;

namespace MineralCLI.Commands
{
    public class TransactionCommand : BaseCommand
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
        /// <summary>
        /// Get total transaction
        /// </summary>
        /// <param name="parameters"></param>
        /// /// Parameter Index
        /// <returns></returns>
        public static bool GetTotalTransaction(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] \n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 0)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                RpcApiResult result = RpcApi.GetTotalTransaction(out NumberMessage message);
                if (result.Result)
                {
                    Console.WriteLine("The num of total transaction is " + message.Num);
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
        /// Get approved transaction list
        /// </summary>
        /// <param name="parameters"></param>
        /// /// Parameter Index
        /// [0] : Transaction binary data
        /// <returns></returns>
        public static bool GetTransactionApprovedList(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <transaction>\n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 1)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                Transaction transaction = Transaction.Parser.ParseFrom(parameters[0].HexToBytes());
                RpcApiResult result = RpcApi.GetTransactionApprovedList(transaction, out TransactionApprovedList transaction_list);
                if (result.Result)
                {
                    Console.WriteLine(PrintUtil.PrintTransactionApprovedList(transaction_list));
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
        /// Get transaction information by id
        /// </summary>
        /// <param name="parameters"></param>
        /// /// Parameter Index
        /// [0] : Transaction id
        /// <returns></returns>
        public static bool GetTransactionById(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <transaction id>\n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 1)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                RpcApiResult result = RpcApi.GetTransactionById(parameters[0], out TransactionExtention transaction);
                if (result.Result)
                {
                    Console.WriteLine(PrintUtil.PrintTransaction(transaction));
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
        /// Get transaction information by id
        /// </summary>
        /// <param name="parameters"></param>
        /// /// Parameter Index
        /// [0] : Transaction id
        /// <returns></returns>
        public static bool GetTransactionInfoById(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <transaction id>\n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 1)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                RpcApiResult result = RpcApi.GetTransactionInfoById(parameters[0], out TransactionInfo transaction);
                if (result.Result)
                {
                    Console.WriteLine(PrintUtil.PrintTransactionInfo(transaction));
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
        /// Get transaction count in block
        /// </summary>
        /// <param name="parameters"></param>
        /// /// Parameter Index
        /// [0] : Block number
        /// <returns></returns>
        public static bool GetTransactionCountByBlockNum(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <block number>\n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 1)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                long block_num = long.Parse(parameters[0]);
                RpcApiResult result = RpcApi.GetTransactionCountByBlockNum(block_num, out int count);
                if (result.Result)
                {
                    Console.WriteLine("The block contain " + count + "transactions");
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
        /// Get transaction information by id
        /// </summary>
        /// <param name="parameters"></param>
        /// /// Parameter Index
        /// [0] : Transaction id
        /// <returns></returns>
        public static bool GetTransactionsFromThis(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <transaction id>\n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 3)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                byte[] address = Wallet.Base58ToAddress(parameters[0]);
                int offset = int.Parse(parameters[1]);
                int limit = int.Parse(parameters[2]);

                RpcApiResult result = RpcApi.GetTransactionsFromThis(address, offset, limit, out TransactionListExtention transactions);
                if (result.Result)
                {
                    if (transactions != null)
                    {
                        Console.WriteLine(PrintUtil.PrintTransactionExtentionList(new List<TransactionExtention>(transactions.Transaction)));
                    }
                    else
                    {
                        Console.WriteLine("No transaction from " + Wallet.AddressToBase58(address));
                    }
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
        /// Get transaction information by id
        /// </summary>
        /// <param name="parameters"></param>
        /// /// Parameter Index
        /// [0] : Transaction id
        /// <returns></returns>
        public static bool GetTransactionsToThis(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <transaction id>\n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 3)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                byte[] address = Wallet.Base58ToAddress(parameters[0]);
                int offset = int.Parse(parameters[1]);
                int limit = int.Parse(parameters[2]);

                RpcApiResult result = RpcApi.GetTransactionsToThis(address, offset, limit, out TransactionListExtention transactions);
                if (result.Result)
                {
                    if (transactions != null)
                    {
                        Console.WriteLine(PrintUtil.PrintTransactionExtentionList(new List<TransactionExtention>(transactions.Transaction)));
                    }
                    else
                    {
                        Console.WriteLine("No transaction from " + Wallet.AddressToBase58(address));
                    }
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
        /// Create account
        /// </summary>
        /// <param name="parameters"></param>
        /// /// Parameter Index
        /// [0] : Create account address
        /// <returns></returns>
        public static bool CreateAccount(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <address>\n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 1)
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
                byte[] owner_address = Wallet.Base58ToAddress(RpcApi.KeyStore.Address);
                byte[] create_address = Wallet.Base58ToAddress(parameters[0]);

                RpcApiResult result = RpcApi.CreateAccountContract(owner_address,
                                                                   create_address,
                                                                   out AccountCreateContract contract);

                TransactionExtention transaction_extention = null;
                if (result.Result)
                {
                    result = RpcApi.CreateTransaction(contract,
                                                      ContractType.AccountCreateContract,
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
        /// Create proposal
        /// </summary>
        /// <param name="parameters"></param>
        /// /// Parameter Index
        /// [0~] : Proposal pair parameter
        /// <returns></returns>
        public static bool CreateProposal(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <id 1> <value 1> <id 2> <value 2> ...\n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length < 2 || parameters.Length % 2 != 0)
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
                byte[] owner_address = Wallet.Base58ToAddress(RpcApi.KeyStore.Address);
                Dictionary<long, long> proposal = new Dictionary<long, long>();

                for (int i = 0; i < parameters.Length; i += 2)
                {
                    long id = long.Parse(parameters[i]);
                    long value = long.Parse(parameters[i + 1]);
                    proposal.Add(id, value);
                }

                RpcApiResult result = RpcApi.CreateProposalContract(owner_address,
                                                                    proposal,
                                                                    out ProposalCreateContract contract);

                TransactionExtention transaction_extention = null;
                if (result.Result)
                {
                    result = RpcApi.CreateTransaction(contract,
                                                      ContractType.ProposalCreateContract,
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
        /// Create witenss
        /// </summary>
        /// <param name="parameters"></param>
        /// /// Parameter Index
        /// [0] : Witness url
        /// <returns></returns>
        public static bool CreateWitness(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <url>\n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 1)
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
                byte[] owner_address = Wallet.Base58ToAddress(RpcApi.KeyStore.Address);
                byte[] url = Encoding.UTF8.GetBytes(parameters[0]);

                RpcApiResult result = RpcApi.CreateWitnessContract(owner_address,
                                                                   url,
                                                                   out WitnessCreateContract contract);

                TransactionExtention transaction_extention = null;
                if (result.Result)
                {
                    result = RpcApi.CreateTransaction(contract,
                                                      ContractType.WitnessCreateContract,
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
        /// Update account
        /// </summary>
        /// <param name="parameters"></param>
        /// /// Parameter Index
        /// [0] : Address name
        /// <returns></returns>
        public static bool UpdateAccount(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <name>\n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 1)
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
                byte[] owner_address = Wallet.Base58ToAddress(RpcApi.KeyStore.Address);
                byte[] name = Encoding.UTF8.GetBytes(parameters[0]);

                RpcApiResult result = RpcApi.CreateUpdateAcountContract(owner_address,
                                                                        name,
                                                                        out AccountUpdateContract contract);

                TransactionExtention transaction_extention = null;
                if (result.Result)
                {
                    result = RpcApi.CreateTransaction(contract,
                                                      ContractType.AccountUpdateContract,
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
        /// Update witness
        /// </summary>
        /// <param name="parameters"></param>
        /// /// Parameter Index
        /// [0] : Witness url
        /// <returns></returns>
        public static bool UpdateWitness(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <url>\n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 1)
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
                byte[] owner_address = Wallet.Base58ToAddress(RpcApi.KeyStore.Address);
                byte[] url = Wallet.Base58ToAddress(parameters[0]);

                RpcApiResult result = RpcApi.CreateUpdateWitnessContract(owner_address,
                                                                         url,
                                                                         out WitnessUpdateContract contract);

                TransactionExtention transaction_extention = null;
                if (result.Result)
                {
                    result = RpcApi.CreateTransaction(contract,
                                                      ContractType.WitnessUpdateContract,
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
        /// Update energy limit
        /// </summary>
        /// <param name="parameters"></param>
        /// /// Parameter Index
        /// [0] : Contract address
        /// [1] : Energy limit
        /// <returns></returns>
        public static bool UpdateEnergyLimit(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <contract address> <energy limit>\n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 2)
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
                byte[] owner_address = Wallet.Base58ToAddress(RpcApi.KeyStore.Address);
                byte[] contract_address = Wallet.Base58ToAddress(parameters[0]);
                long energy_limit = long.Parse(parameters[1]);

                RpcApiResult result = RpcApi.CreateUpdateEnergyLimitContract(owner_address,
                                                                             contract_address,
                                                                             energy_limit,
                                                                             out UpdateEnergyLimitContract contract);

                TransactionExtention transaction_extention = null;
                if (result.Result)
                {
                    result = RpcApi.CreateTransaction(contract,
                                                      ContractType.UpdateEnergyLimitContract,
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
        /// Update account permission
        /// </summary>
        /// <param name="parameters"></param>
        /// /// Parameter Index
        /// [0] : Owner address
        /// [1] : Permission json
        /// <returns></returns>
        public static bool UpdateAccountPermission(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <owner address> <permission(json format)>\n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 2)
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
                byte[] owner_address = Wallet.Base58ToAddress(parameters[0]);
                string permission = parameters[2];

                RpcApiResult result = RpcApi.CreateAccountPermissionUpdateContract(owner_address,
                                                                                   permission,
                                                                                   out AccountPermissionUpdateContract contract);

                TransactionExtention transaction_extention = null;
                if (result.Result)
                {
                    result = RpcApi.CreateTransaction(contract,
                                                      ContractType.AccountPermissionUpdateContract,
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
        /// Update setting
        /// </summary>
        /// <param name="parameters"></param>
        /// /// Parameter Index
        /// [0] : Contract address
        /// [1] : Consume user resource percent
        /// <returns></returns>
        public static bool UpdateSetting(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <address> <consume user resource percent>\n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 2)
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
                byte[] owner_address = Wallet.Base58ToAddress(RpcApi.KeyStore.Address);
                byte[] contract_address = Wallet.Base58ToAddress(parameters[0]);
                long resource_percent = long.Parse(parameters[1]);
                if (resource_percent > 100 || resource_percent < 0)
                {
                    Console.WriteLine("Consume user resource percent must be 0 to 100.");
                    return true;
                }

                RpcApiResult result = RpcApi.CreateUpdateSettingContract(owner_address,
                                                                         contract_address,
                                                                         resource_percent,
                                                                         out UpdateSettingContract contract);

                TransactionExtention transaction_extention = null;
                if (result.Result)
                {
                    result = RpcApi.CreateTransaction(contract,
                                                      ContractType.UpdateSettingContract,
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
        /// Update setting
        /// </summary>
        /// <param name="parameters"></param>
        /// /// Parameter Index
        /// [0] : Proposal id
        /// <returns></returns>
        public static bool DeleteProposal(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <id>\n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 1)
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
                byte[] owner_address = Wallet.Base58ToAddress(RpcApi.KeyStore.Address);
                long id = long.Parse(parameters[0]);

                RpcApiResult result = RpcApi.CreateProposalDeleteContract(owner_address,
                                                                          id,
                                                                          out ProposalDeleteContract contract);

                TransactionExtention transaction_extention = null;
                if (result.Result)
                {
                    result = RpcApi.CreateTransaction(contract,
                                                      ContractType.ProposalDeleteContract,
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
        /// Send balance
        /// </summary>
        /// <param name="parameters">
        /// Parameter Index
        /// [0] : To address
        /// [1] : Balance amount
        /// </param>
        /// <returns></returns>
        public static bool SendCoin(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <to address> <amount>\n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 2)
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
                byte[] owner_address = Wallet.Base58ToAddress(RpcApi.KeyStore.Address);
                byte[] to_address = Wallet.Base58ToAddress(parameters[0]);
                long amount = long.Parse(parameters[1]);

                RpcApiResult result = RpcApi.CreateTransaferContract(owner_address,
                                                                     to_address,
                                                                     amount,
                                                                     out TransferContract contract);

                TransactionExtention transaction_extention = null;
                if (result.Result)
                {
                    result = RpcApi.CreateTransaction(contract,
                                                      ContractType.TransferContract,
                                                      out transaction_extention);
                }

                if (result.Result)
                {
                    result = RpcApi.ProcessTransactionExtention(transaction_extention);
                }

                if (result.Result)
                {
                    Console.WriteLine(
                        string.Format("Send {0} drop to {1} + successful. ", long.Parse(parameters[1]), parameters[0]));
                }
                else
                {
                    Console.WriteLine(
                        string.Format("Send {0} drop to {1} + failed. ", long.Parse(parameters[1]), parameters[0]));
                }

                OutputResultMessage(RpcCommand.Transaction.SendCoin, result.Result, result.Code, result.Message);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message + "\n\n" + e.StackTrace);
            }

            return true;
        }

        /// <summary>
        /// Freeze balance
        /// </summary>
        /// <param name="parameters">
        /// Parameter Index
        /// [0] : Amount
        /// [1] : Duration time (day)
        /// [2] : Energy / Bandwidth        (default 0 : enerygy)
        /// [3] : Address                   (optional)
        /// </param>
        /// <returns></returns>
        public static bool FreezeBalance(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <amount> <duration> || [<energy/bandwidth>}] || [<address>]\n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length < 2 || parameters.Length > 4)
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
                byte[] owner_address = Wallet.Base58ToAddress(RpcApi.KeyStore.Address);
                byte[] address = null;
                long amount = long.Parse(parameters[0]);
                long duration = long.Parse(parameters[1]);
                int resource_code = 0;
                
                if (parameters.Length == 3)
                {
                    try
                    {
                        resource_code = int.Parse(parameters[2]);
                    }
                    catch
                    {
                        address = Wallet.Base58ToAddress(parameters[3]);
                    }
                }
                else if (parameters.Length == 4)
                {
                    resource_code = int.Parse(parameters[2]);
                    address = Wallet.Base58ToAddress(parameters[3]);
                }

                RpcApiResult result = RpcApi.CreateFreezeBalanceContract(owner_address,
                                                                         address,
                                                                         amount,
                                                                         duration,
                                                                         resource_code,
                                                                         out FreezeBalanceContract contract);


                TransactionExtention transaction_extention = null;
                if (result.Result)
                {
                    result = RpcApi.CreateTransaction(contract,
                                                      ContractType.FreezeBalanceContract,
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
        /// UnFreeze balance
        /// </summary>
        /// <param name="parameters">
        /// Parameter Index
        /// [0] : Energy / Bandwidth        (default 0 : enerygy)
        /// [1] : Address                   (optional)
        /// </param>
        /// <returns></returns>
        public static bool UnFreezeBalance(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <address>\n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };
            
            if (parameters == null || parameters.Length < 1 || parameters.Length > 2)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                byte[] owner_address = Wallet.Base58ToAddress(RpcApi.KeyStore.Address);
                byte[] address = null;
                int resource_code = 0;

                if (parameters.Length == 1)
                {
                    try
                    {
                        resource_code = int.Parse(parameters[0]);
                    }
                    catch (System.Exception)
                    {
                        address = Wallet.Base58ToAddress(parameters[0]);
                    }
                }
                else if (parameters.Length == 2)
                {
                    resource_code = int.Parse(parameters[0]);
                    address = Wallet.Base58ToAddress(parameters[1]);
                }


                RpcApiResult result = RpcApi.CreateUnfreezeBalanceContract(owner_address,
                                                                           address,
                                                                           resource_code,
                                                                           out UnfreezeBalanceContract contract);


                TransactionExtention transaction_extention = null;
                if (result.Result)
                {
                    result = RpcApi.CreateTransaction(contract,
                                                      ContractType.UnfreezeBalanceContract,
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
        /// Vote
        /// </summary>
        /// <param name="parameters">
        /// Parameter Index
        /// [0~] : vote pair parameter
        /// </param>
        /// <returns></returns>
        public static bool VoteWitness(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <address 1> <amount 1> <address 2> <amount 2> ...\n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length < 2 || parameters.Length % 2 == 0)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                byte[] owner_address = Wallet.Base58ToAddress(RpcApi.KeyStore.Address);
                Dictionary<byte[], long> votes = new Dictionary<byte[], long>(new ByteArrayEqualComparer());

                for (int i = 0; i < parameters.Length; i += 2)
                {
                    byte[] address = Wallet.Base58ToAddress(parameters[i]);
                    long amount = long.Parse(parameters[i + 1]);
                    votes.Add(address, amount);
                }

                RpcApiResult result = RpcApi.CreateVoteWitnessContract(owner_address,
                                                                       votes,
                                                                       out VoteWitnessContract contract);


                TransactionExtention transaction_extention = null;
                if (result.Result)
                {
                    result = RpcApi.CreateTransaction(contract,
                                                      ContractType.VoteWitnessContract,
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
        /// Withdraw balance
        /// </summary>
        /// <param name="parameters">
        /// Parameter Index
        /// </param>
        /// <returns></returns>
        public static bool WithdrawBalance(string command, string[] parameters)
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
                byte[] owner_address = Wallet.Base58ToAddress(RpcApi.KeyStore.Address);

                RpcApiResult result = RpcApi.CreateWithdrawBalanceContract(owner_address,
                                                                           out WithdrawBalanceContract contract);

                TransactionExtention transaction_extention = null;
                if (result.Result)
                {
                    result = RpcApi.CreateTransaction(contract,
                                                      ContractType.WithdrawBalanceContract,
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
        /// Get information proposal list
        /// </summary>
        /// <param name="parameters">
        /// Parameter Index
        /// </param>
        /// <returns></returns>
        public static bool ListProposal(string command, string[] parameters)
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
                RpcApiResult result = RpcApi.ListProposal(out ProposalList proposals);
                if (result.Result)
                {
                    Console.WriteLine(PrintUtil.PrintProposalsList(proposals));
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
        /// Get information proposal list
        /// </summary>
        /// <param name="parameters">
        /// Parameter Index
        /// [0] : Offset
        /// [1] : Limit
        /// </param>
        /// <returns></returns>
        public static bool ListProposalPaginated(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] \n", command) };

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

                RpcApiResult result = RpcApi.ListProposalPaginated(offset,
                                                                   limit,
                                                                   out ProposalList proposals);
                if (result.Result)
                {
                    Console.WriteLine(PrintUtil.PrintProposalsList(proposals));
                }

                OutputResultMessage(command, result.Result, result.Code, result.Message);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message + "\n\n" + e.StackTrace);
            }

            return true;
        }
        #endregion
    }
}
