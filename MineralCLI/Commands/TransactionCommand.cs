using Mineral.Core;
using Mineral.Core.Net.RpcHandler;
using MineralCLI.Network;
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
        /// Create account
        /// </summary>
        /// <param name="parameters"></param>
        /// /// Parameter Index
        /// [0] : Command
        /// [1] : Create account address
        /// <returns></returns>
        public static bool CreateAccount(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <address>\n", RpcCommandType.CreateAccount) };

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
                byte[] create_address = Wallet.Base58ToAddress(parameters[1]);

                RpcApiResult result = RpcApi.CreateAccountContract(owner_address,
                                                                   create_address,
                                                                   out AccountCreateContract contract);

                TransactionExtention transaction_extention = null;
                if (result.Result)
                {
                    result = RpcApi.CreateTransaction(contract,
                                                      ContractType.AccountCreateContract,
                                                      RpcCommandType.CreateAccount,
                                                      out transaction_extention);
                }

                if (result.Result)
                {
                    result = RpcApi.ProcessTransactionExtention(transaction_extention);
                }

                OutputResultMessage(RpcCommandType.CreateAccount, result.Result, result.Code, result.Message);
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
        /// [0] : Command
        /// [1~] : Proposal pair parameter
        /// <returns></returns>
        public static bool CreateProposal(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <id 1> <value 1> <id 2> <value 2> ...\n", RpcCommandType.CreateProposal) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length < 3 || (parameters.Length - 1) % 2 == 0)
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

                for (int i = 1; i < parameters.Length; i += 2)
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
                                                      RpcCommandType.CreateProposal,
                                                      out transaction_extention);
                }

                if (result.Result)
                {
                    result = RpcApi.ProcessTransactionExtention(transaction_extention);
                }

                OutputResultMessage(RpcCommandType.CreateProposal, result.Result, result.Code, result.Message);
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
        /// [0] : Command
        /// [1] : Witness url
        /// <returns></returns>
        public static bool CreateWitness(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <url>\n", RpcCommandType.CreateWitness) };

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
                byte[] url = Encoding.UTF8.GetBytes(parameters[1]);

                RpcApiResult result = RpcApi.CreateWitnessContract(owner_address,
                                                                   url,
                                                                   out WitnessCreateContract contract);

                TransactionExtention transaction_extention = null;
                if (result.Result)
                {
                    result = RpcApi.CreateTransaction(contract,
                                                      ContractType.WitnessCreateContract,
                                                      RpcCommandType.CreateWitness,
                                                      out transaction_extention);
                }

                if (result.Result)
                {
                    result = RpcApi.ProcessTransactionExtention(transaction_extention);
                }

                OutputResultMessage(RpcCommandType.CreateWitness, result.Result, result.Code, result.Message);
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
        /// [0] : Command
        /// [1] : Address name
        /// <returns></returns>
        public static bool UpdateAccount(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <name>\n", RpcCommandType.UpdateAccount) };

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
                byte[] name = Encoding.UTF8.GetBytes(parameters[1]);

                RpcApiResult result = RpcApi.CreateUpdateAcountContract(owner_address,
                                                                        name,
                                                                        out AccountUpdateContract contract);

                TransactionExtention transaction_extention = null;
                if (result.Result)
                {
                    result = RpcApi.CreateTransaction(contract,
                                                      ContractType.AccountUpdateContract,
                                                      RpcCommandType.UpdateAccount,
                                                      out transaction_extention);
                }

                if (result.Result)
                {
                    result = RpcApi.ProcessTransactionExtention(transaction_extention);
                }

                OutputResultMessage(RpcCommandType.UpdateAccount, result.Result, result.Code, result.Message);
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
        /// [0] : Command
        /// [1] : Witness url
        /// <returns></returns>
        public static bool UpdateWitness(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <url>\n", RpcCommandType.UpdateWitness) };

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
                byte[] url = Wallet.Base58ToAddress(parameters[1]);

                RpcApiResult result = RpcApi.CreateUpdateWitnessContract(owner_address,
                                                                         url,
                                                                         out WitnessUpdateContract contract);

                TransactionExtention transaction_extention = null;
                if (result.Result)
                {
                    result = RpcApi.CreateTransaction(contract,
                                                      ContractType.WitnessUpdateContract,
                                                      RpcCommandType.UpdateWitness,
                                                      out transaction_extention);
                }

                if (result.Result)
                {
                    result = RpcApi.ProcessTransactionExtention(transaction_extention);
                }

                OutputResultMessage(RpcCommandType.UpdateWitness, result.Result, result.Code, result.Message);
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
        /// [0] : Command
        /// [1] : Limit
        /// [2] : Public limit
        /// [3] : Description
        /// [4] : url
        /// <returns></returns>
        public static bool UpdateAsset(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <limit> <public limit> <description> <url>\n", RpcCommandType.UpdateAsset) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 5)
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
                long limit = long.Parse(parameters[1]);
                long public_limit = long.Parse(parameters[2]);
                byte[] owner_address = Wallet.Base58ToAddress(RpcApi.KeyStore.Address);
                byte[] description = Encoding.UTF8.GetBytes(parameters[3]);
                byte[] url = Encoding.UTF8.GetBytes(parameters[4]);

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
                                                      RpcCommandType.UpdateAsset,
                                                      out transaction_extention);
                }

                if (result.Result)
                {
                    result = RpcApi.ProcessTransactionExtention(transaction_extention);
                }

                OutputResultMessage(RpcCommandType.UpdateAsset, result.Result, result.Code, result.Message);
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
        /// [0] : Command
        /// [1] : Contract address
        /// [2] : Energy limit
        /// <returns></returns>
        public static bool UpdateEnergyLimit(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <contract address> <energy limit>\n", RpcCommandType.UpdateEnergyLimit) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 3)
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
                byte[] contract_address = Wallet.Base58ToAddress(parameters[1]);
                long energy_limit = long.Parse(parameters[2]);

                RpcApiResult result = RpcApi.CreateUpdateEnergyLimitContract(owner_address,
                                                                             contract_address,
                                                                             energy_limit,
                                                                             out UpdateEnergyLimitContract contract);

                TransactionExtention transaction_extention = null;
                if (result.Result)
                {
                    result = RpcApi.CreateTransaction(contract,
                                                      ContractType.UpdateEnergyLimitContract,
                                                      RpcCommandType.UpdateEnergyLimit,
                                                      out transaction_extention);
                }

                if (result.Result)
                {
                    result = RpcApi.ProcessTransactionExtention(transaction_extention);
                }

                OutputResultMessage(RpcCommandType.UpdateEnergyLimit, result.Result, result.Code, result.Message);
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
        /// [0] : Command
        /// [1] : Owner address
        /// [2] : Permission json
        /// <returns></returns>
        public static bool UpdateAccountPermission(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <owner address> <permission(json format)>\n", RpcCommandType.UpdateAccountPermission) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 3)
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
                byte[] owner_address = Wallet.Base58ToAddress(parameters[1]);
                string permission = parameters[2];

                RpcApiResult result = RpcApi.CreateAccountPermissionUpdateContract(owner_address,
                                                                                   permission,
                                                                                   out AccountPermissionUpdateContract contract);

                TransactionExtention transaction_extention = null;
                if (result.Result)
                {
                    result = RpcApi.CreateTransaction(contract,
                                                      ContractType.AccountPermissionUpdateContract,
                                                      RpcCommandType.UpdateAccountPermission,
                                                      out transaction_extention);
                }

                if (result.Result)
                {
                    result = RpcApi.ProcessTransactionExtention(transaction_extention);
                }

                OutputResultMessage(RpcCommandType.UpdateAccountPermission, result.Result, result.Code, result.Message);
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
        /// [0] : Command
        /// [1] : Contract address
        /// [2] : Consume user resource percent
        /// <returns></returns>
        public static bool UpdateSetting(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <address> <consume user resource percent>\n", RpcCommandType.UpdateSetting) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 3)
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
                byte[] contract_address = Wallet.Base58ToAddress(parameters[1]);
                long resource_percent = long.Parse(parameters[2]);
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
                                                      RpcCommandType.UpdateSetting,
                                                      out transaction_extention);
                }

                if (result.Result)
                {
                    result = RpcApi.ProcessTransactionExtention(transaction_extention);
                }

                OutputResultMessage(RpcCommandType.UpdateSetting, result.Result, result.Code, result.Message);
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
        /// [0] : Command
        /// [1] : Proposal id
        /// <returns></returns>
        public static bool DeleteProposal(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <id>\n", RpcCommandType.DeleteProposal) };

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
                long id = long.Parse(parameters[1]);

                RpcApiResult result = RpcApi.CreateProposalDeleteContract(owner_address,
                                                                          id,
                                                                          out ProposalDeleteContract contract);

                TransactionExtention transaction_extention = null;
                if (result.Result)
                {
                    result = RpcApi.CreateTransaction(contract,
                                                      ContractType.ProposalDeleteContract,
                                                      RpcCommandType.DeleteProposal,
                                                      out transaction_extention);
                }

                if (result.Result)
                {
                    result = RpcApi.ProcessTransactionExtention(transaction_extention);
                }

                OutputResultMessage(RpcCommandType.DeleteProposal, result.Result, result.Code, result.Message);
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
        /// [0] : Command
        /// [1] : To address
        /// [2] : Balance amount
        /// </param>
        /// <returns></returns>
        public static bool SendCoin(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <to address> <amount>\n", RpcCommandType.SendCoin) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 3)
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
                byte[] to_address = Wallet.Base58ToAddress(parameters[1]);
                long amount = long.Parse(parameters[2]);

                RpcApiResult result = RpcApi.CreateTransaferContract(owner_address,
                                                                     to_address,
                                                                     amount,
                                                                     out TransferContract contract);

                TransactionExtention transaction_extention = null;
                if (result.Result)
                {
                    result = RpcApi.CreateTransaction(contract,
                                                      ContractType.TransferContract,
                                                      RpcCommandType.CreateTransaction,
                                                      out transaction_extention);
                }

                if (result.Result)
                {
                    result = RpcApi.ProcessTransactionExtention(transaction_extention);
                }

                if (result.Result)
                {
                    Console.WriteLine(
                        string.Format("Send {0} drop to {1} + successful. ", long.Parse(parameters[2]), parameters[1]));
                }
                else
                {
                    Console.WriteLine(
                        string.Format("Send {0} drop to {1} + failed. ", long.Parse(parameters[2]), parameters[1]));
                }

                OutputResultMessage(RpcCommandType.SendCoin, result.Result, result.Code, result.Message);
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
        /// [0] : Command
        /// [1] : Amount
        /// [2] : Duration time (day)
        /// [3] : Energy / Bandwidth        (default 0 : enerygy)
        /// [4] : Address                   (optional)
        /// </param>
        /// <returns></returns>
        public static bool FreezeBalance(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <amount> <duration> || [<energy/bandwidth>}] || [<address>]\n", RpcCommandType.FreezeBalance) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length < 3 || parameters.Length > 5)
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
                long amount = long.Parse(parameters[1]);
                long duration = long.Parse(parameters[2]);
                int resource_code = 0;
                
                if (parameters.Length == 4)
                {
                    try
                    {
                        resource_code = int.Parse(parameters[3]);
                    }
                    catch (System.Exception e)
                    {
                        address = Wallet.Base58ToAddress(parameters[4]);
                    }
                }
                else if (parameters.Length == 5)
                {
                    resource_code = int.Parse(parameters[3]);
                    address = Wallet.Base58ToAddress(parameters[4]);
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
                                                      RpcCommandType.FreezeBalance,
                                                      out transaction_extention);
                }

                if (result.Result)
                {
                    result = RpcApi.ProcessTransactionExtention(transaction_extention);
                }

                OutputResultMessage(RpcCommandType.FreezeBalance, result.Result, result.Code, result.Message);
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
        /// [0] : Command
        /// [1] : Energy / Bandwidth        (default 0 : enerygy)
        /// [2] : Address                   (optional)
        /// </param>
        /// <returns></returns>
        public static bool UnFreezeBalance(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <address>\n", RpcCommandType.UnfreezeBalance) };

            string[] command_option = new string[] { HelpCommandOption.Help };
            
            if (parameters == null || parameters.Length < 2 || parameters.Length > 3)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                byte[] owner_address = Wallet.Base58ToAddress(RpcApi.KeyStore.Address);
                byte[] address = null;
                int resource_code = 0;

                if (parameters.Length == 2)
                {
                    try
                    {
                        resource_code = int.Parse(parameters[2]);
                    }
                    catch (System.Exception e)
                    {
                        address = Wallet.Base58ToAddress(parameters[2]);
                    }
                }
                else if (parameters.Length == 3)
                {
                    resource_code = int.Parse(parameters[2]);
                    address = Wallet.Base58ToAddress(parameters[2]);
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
                                                      RpcCommandType.UnfreezeBalance,
                                                      out transaction_extention);
                }

                if (result.Result)
                {
                    result = RpcApi.ProcessTransactionExtention(transaction_extention);
                }

                OutputResultMessage(RpcCommandType.UnfreezeBalance, result.Result, result.Code, result.Message);
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
        /// [0] : Command
        /// [1~] : vote pair parameter
        /// </param>
        /// <returns></returns>
        public static bool VoteWitness(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <address 1> <amount 1> <address 2> <amount 2> ...\n", RpcCommandType.VoteWitness) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length < 3 || (parameters.Length - 1) % 2 == 0)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                byte[] owner_address = Wallet.Base58ToAddress(RpcApi.KeyStore.Address);
                Dictionary<byte[], long> votes = new Dictionary<byte[], long>();

                for (int i = 1; i < parameters.Length; i += 2)
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
                                                      RpcCommandType.VoteWitness,
                                                      out transaction_extention);
                }

                if (result.Result)
                {
                    result = RpcApi.ProcessTransactionExtention(transaction_extention);
                }

                OutputResultMessage(RpcCommandType.VoteWitness, result.Result, result.Code, result.Message);
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
        /// [0] : Command
        /// </param>
        /// <returns></returns>
        public static bool WithdrawBalance(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] \n", RpcCommandType.WithdrawBalance) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length < 3 || (parameters.Length - 1) % 2 == 0)
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
                                                      RpcCommandType.WithdrawBalance,
                                                      out transaction_extention);
                }

                if (result.Result)
                {
                    result = RpcApi.ProcessTransactionExtention(transaction_extention);
                }

                OutputResultMessage(RpcCommandType.WithdrawBalance, result.Result, result.Code, result.Message);
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
