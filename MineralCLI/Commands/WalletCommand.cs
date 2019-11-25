using Mineral.CommandLine;
using Mineral.Core;
using Mineral.Core.Net.RpcHandler;
using Mineral.Wallets.KeyStore;
using MineralCLI.Network;
using MineralCLI.Util;
using Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using static Protocol.Transaction.Types.Contract.Types;

namespace MineralCLI.Commands
{
    public sealed class WalletCommand : BaseCommand
    {
        /// <summary>
        /// Generate keystore by privatekey
        /// </summary>
        /// <param name="parameters">
        /// Parameters Index
        /// </param>
        /// <returns></returns>
        public static bool ImportWallet(string command, string[] parameters)
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
                string password = CommandLineUtil.ReadPasswordString("Please input your password.");
                string privatekey = CommandLineUtil.ReadString("Please input your private key.");

                RpcApiResult result = RpcApi.Logout();
                if (result.Result)
                {
                    result = RpcApi.ImportWallet(password, privatekey);
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
        /// Extract private key by keystore
        /// </summary>
        /// <param name="parameters">
        /// Parameters Index
        /// </param>
        /// <returns></returns>
        public static bool BackupWallet(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <path>\n", command) };

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
                string password = CommandLineUtil.ReadPasswordString("Please input your password.");
                RpcApiResult result = RpcApi.BackupWallet(password);

                OutputResultMessage(command, result.Result, result.Code, result.Message);
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
        /// </param>
        /// <returns></returns>
        public static bool RegisterWallet(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <path>\n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters != null)
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
                Logout(null, null);

                OutputResultMessage(command, result.Result, result.Code, result.Message);
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
        /// </param>
        /// <returns></returns>
        public static bool Login(string command, string[] parameters)
        {
            RpcApiResult result = RpcApi.Logout();
            if (result.Result)
            {
                result = RpcApi.Login();
            }
            OutputResultMessage(command, result.Result, result.Code, result.Message);

            return true;
        }

        /// <summary>
        /// Logout keystore
        /// </summary>
        /// <param name="parameters">
        /// Parameters Index
        /// </param>
        /// <returns></returns>
        public static bool Logout(string command, string[] parameters)
        {
            RpcApiResult result = RpcApi.Logout();
            OutputResultMessage(command, result.Result, result.Code, result.Message);

            return true;
        }

        /// <summary>
        /// Get address in current keystore
        /// </summary>
        /// <param name="parameters">
        /// Parameters Index
        /// </param>
        /// <returns></returns>
        public static bool GetAddress(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option]\n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters != null)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            if (!RpcApi.IsLogin)
                return true;

            Console.WriteLine(RpcApi.KeyStore.Address);

            OutputResultMessage(command, true, 0, "");

            return true;
        }

        /// <summary>
        /// Get balance in current keystore
        /// </summary>
        /// <param name="parameters">
        /// Parameters Index
        /// </param>
        /// <returns></returns>
        public static bool GetBalance(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option]\n", command) };

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
                string address = RpcApi.KeyStore.Address;
                RpcApiResult result = RpcApi.GetBalance(out long balance);

                Console.WriteLine("Balance : " + balance);
                OutputResultMessage(command, result.Result, result.Code, result.Message);

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
        /// [1] : Wallet address
        /// </param>
        /// <returns></returns>
        public static bool GetAccount(string command, string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <address>\n", command) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 1)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            try
            {
                RpcApiResult result = RpcApi.GetAccount(parameters[0], out Account account);

                Console.WriteLine(PrintUtil.PrintAccount(account));
                OutputResultMessage(command, result.Result, result.Code, result.Message);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message + "\n\n" + e.StackTrace);
            }

            return true;
        }

        /// <summary>
        /// Get information witness list
        /// </summary>
        /// <param name="parameters">
        /// Parameter Index
        /// </param>
        /// <returns></returns>
        public static bool ListWitness(string command, string[] parameters)
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
                RpcApiResult result = RpcApi.ListWitness(out WitnessList witnesses);
                if (result.Result)
                {
                    Console.WriteLine(PrintUtil.PrintWitnessList(witnesses));
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
