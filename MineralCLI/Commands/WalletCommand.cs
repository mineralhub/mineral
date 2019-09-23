using Mineral.CommandLine;
using Mineral.Core;
using Mineral.Core.Net.RpcHandler;
using Mineral.Wallets.KeyStore;
using MineralCLI.Network;
using MineralCLI.Util;
using Protocol;
using System;
using System.Collections.Generic;

namespace MineralCLI.Commands
{
    public sealed class WalletCommand : BaseCommand
    {
        /// <summary>
        /// Generate keystore by privatekey
        /// </summary>
        /// <param name="parameters">
        /// Parameters Index
        /// [0] : Command
        /// </param>
        /// <returns></returns>
        public static bool ImportWallet(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <path>\n", RpcCommandType.BackupWallet) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 1)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                string password = CommandLineUtil.ReadPasswordString("Please input your password.");
                string privatekey = CommandLineUtil.ReadString("Please input your private key.");

                RpcApiResult result = RpcApi.ImportWallet(password, privatekey);
                Logout(null);

                OutputResultMessage(RpcCommandType.BackupWallet, result.Result, result.Code, result.Message);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message + "\n\n" + e.StackTrace);
            }

            return true;
        }

        /// <summary>
        /// Extract private key by keystore
        /// </summary>
        /// <param name="parameters">
        /// Parameters Index
        /// [0] : Command
        /// </param>
        /// <returns></returns>
        public static bool BackupWallet(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <path>\n", RpcCommandType.BackupWallet) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 1)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            if (!RpcApi.IsLogin)
                return true;

            try
            {
                string password = CommandLineUtil.ReadPasswordString("Please input your password.");
                RpcApiResult result = RpcApi.BackupWallet(password);

                OutputResultMessage(RpcCommandType.BackupWallet, result.Result, result.Code, result.Message);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message + "\n\n" + e.StackTrace);
            }
            return true;
        }

        /// <summary>
        /// Create keystore file
        /// </summary>
        /// <param name="parameters">
        /// Parameters Index
        /// [0] : Command
        /// </param>
        /// <returns></returns>
        public static bool RegisterWallet(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <path>\n", RpcCommandType.RegisterWallet) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 1)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                string password = CommandLineUtil.ReadPasswordString("Please input wallet password");
                string confirm = CommandLineUtil.ReadPasswordString("Please input confirm wallet password");

                if (!password.Equals(confirm))
                {
                    Console.WriteLine("Confirm password does not match");
                    return true;
                }

                RpcApiResult result = RpcApi.RegisterWallet(password);
                Logout(null);

                OutputResultMessage(RpcCommandType.RegisterWallet, result.Result, result.Code, result.Message);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message + "\n\n" + e.StackTrace);
            }


            return true;
        }

        /// <summary>
        /// Login keystore
        /// </summary>
        /// <param name="parameters">
        /// Parameters Index
        /// [0] : Command
        /// </param>
        /// <returns></returns>
        public static bool Login(string[] parameters)
        {
            Logout(null);

            KeyStore keystore = RpcApi.SelectKeyStore();

            string password = CommandLineUtil.ReadPasswordString("Please input your password.");
            if (!KeyStoreService.CheckPassword(password, keystore))
            {
                Console.WriteLine("Login Fail.");
                return true;
            }

            RpcApi.KeyStore = keystore;

            OutputResultMessage(RpcCommandType.Login, true, 0, "");

            return true;
        }

        /// <summary>
        /// Logout keystore
        /// </summary>
        /// <param name="parameters">
        /// Parameters Index
        /// [0] : Command
        /// </param>
        /// <returns></returns>
        public static bool Logout(string[] parameters)
        {
            RpcApi.KeyStore = null;
            OutputResultMessage(RpcCommandType.Logout, true, 0, "");

            return true;
        }

        /// <summary>
        /// Get address in current keystore
        /// </summary>
        /// <param name="parameters">
        /// Parameters Index
        /// [0] : Command
        /// </param>
        /// <returns></returns>
        public static bool GetAddress(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option]\n", RpcCommandType.GetAddress) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 1)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            if (!RpcApi.IsLogin)
                return true;

            OutputResultMessage(RpcCommandType.GetAddress, true, 0, "");

            return true;
        }

        /// <summary>
        /// Get balance in current keystore
        /// </summary>
        /// <param name="parameters">
        /// Parameters Index
        /// [0] : Command
        /// </param>
        /// <returns></returns>
        public static bool GetBalance(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option]\n", RpcCommandType.GetAccount) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 1)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            if (!RpcApi.IsLogin)
                return true;

            try
            {
                string address = RpcApi.KeyStore.Address;
                RpcApiResult result = RpcApi.GetBalance(out long balance);

                Console.WriteLine("Balance : " + balance);
                OutputResultMessage(RpcCommandType.GetBalance, result.Result, result.Code, result.Message);

            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message + "\n\n" + e.StackTrace);
            }

            return true;
        }

        /// <summary>
        /// Get account infomation
        /// </summary>
        /// <param name="parameters">
        /// Parameter Index
        /// [0] : Command
        /// [1] : Wallet address
        /// </param>
        /// <returns></returns>
        public static bool GetAccount(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <address>\n", RpcCommandType.GetAccount) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 2)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                RpcApiResult result = RpcApi.GetAccount(parameters[1], out Account account);

                Console.WriteLine(PrintUtil.PrintAccount(account));
                OutputResultMessage(RpcCommandType.GetAccount, result.Result, result.Code, result.Message);
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
                byte[] to_address = Wallet.Base58ToAddress(parameters[1]);

                RpcApiResult result = RpcApi.CreateAccountContract(owner_address,
                                                                   to_address,
                                                                   out AccountCreateContract contract);

                TransactionExtention transaction_extention = null;
                if (result.Result)
                {
                    result = RpcApi.CreateTransaction(contract, RpcCommandType.CreateAccount, out transaction_extention);
                }

                if (result.Result)
                {
                    result = RpcApi.ProcessTransactionExtention(transaction_extention);
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
        /// Create account
        /// </summary>
        /// <param name="parameters"></param>
        /// /// Parameter Index
        /// [0] : Command
        /// [1] : Proposal pair parameter
        /// <returns></returns>
        public static bool CreateProposal(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <address>\n", RpcCommandType.CreateAccount) };

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
                    result = RpcApi.CreateTransaction(contract, RpcCommandType.CreateProposal, out transaction_extention);
                }

                if (result.Result)
                {
                    result = RpcApi.ProcessTransactionExtention(transaction_extention);
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
                RpcApiResult result = RpcApi.CreateTransaferContract(Wallet.Base58ToAddress(RpcApi.KeyStore.Address),
                                                                     Wallet.Base58ToAddress(parameters[1]),
                                                                     long.Parse(parameters[2]),
                                                                     out TransferContract contract);

                TransactionExtention transaction_extention = null;
                if (result.Result)
                {
                    result = RpcApi.CreateTransaction(contract, RpcCommandType.CreateTransaction, out transaction_extention);
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
    }
}
