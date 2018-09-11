using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using Sky;
using Sky.Network.RPC.Command;
using SkyCLI.Network;

namespace SkyCLI.Commands
{
    public class WalletCommand : BaseCommand
    {
        public static bool OnCreateAccount(string[] parameters)
        {
            JObject obj = MakeCommand(Config.BlockVersion, RpcCommands.Wallet.CreateAccount, new JArray());
            obj = RcpClient.RequestPostAnsyc(Program.url, obj.ToString()).Result;

            return true;
        }

        public static bool OnOpenAccount(string[] parameters)
        {
            JObject obj = MakeCommand(Config.BlockVersion, RpcCommands.Wallet.OpenAccount, new JArray());
            obj = RcpClient.RequestPostAnsyc(Program.url, obj.ToString()).Result;

            return true;
        }

        public static bool OnCloseAccount(string[] parameters)
        {
            JObject obj = MakeCommand(Config.BlockVersion, RpcCommands.Wallet.CloseAccount, new JArray());
            obj = RcpClient.RequestPostAnsyc(Program.url, obj.ToString()).Result;

            return true;
        }

        public static bool OnGetAccount(string[] parameters)
        {
            JObject obj = MakeCommand(Config.BlockVersion, RpcCommands.Wallet.GetAccount, new JArray());
            obj = RcpClient.RequestPostAnsyc(Program.url, obj.ToString()).Result;

            return true;
        }

        public static bool OnGetAddress(string[] parameters)
        {
            JObject obj = MakeCommand(Config.BlockVersion, RpcCommands.Wallet.GetAddress, new JArray());
            obj = RcpClient.RequestPostAnsyc(Program.url, obj.ToString()).Result;

            return true;
        }

        public static bool OnGetBalance(string[] parameters)
        {
            JObject obj = MakeCommand(Config.BlockVersion, RpcCommands.Wallet.GetBalance, new JArray());
            obj = RcpClient.RequestPostAnsyc(Program.url, obj.ToString()).Result;

            return true;
        }

        public static bool OnSendTo(string[] parameters)
        {
            JObject obj = MakeCommand(Config.BlockVersion, RpcCommands.Wallet.SendTo, new JArray());
            obj = RcpClient.RequestPostAnsyc(Program.url, obj.ToString()).Result;

            return true;
        }

        public static bool OnFreezeBalance(string[] parameters)
        {
            JObject obj = MakeCommand(Config.BlockVersion, RpcCommands.Wallet.FreezeBalance, new JArray());
            obj = RcpClient.RequestPostAnsyc(Program.url, obj.ToString()).Result;

            return true;
        }

        public static bool OnUnfreezeBalance(string[] parameters)
        {
            JObject obj = MakeCommand(Config.BlockVersion, RpcCommands.Wallet.UnfreezeBalance, new JArray());
            obj = RcpClient.RequestPostAnsyc(Program.url, obj.ToString()).Result;

            return true;
        }

        public static bool OnVoteWitness(string[] parameters)
        {
            JObject obj = MakeCommand(Config.BlockVersion, RpcCommands.Wallet.VoteWitness, new JArray());
            obj = RcpClient.RequestPostAnsyc(Program.url, obj.ToString()).Result;

            return true;
        }
    }
}
    