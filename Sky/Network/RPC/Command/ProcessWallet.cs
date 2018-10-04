using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using Sky.Core;
using Sky.Cryptography;
using Sky.Wallets;

namespace Sky.Network.RPC.Command
{
    public partial class ProcessCommand
    {
        public static JObject OnCreateAccount(object obj, JArray parameters)
        {
            JObject json = new JObject();
            WalletAccount account = WalletAccount.CreateAccount();
            json["address"] = account.Address;
            json["addresshash"] = account.AddressHash.ToArray();
            json["privatekey"] = account.Key.PrivateKey.D.ToByteArray();
            json["publickey"] = account.Key.PublicKey.ToByteArray();
            return json;
        }

        public static JObject OnOpenAccount(object obj, JArray parameters)
        {
            JObject json = new JObject();
            return json;
        }

        public static JObject OnCloseAccount(object obj, JArray parameters)
        {
            JObject json = new JObject();
            return json;
        }

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
            return json;
        }

        public static JObject OnSendTo(object obj, JArray parameters)
        {
            JObject json = new JObject();
            LocalNode localNode = obj as LocalNode;

            ECKey key = new ECKey(parameters[1].ToObject<byte[]>(), true);
            WalletAccount from_account = new WalletAccount(key.PrivateKey.D.ToByteArray());
            UInt160 to_address = UInt160.FromHexString(parameters[2].Value<string>(), false);
            Fixed8 value = Fixed8.Parse(parameters[4].ToString());

            if (from_account.GetBalance() >= value)
            {
                TransferTransaction trans = new TransferTransaction()
                {
                    From = UInt160.FromHexString(parameters[0].Value<string>(), false),
                    To = new Dictionary<UInt160, Fixed8> { { to_address, value } }
                };

                Transaction tx = new Transaction(eTransactionType.TransferTransaction, DateTime.UtcNow.ToTimestamp(), trans);
                tx.Sign(from_account);

                localNode.AddTransaction(tx);
            }
            else
            {
                //json = CreateErrorResponse(null, 0, "");
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
