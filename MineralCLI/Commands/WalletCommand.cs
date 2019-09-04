using Google.Protobuf;
using Mineral;
using Mineral.Core;
using Mineral.Core.Net.RpcHandler;
using MineralCLI.Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace MineralCLI.Commands
{
    public sealed class WalletCommand : BaseCommand
    {
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

            try
            {
                TransferContract contract =
                    WalletApi.CreateTransaferContract(Wallet.Base58ToAddress("MQGFD8zDTk9vGWmP6E9FzKLd8mdmFVzWBQ"),
                                                      Wallet.Base58ToAddress(parameters[1]),
                                                      long.Parse(parameters[2]));

                JObject receive_contract = 
                    SendCommand(RpcCommandType.CreateTransaction, new JArray() { contract.ToByteArray() });

                if (receive_contract.TryGetValue("error", out JToken value))
                {
                    OutputErrorMessage(receive_contract["code"].ToObject<int>(), receive_contract["message"].ToObject<string>());
                    return true;
                }

                Transaction transaction = Transaction.Parser.ParseFrom(receive_contract["result"].ToObject<byte[]>());

                transaction = WalletApi.SignatureTransaction(transaction);
                
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message + "\n\n" + e.StackTrace);
            }

            return true;
        }
    }
}
