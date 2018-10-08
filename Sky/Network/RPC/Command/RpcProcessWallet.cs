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

            ECKey key = new ECKey(parameters[0].ToObject<byte[]>(), true);
            WalletAccount account = new WalletAccount(key.PrivateKey.D.ToByteArray());
            json["balance"] = account.GetBalance().ToString();

            return json;
        }

        public static JObject OnSendTo(object obj, JArray parameters)
        {
            JObject json = new JObject();
            LocalNode localNode = obj as LocalNode;

            WalletAccount from_account = new WalletAccount(parameters[0].ToObject<byte[]>());
            UInt160 to_address = WalletAccount.ToAddressHash(parameters[1].Value<string>());
            Fixed8 value = Fixed8.Parse(parameters[2].ToString());

            if (from_account.GetBalance() >= value)
            {
                TransferTransaction trans = new TransferTransaction()
                {
                    From = from_account.AddressHash,
                    To = new Dictionary<UInt160, Fixed8> { { to_address, value } }
                };

                Transaction tx = new Transaction(eTransactionType.TransferTransaction, DateTime.UtcNow.ToTimestamp(), trans);
                tx.Sign(from_account);

                localNode.AddTransaction(tx);
            }
            else
            {
                json = RpcCommand.CreateErrorResponse(null, 0, "Not enough balance");
            }

            return json;
        }

        public static JObject OnFreezeBalance(object obj, JArray parameters)
        {
            JObject json = new JObject();
            return json;
        }

        public static JObject OnUnfreezeBalance(object obj, JArray parameters)
        {
            JObject json = new JObject();
            return json;
        }

        public static JObject OnVoteWitness(object obj, JArray parameters)
        {
            JObject json = new JObject();
            return json;
        }
    }
}
