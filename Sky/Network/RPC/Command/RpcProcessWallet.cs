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
        public static JObject OnGetAccount(object obj, RpcCommand.ParamType type, JArray parameters)
        {
            JObject json = new JObject();
            return json;
        }

        public static JObject OnGetAddress(object obj, RpcCommand.ParamType type, JArray parameters)
        {
            JObject json = new JObject();
            return json;
        }

        public static JObject OnGetBalance(object obj, RpcCommand.ParamType type, JArray parameters)
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

        public static JObject OnSendTo(object obj, byte[] parameters)
        {
            JObject json = new JObject();
            LocalNode localNode = obj as LocalNode;

            Transaction tx = Transaction.DeserializeFrom(parameters);
            if (tx != null)
            {
                if (tx.VerifyBlockchain())
                    localNode.AddTransaction(tx);
                else
                    json = RpcCommand.CreateErrorResult(null, 0, "Not enough balance");
            }
            else
            {
                json = RpcCommand.CreateErrorResult(null, 0, "Invalid trasaction data");
            }

            return json;
        }

        public static JObject OnSendTo(object obj, WalletAccount from_account, UInt160 to_address, Fixed8 balance)
        {
            JObject json = new JObject();
            LocalNode localNode = obj as LocalNode;

            TransferTransaction trans = new TransferTransaction()
            {
                From = from_account.AddressHash,
                To = new Dictionary<UInt160, Fixed8> { { to_address, balance } }
            };
            trans.CalcFee();

            if (trans.VerifyBlockchain(Blockchain.Instance.storage))
            {
                Transaction tx = new Transaction(eTransactionType.TransferTransaction, DateTime.UtcNow.ToTimestamp(), trans);
                tx.Sign(from_account);
                localNode.AddTransaction(tx);
            }
            else
            {
                json = RpcCommand.CreateErrorResult(null, (int)trans.TxResult, trans.TxResult.ToString());
            }

            return json;
        }

        public static JObject OnSendTo(object obj, RpcCommand.ParamType type, JArray parameters)
        {
            JObject json = new JObject();

            if (type == RpcCommand.ParamType.Serialize)
            {
                byte[] value = parameters[0].ToObject<byte[]>();
                json = OnSendTo(obj, value);
            }
            else
            {
                WalletAccount from_account = new WalletAccount(parameters[0].ToObject<byte[]>());
                UInt160 to_address = WalletAccount.ToAddressHash(parameters[1].Value<string>());
                Fixed8 balance = Fixed8.Parse(parameters[2].ToString());

                json = OnSendTo(obj, from_account, to_address, balance);
            }
            return json;
        }

        public static JObject OnFreezeBalance(object obj, RpcCommand.ParamType type, JArray parameters)
        {
            JObject json = new JObject();
            return json;
        }

        public static JObject OnUnfreezeBalance(object obj, RpcCommand.ParamType type, JArray parameters)
        {
            JObject json = new JObject();
            return json;
        }

        public static JObject OnVoteWitness(object obj, RpcCommand.ParamType type, JArray parameters)
        {
            JObject json = new JObject();
            return json;
        }
    }
}
