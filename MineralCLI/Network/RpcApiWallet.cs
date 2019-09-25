using Mineral;
using Mineral.CommandLine;
using Mineral.Common.Net.RPC;
using Mineral.Core;
using Mineral.Core.Net.RpcHandler;
using Mineral.Cryptography;
using Mineral.Utils;
using Mineral.Wallets.KeyStore;
using MineralCLI.Util;
using Newtonsoft.Json.Linq;
using Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MineralCLI.Network
{
    public partial class RpcApi
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public static RpcApiResult ImportWallet(string password, string privatekey)
        {
            if (password.IsNullOrEmpty() || privatekey.IsNullOrEmpty())
            {
                Console.WriteLine("Invalide password and privatekey");
                return new RpcApiResult(false, RpcMessage.INVALID_PASSWORD, "Invalide password and privatekey");
            }

            try
            {
                byte[] pk = privatekey.HexToBytes();
                if (pk.Length != 32)
                {
                    return new RpcApiResult(false, RpcMessage.INVALID_PRIVATEKEY, "Invalid privatekey. Privatekey must be 32 bytes.");
                }

                ECKey key = ECKey.FromPrivateKey(pk);
                string address = Wallet.AddressToBase58(Wallet.PublickKeyToAddress(key.PublicKey));

                if (!KeyStoreService.GenerateKeyStore(RpcApi.FILE_PATH,
                                                      password,
                                                      pk,
                                                      address))
                {
                    Console.WriteLine();
                    return new RpcApiResult(false, RpcMessage.INTERNAL_ERROR, "Faild to generate keystore file.");
                }
            }
            catch (System.Exception e)
            {
                throw e;
            }

            return RpcApiResult.Success;
        }

        public static RpcApiResult BackupWallet(string password)
        {
            if (password.IsNullOrEmpty())
            {
                return new RpcApiResult(false, RpcMessage.INVALID_PASSWORD, "Invalid password.");
            }

            if (!KeyStoreService.DecryptKeyStore(password, RpcApi.KeyStore, out byte[] privatekey))
            {
                Console.WriteLine("Fail to Decrypt keystore.");
                return new RpcApiResult(false, RpcMessage.INTERNAL_ERROR, "Fail to Decrypt keystore.");
            }

            Console.WriteLine(privatekey.ToHexString());

            return RpcApiResult.Success;
        }

        public static RpcApiResult RegisterWallet(string password)
        {
            ECKey key = new ECKey();
            PathUtil.MakeDirectory(RpcApi.FILE_PATH);

            if (!KeyStoreService.GenerateKeyStore(RpcApi.FILE_PATH,
                                                  password,
                                                  key.PrivateKey,
                                                  Wallet.AddressToBase58(Wallet.PublickKeyToAddress(key.PublicKey))))
            {
                return new RpcApiResult(false, RpcMessage.INTERNAL_ERROR, "Failed to generate keystore file.");
            }

            return RpcApiResult.Success;
        }

        public static RpcApiResult Login()
        {
            KeyStore keystore = RpcApi.SelectKeyStore();

            string password = CommandLineUtil.ReadPasswordString("Please input your password.");
            if (!KeyStoreService.CheckPassword(password, keystore))
            {
                Console.WriteLine("Login Fail.");
                return new RpcApiResult(false, RpcMessage.INVALID_PASSWORD, "Please check password.");
            }

            RpcApi.KeyStore = keystore;

            return RpcApiResult.Success;
        }

        public static RpcApiResult Logout()
        {
            RpcApi.KeyStore = null;

            return RpcApiResult.Success;
        }

        public static KeyStore SelectKeyStore()
        {
            DirectoryInfo info = new DirectoryInfo(FILE_PATH);
            if (!info.Exists)
            {
                return null;
            }

            FileInfo[] wallets = info.GetFiles();
            if (wallets.Length <= 0)
            {
                return null;
            }

            for (int i = 0; i < wallets.Length; i++)
            {
                Console.WriteLine("[" + (i + 1) + "]" + " Keystore file name : " + wallets[i].Name);
            }
            Console.WriteLine("Please input keystore file number.");

            FileInfo wallet = null;
            while (true)
            {
                int index = -1;
                string input = Console.ReadLine().Trim();
                try
                {
                    index = int.Parse(input);
                }
                catch (System.Exception)
                {
                    Console.WriteLine("Invalid number of " + input);
                    Console.WriteLine("Please choose again between 1 to " + wallets.Length);
                    continue;
                }

                if (index < 1 || index > wallets.Length)
                {
                    Console.WriteLine("Please choose again between 1 to " + wallets.Length);
                    continue;
                }

                wallet = wallets[index - 1];
                break;
            }

            try
            {
                KeyStore keystore = null;
                using (var file = File.OpenText(wallet.FullName))
                {
                    keystore = KeyStore.FromJson(file.ReadToEnd());
                }

                return keystore;
            }
            catch (System.Exception)
            {
                Console.WriteLine("load fail keystore file : " + wallet.FullName);
            }

            return null;
        }

        public static RpcApiResult GetAccount(string address, out Account account)
        {
            account = null;

            JObject receive = SendCommand(RpcCommand.Wallet.GetAccount, new JArray() { KeyStore.Address });
            if (receive.TryGetValue("error", out JToken value))
            {
                return new RpcApiResult(false, value["code"].ToObject<int>(), value["message"].ToObject<string>());
            }

            if (receive["result"].Type == JTokenType.Null)
            {
                account = new Account();
            }
            else
            {
                account = Account.Parser.ParseFrom(receive["result"].ToObject<byte[]>());
            }

            return RpcApiResult.Success;
        }

        public static RpcApiResult GetBalance(out long balance)
        {
            balance = 0;

            RpcApiResult result = GetAccount(KeyStore.Address, out Account account);
            if (result.Result)
            {
                balance = account.Balance;
            }

            return result;
        }

        public static RpcApiResult ListWitness(out WitnessList witnesses)
        {
            witnesses = null;

            JObject receive = SendCommand(RpcCommand.Wallet.ListWitness, new JArray() { });
            if (receive.TryGetValue("error", out JToken value))
            {
                return new RpcApiResult(false, value["code"].ToObject<int>(), value["message"].ToObject<string>());
            }

            witnesses = WitnessList.Parser.ParseFrom(receive["result"].ToObject<byte[]>());

            return RpcApiResult.Success;
        }
        #endregion
    }
}