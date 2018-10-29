using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using Sky.Core;
using Sky.Cryptography;
using Sky.Wallets;

namespace Sky.Network.RPC.Command
{
    public partial class RpcProcessCommand
    {
        public static JObject OnGetAccount(object obj, JArray parameters)
        {
            JObject json = new JObject();
            return json;
        }

        public static JObject OnGetAddress(object obj, JArray parameters)
        {
            JObject json = new JObject();
            return json;
        }

        public static JObject OnGetBalance(object obj, JArray parameters)
        {
            JObject json = new JObject();

            WalletAccount acc = new WalletAccount(parameters[0].ToObject<byte[]>());
            string address = WalletAccount.ToAddress(acc.AddressHash);
            json["address"] = address;
            json["addresshash"] = acc.AddressHash.ToArray();
            json["balance"] = acc.GetBalance().ToString();
            json["lock_balance"] = acc.GetLockBalance().ToString();
            json["total_balance"] = acc.GetTotalBalance().ToString();

            return json;
        }

        public static JObject OnSendTo(object obj, JArray parameters)
        {
            return ProcessTransaction(obj as LocalNode, parameters[0].ToObject<byte[]>());
        }

        public static JObject OnLockBalance(object obj, JArray parameters)
        {
            return ProcessTransaction(obj as LocalNode, parameters[0].ToObject<byte[]>());
        }

        public static JObject OnUnlockBalance(object obj, JArray parameters)
        {
            return ProcessTransaction(obj as LocalNode, parameters[0].ToObject<byte[]>());
        }

        public static JObject OnVoteWitness(object obj, JArray parameters)
        {
            return ProcessTransaction(obj as LocalNode, parameters[0].ToObject<byte[]>());
        }
    }
}
