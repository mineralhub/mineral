using System;
using System.Collections.Generic;
using System.IO;
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
            if (parameters.Length != 2)
            {
                ErrorParamMessage(RpcCommands.Wallet.CreateAccount);
                return true;
            }

            JObject obj = MakeCommand(Config.BlockVersion, RpcCommands.Wallet.CreateAccount, new JArray());
            obj = RcpClient.RequestPostAnsyc(Program.url, obj.ToString()).Result;

            if (obj.ContainsKey("error"))
            {
                Console.WriteLine("Fail to create account.");
                return true;
            }

            string path = parameters[1] + ".json";
            using (var file = File.CreateText(path))
            {
                file.Write(obj);
                file.Flush();
            }

            JToken key;
            JObject result = JObject.Parse(obj["result"].ToString());
            if (result.TryGetValue("privatekey", out key))
            {
                Program.Wallet = new Sky.Wallets.WalletAccount(key.ToObject<byte[]>());
            }

            return true;
        }

        public static bool OnOpenAccount(string[] parameters)
        {
            if (parameters.Length != 2)
            {
                ErrorParamMessage(RpcCommands.Wallet.OpenAccount);
                return true;
            }

            JObject json;
            string path = parameters[1];

            if (!File.Exists(path))
            {
                Console.WriteLine(string.Format("Not found file : [0]", path));
                return true;
            }

            var file = File.OpenText(path);
            string data = file.ReadToEnd();
            json = JObject.Parse(data);

            JToken key;
            JObject result = JObject.Parse(json["result"].ToString());
            if (result.TryGetValue("privatekey", out key))
            {
                Program.Wallet = new Sky.Wallets.WalletAccount(key.ToObject<byte[]>());
            }

            string message = Program.Wallet != null ?
                                string.Format("Address : {0}", Program.Wallet.Address.ToString()) : "Load fail to wallet account";
            Console.WriteLine(message);

            return true;
        }

        public static bool OnCloseAccount(string[] parameters)
        {
            Program.Wallet = null;

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
            if (Program.Wallet == null)
            {
                Console.WriteLine("Not load wallet account");
                return true;
            }

            JArray param = new JArray();
            param.Add(Program.Wallet.Key.PrivateKey.D.ToByteArray());

            JObject obj = MakeCommand(Config.BlockVersion, RpcCommands.Wallet.GetBalance, param);
            obj = RcpClient.RequestPostAnsyc(Program.url, obj.ToString()).Result;

            TestOutput(obj);

            return true;
        }

        public static bool OnSendTo(string[] parameters)
        {
            if (parameters.Length != 3)
            {
                ErrorParamMessage(RpcCommands.Wallet.SendTo);
                return true;
            }

            if (Program.Wallet == null)
            {
                Console.WriteLine("Not load wallet account");
                return true;
            }

            JArray param = new JArray(new ArraySegment<string>(parameters, 1, parameters.Length - 1));
            param.AddFirst(Program.Wallet.Key.PrivateKey.D.ToByteArray());

            JObject obj = MakeCommand(Config.BlockVersion, RpcCommands.Wallet.SendTo, param);
            obj = RcpClient.RequestPostAnsyc(Program.url, obj.ToString()).Result;

            TestOutput(obj);

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
    