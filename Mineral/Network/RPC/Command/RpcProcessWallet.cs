using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using Mineral.Core;
using Mineral.Cryptography;
using Mineral.Wallets;
using Mineral.Utils;
using Mineral.Core.State;

namespace Mineral.Network.RPC.Command
{
    public partial class RpcProcessCommand
    {
        public static JObject OnGetBalance(object obj, JArray parameters)
        {
            JObject json = new JObject();

            string address = parameters[0].ToString();
            UInt160 addresHash = WalletAccount.ToAddressHash(address);
            json["address"] = address;
            json["addresshash"] = addresHash.ToString();
            json["balance"] = WalletAccount.GetBalance(addresHash).ToString();
            json["lock_balance"] = WalletAccount.GetLockBalance(addresHash).ToString();
            json["total_balance"] = WalletAccount.GetTotalBalance(addresHash).ToString();

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

        public static JObject OnGetVoteWitness(object obj, JArray parameters)
        {
            UInt160 addressHash = WalletAccount.ToAddressHash(parameters[0].ToString());
            AccountState state = BlockChain.Instance.GetAccountState(addressHash);

            JObject json = new JObject();
            json["votes"] = new JArray();
            foreach (KeyValuePair<UInt160, Fixed8> pair in state.Votes)
                (json["votes"] as JArray).Add(pair.ToString());

            return json;
        }

        public static JObject OnAddTransaction(object obj, JArray parameters)
        {
            return ProcessTransaction(obj as LocalNode, parameters[0].ToObject<byte[]>());
        }

    }
}
