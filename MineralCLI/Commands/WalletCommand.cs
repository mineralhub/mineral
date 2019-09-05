using Google.Protobuf;
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

namespace MineralCLI.Commands
{
    using ResponseCode = TransactionSignWeight.Types.Result.Types.response_code;

    public sealed class WalletCommand : BaseCommand
    {
        public static bool RegistWallet(string[] parameters)
        {
            string[] usage = new string[] {
                string.Format("{0} [command option] <path>\n", RpcCommandType.RegistWallet) };

            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters == null || parameters.Length != 1)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            string password = CommandLineUtil.ReadPasswordString("Please input wallet password");

            ECKey key = new ECKey();
            if (!KeyStoreService.GenerateKeyStore(WalletApi.FILE_PATH + @"\" + parameters[0],
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

            Program.Wallet = keystore;
            Console.WriteLine("Login success.");

            return true;
        }

        public static bool Logout(string[] parameters)
        {
            Program.Wallet = null;

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
            SendCommand(method, new JArray() { address });

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

            if (WalletApi.IsLogin)
            {
                return true;
            }

            try
            {
                TransferContract contract =
                    WalletApi.CreateTransaferContract(Wallet.Base58ToAddress(Program.Wallet.Address),
                                                      Wallet.Base58ToAddress(parameters[1]),
                                                      long.Parse(parameters[2]));

                JObject receive_contract = 
                    SendCommand(RpcCommandType.CreateTransaction, new JArray() { contract.ToByteArray() });

                if (receive_contract.TryGetValue("error", out JToken value))
                {
                    OutputErrorMessage(receive_contract["code"].ToObject<int>(), receive_contract["message"].ToObject<string>());
                    return true;
                }

                TransactionExtention transaction_extention = TransactionExtention.Parser.ParseFrom(receive_contract["result"].ToObject<byte[]>());
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

                    JObject receive_weight =
                        SendCommand(RpcCommandType.GetTransactionSignWeight, new JArray() { transaction.ToByteArray() });

                    TransactionSignWeight weight = TransactionSignWeight.Parser.ParseFrom(receive_weight["result"].ToObject<byte[]>());
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
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message + "\n\n" + e.StackTrace);
            }

            return true;
        }
    }
}
