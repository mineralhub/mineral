﻿using Google.Protobuf;
using Mineral;
using Mineral.CommandLine;
using Mineral.Core;
using Mineral.Core.Net.RpcHandler;
using Mineral.Cryptography;
using Mineral.Wallets.KeyStore;
using MineralCLI.Api;
using MineralCLI.Exception;
using MineralCLI.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace MineralCLI.Commands
{
    using ResponseCode = TransactionSignWeight.Types.Result.Types.response_code;

    public sealed class WalletCommand : BaseCommand
    {
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

            string password = CommandLineUtil.ReadPasswordString("Please input your password.");
            string privatekey = CommandLineUtil.ReadString("Please input your private key.");

            if (WalletApi.ImportWallet(password, privatekey))
            {
                Logout(null);
            }

            return true;
        }

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

            if (!WalletApi.IsLogin)
            {
                return true;
            }

            string password = CommandLineUtil.ReadPasswordString("Please input your password.");
            WalletApi.BackupWallet(password);

            return true;
        }

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

            string password = CommandLineUtil.ReadPasswordString("Please input wallet password");
            string confirm = CommandLineUtil.ReadPasswordString("Please input confirm wallet password");

            if (!password.Equals(confirm))
            {
                Console.WriteLine("Confirm password does not match");
                return true;
            }

            ECKey key = new ECKey();
            PathUtil.MakeDirectory(WalletApi.FILE_PATH);
            if (!KeyStoreService.GenerateKeyStore(WalletApi.FILE_PATH,
                                                  password,
                                                  key.PrivateKey,
                                                  Wallet.AddressToBase58(Wallet.PublickKeyToAddress(key.PublicKey))))
            {
                Console.WriteLine("Failed to RegistWallet.");
                return true;
            }

            Console.WriteLine("Register wallet complete.");
            Logout(null);

            return true;
        }

        public static bool Login(string[] parameters)
        {
            Logout(null);

            KeyStore keystore = WalletApi.SelectKeyStore();

            string password = CommandLineUtil.ReadPasswordString("Please input your password.");
            if (!KeyStoreService.CheckPassword(password, keystore))
            {
                Console.WriteLine("Login Fail.");
                return true;
            }

            WalletApi.KeyStore = keystore;
            Console.WriteLine("Login success.");

            return true;
        }

        public static bool Logout(string[] parameters)
        {
            WalletApi.KeyStore = null;

            return true;
        }

        public static bool GetAccount(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <path>\n", RpcCommandType.GetAccount) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 2)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            string method = parameters[0].ToLower();
            string address = parameters[1];

            JObject receive = SendCommand(method, new JArray() { address });
            if (receive.TryGetValue("error", out JToken value))
            {
                OutputErrorMessage(value["code"].ToObject<int>(), value["message"].ToObject<string>());
                return true;
            }


            Account account = Account.Parser.ParseFrom(receive["result"].ToObject<byte[]>());
            Console.WriteLine(PrintUtil.PrintAccount(account));

            return true;
        }

        public static bool SendCoin(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <address> <amount>\n", RpcCommandType.SendCoin) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 3)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            if (!WalletApi.IsLogin)
            {
                return true;
            }

            try
            {
                TransferContract contract =
                    WalletApi.CreateTransaferContract(Wallet.Base58ToAddress(WalletApi.KeyStore.Address),
                                                      Wallet.Base58ToAddress(parameters[1]),
                                                      long.Parse(parameters[2]));

                JObject receive = 
                    SendCommand(RpcCommandType.CreateTransaction, new JArray() { contract.ToByteArray() });

                if (receive.TryGetValue("error", out JToken value))
                {
                    OutputErrorMessage(value["code"].ToObject<int>(), value["message"].ToObject<string>());
                    return true;
                }

                TransactionExtention transaction_extention = TransactionExtention.Parser.ParseFrom(receive["result"].ToObject<byte[]>());
                Return ret = transaction_extention.Result;
                if (!ret.Result)
                {
                    Console.WriteLine("Code : " + ret.Code);
                    Console.WriteLine("Message : " + ret.Message.ToStringUtf8());
                    return true;
                }

                Transaction transaction = WalletApi.InitSignatureTransaction(transaction_extention.Transaction);
                while (true)
                {
                    transaction = WalletApi.SignatureTransaction(transaction);
                    Console.WriteLine("current transaction hex string is " + transaction.ToByteArray().ToHexString());

                    receive = SendCommand(RpcCommandType.GetTransactionSignWeight, new JArray() { transaction.ToByteArray() });

                    TransactionSignWeight weight = TransactionSignWeight.Parser.ParseFrom(receive["result"].ToObject<byte[]>());
                    if (weight.Result.Code == ResponseCode.EnoughPermission)
                    {
                        break;
                    }
                    else if (weight.Result.Code == ResponseCode.NotEnoughPermission)
                    {
                        Console.WriteLine("Current signWeight is:");
                        Console.WriteLine(PrintUtil.PrintTransactionSignWeight(weight));
                        Console.WriteLine("Please confirm if continue add signature enter y or Y, else any other");

                        if (!CommandLineUtil.Confirm())
                        {
                            throw new CancelException("User cancelled");
                        }
                        continue;
                    }

                    throw new CancelException(weight.Result.Message);
                }

                receive = SendCommand(RpcCommandType.BroadcastTransaction, new JArray() { transaction.ToByteArray() });
                ret = Return.Parser.ParseFrom(receive["result"].ToObject<byte[]>());

                int retry = 10;
                while (ret.Result == false && ret.Code == Return.Types.response_code.ServerBusy && retry > 0)
                {
                    retry--;
                    receive = SendCommand(RpcCommandType.BroadcastTransaction, new JArray() { transaction.ToByteArray() });
                    ret = Return.Parser.ParseFrom(receive["result"].ToObject<byte[]>());
                    Console.WriteLine("Retry broadcast : " + (11 - retry));

                    Thread.Sleep(1000);
                }

                if (ret.Result)
                {
                    Console.WriteLine(
                        string.Format("Send {0} drop to {1} + successful. ", long.Parse(parameters[2]), parameters[1]));
                }
                else
                {
                    Console.WriteLine("Code : " + ret.Code);
                    Console.WriteLine("Message : " + ret.Message);
                    Console.WriteLine(
                        string.Format("Send {0} drop to {1} + failed. ", long.Parse(parameters[2]), parameters[1]));
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message + "\n\n" + e.StackTrace);
            }

            return true;
        }
    }
}
