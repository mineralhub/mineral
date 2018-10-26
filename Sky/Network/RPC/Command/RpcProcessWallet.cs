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
            JObject json = new JObject();
            LocalNode localNode = obj as LocalNode;

            Transaction tx = Transaction.DeserializeFrom(parameters.ToObject<byte[]>());
            if (tx != null)
            {
                if (tx.Verify() && tx.VerifyBlockchain())
                {
                    localNode.AddTransaction(tx);
                }
                else
                {
                    json = RpcCommand.CreateErrorResult(null, (int)tx.TxResult, tx.TxResult.ToString());
                }
            }
            else
            {
                json = RpcCommand.CreateErrorResult(null, 0, "Invalid trasaction data");
            }

            return json;
        }

        public static JObject OnLockBalance(object obj, JArray parameters)
        {
            JObject json = new JObject();
            LocalNode localNode = obj as LocalNode;

            Transaction tx = Transaction.DeserializeFrom(parameters.ToObject<byte[]>());
            if (tx != null)
            {
                if (tx.Verify() && tx.VerifyBlockchain())
                    localNode.AddTransaction(tx);
                else
                    json = RpcCommand.CreateErrorResult(null, (int)tx.TxResult, tx.TxResult.ToString());
            }
            else
            {
                json = RpcCommand.CreateErrorResult(null, 0, "Invalid trasaction data");
            }

            return json;
        }

        public static JObject OnUnlockBalance(object obj, JArray parameters)
        {
            JObject json = new JObject();
            LocalNode localNode = obj as LocalNode;

            Transaction tx = Transaction.DeserializeFrom(parameters.ToObject<byte[]>());
            if (tx != null)
            {
                if (tx.Verify() && tx.VerifyBlockchain())
                    localNode.AddTransaction(tx);
                else
                    json = RpcCommand.CreateErrorResult(null, (int)tx.TxResult, tx.TxResult.ToString());
            }
            else
            {
                json = RpcCommand.CreateErrorResult(null, 0, "Invalid trasaction data");
            }

            return json;
        }

        public static JObject OnVoteWitness(object obj, JArray parameters)
        {
            JObject json = new JObject();
            return json;
        }
    }
}
