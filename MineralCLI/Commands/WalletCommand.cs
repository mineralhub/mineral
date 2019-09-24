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
        /// [0] : Command
        /// </param>
        /// <returns></returns>
        public static bool ImportWallet(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <path>\n", RpcCommand.Wallet.BackupWallet) };

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

                OutputResultMessage(RpcCommand.Wallet.BackupWallet, result.Result, result.Code, result.Message);
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
                string.Format("{0} [command option] <path>\n", RpcCommand.Wallet.BackupWallet) };

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

                OutputResultMessage(RpcCommand.Wallet.BackupWallet, result.Result, result.Code, result.Message);
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
                string.Format("{0} [command option] <path>\n", RpcCommand.Wallet.RegisterWallet) };

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

                OutputResultMessage(RpcCommand.Wallet.RegisterWallet, result.Result, result.Code, result.Message);
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

            OutputResultMessage(RpcCommand.Wallet.Login, true, 0, "");

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
            OutputResultMessage(RpcCommand.Wallet.Logout, true, 0, "");

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
                string.Format("{0} [command option]\n", RpcCommand.Wallet.GetAddress) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 1)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            if (!RpcApi.IsLogin)
                return true;

            OutputResultMessage(RpcCommand.Wallet.GetAddress, true, 0, "");

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
                string.Format("{0} [command option]\n", RpcCommand.Wallet.GetAccount) };

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
                OutputResultMessage(RpcCommand.Wallet.GetBalance, result.Result, result.Code, result.Message);

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
                string.Format("{0} [command option] <address>\n", RpcCommand.Wallet.GetAccount) };

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
                OutputResultMessage(RpcCommand.Wallet.GetAccount, result.Result, result.Code, result.Message);
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message + "\n\n" + e.StackTrace);
            }

            return true;
        }
    }
}
