using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sky;
using Sky.Core;
using Sky.Cryptography;
using Sky.Network.RPC.Command;
using Sky.Wallets;
using Sky.Wallets.KeyStore;
using SkyCLI.Network;
using SkyCLI.Shell;

namespace SkyCLI.Commands
{
    public class WalletCommand : BaseCommand
    {
        public static bool OnCreateAccount(string[] parameters)
        {
            string[] usage = new string[] { string.Format(
                "{0} [command option] <path>\n"
                , RpcCommand.Wallet.CreateAccount) };
            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters.Length == 1 || parameters.Length > 3)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            int index = 1;
            if (parameters.Length > index)
            {
                string option = parameters[index];
                if (option.ToLower().Equals("-help") || option.ToLower().Equals("-h"))
                {
                    OutputHelpMessage(usage, null, command_option, null);
                    index++;
                    return true;
                }
            }

            Console.Write("Password : ");
            string password = ConsoleServiceBase.ReadPasswordString();
            Console.Write("Confirm password : ");
            string confirm = ConsoleServiceBase.ReadPasswordString();

            if (!password.Equals(confirm))
            {
                Console.WriteLine("Password do not match.");
                return true;
            }

            string path = parameters[1].Contains(".keystore") ? parameters[1] : parameters[1] + ".keystore";
            WalletAccount account = WalletAccount.CreateAccount();

            if (!KeyStoreService.GenerateKeyStore(path, password, account.Key.PrivateKeyBytes, account.Address))
            {
                Console.WriteLine("Fail to generate keystore file.");
                return true;
            }

            Console.WriteLine("Address : {0}", account.Address);
            Console.WriteLine("PrivateKey : {0}", account.Key.PrivateKeyBytes.ToHexString());

            Program.Wallet = account;

            return true;
        }

        public static bool OnOpenAccount(string[] parameters)
        {
            string[] usage = new string[] { string.Format(
                "{0} [command option] <path>\n"
                , RpcCommand.Wallet.OpenAccount) };
            string[] command_option = new string[] { HelpCommandOption.Help };;

            if (parameters.Length == 1 || parameters.Length > 3)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            int index = 1;
            if (parameters.Length > index)
            {
                string option = parameters[index];
                if (option.ToLower().Equals("-help") || option.ToLower().Equals("-h"))
                {
                    OutputHelpMessage(usage, null, command_option, null);
                    index++;
                    return true;
                }
            }

            string path = parameters[1].Contains(".keystore") ? parameters[1] : parameters[1] + ".keystore";
            if (!File.Exists(path))
            {
                Console.WriteLine(string.Format("Not found file : [0]", path));
                return true;
            }

            JObject json;
            using (var file = File.OpenText(path))
            {
                string data = file.ReadToEnd();
                json = JObject.Parse(data);
            }

            Console.Write("Password : ");
            string password = ConsoleServiceBase.ReadPasswordString();

            KeyStore keystore = new KeyStore();
            keystore = JsonConvert.DeserializeObject<KeyStore>(json.ToString());

            byte[] privatekey = null;
            if (!KeyStoreService.DecryptKeyStore(password, keystore, out privatekey))
            {
                Console.WriteLine("Fail to decrypt keystore file.");
                return true;
            }

            Program.Wallet = new WalletAccount(privatekey);

            string message = Program.Wallet != null ?
                                string.Format("Address : {0}", Program.Wallet.Address.ToString()) : "Load fail to wallet account";

            Console.WriteLine(message);
            
            return true;
        }

        public static bool OnCloseAccount(string[] parameters)
        {
            Program.Wallet = null;
            Console.WriteLine("Close account");
            return true;
        }

        public static bool OnGetAccount(string[] parameters)
        {
            JObject obj = MakeCommand(Config.BlockVersion, RpcCommand.Wallet.GetAccount, new JArray());
            obj = RcpClient.RequestPostAnsyc(Program.url, obj.ToString()).Result;

            return true;
        }

        public static bool OnGetAddress(string[] parameters)
        {
            WalletAccount account = new WalletAccount(Sky.Cryptography.Helper.SHA256(Encoding.Default.GetBytes("1")));
            KeyStoreService.GenerateKeyStore("1.keystore", "1", account.Key.PrivateKeyBytes, account.Address);

            account = new WalletAccount(Sky.Cryptography.Helper.SHA256(Encoding.Default.GetBytes("2")));
            KeyStoreService.GenerateKeyStore("2.keystore", "1", account.Key.PrivateKeyBytes, account.Address);

            account = new WalletAccount(Sky.Cryptography.Helper.SHA256(Encoding.Default.GetBytes("3")));
            KeyStoreService.GenerateKeyStore("3.keystore", "1", account.Key.PrivateKeyBytes, account.Address);

            account = new WalletAccount(Sky.Cryptography.Helper.SHA256(Encoding.Default.GetBytes("4")));
            KeyStoreService.GenerateKeyStore("4.keystore", "1", account.Key.PrivateKeyBytes, account.Address);

            account = new WalletAccount(Sky.Cryptography.Helper.SHA256(Encoding.Default.GetBytes("5")));
            KeyStoreService.GenerateKeyStore("5.keystore", "1", account.Key.PrivateKeyBytes, account.Address);

            return true;
        }

        public static bool OnGetBalance(string[] parameters)
        {
            if (Program.Wallet == null)
            {
                Console.WriteLine("Not loaded wallet account");
                return true;
            }

            string[] usage = new string[] { string.Format(
                "{0} [command option]\n"
                , RpcCommand.Wallet.GetBalance) };
            string[] command_option = new string[] { HelpCommandOption.Help };;

            if (parameters.Length > 2)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            int index = 1;
            if (parameters.Length > index)
            {
                string option = parameters[index];
                if (option.ToLower().Equals("-help") || option.ToLower().Equals("-h"))
                {
                    OutputHelpMessage(usage, null, command_option, null);
                    index++;
                    return true;
                }
            }

            JArray param = new JArray() { Program.Wallet.AddressHash.ToString() };
            SendCommand(Config.BlockVersion, RpcCommand.Wallet.GetBalance, param);

            return true;
        }

        public static bool OnSendTo(string[] parameters)
        {
            if (Program.Wallet == null)
            {
                Console.WriteLine("Not loaded wallet account");
                return true;
            }

            string[] usage = new string[] { string.Format(
                    "{0} [command option] <to address> <balance>\n"
                    , RpcCommand.Wallet.SendTo) };
            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters.Length == 1 || parameters.Length > 4)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            int index = 1;
            if (parameters.Length > index)
            {
                string option = parameters[index];
                if (option.ToLower().Equals("-help") || option.ToLower().Equals("-h"))
                {
                    OutputHelpMessage(usage, null, command_option, null);
                    index++;
                    return true;
                }
            }

            UInt160 to_address = WalletAccount.ToAddressHash(parameters[1]);
            Fixed8 value = Fixed8.Parse(parameters[2]);

            TransferTransaction trans = new TransferTransaction()
            {
                From = Program.Wallet.AddressHash,
                To = new Dictionary<UInt160, Fixed8> { { to_address, value } }
            };

            Transaction tx = new Transaction(eTransactionType.TransferTransaction, DateTime.UtcNow.ToTimestamp(), trans);
            tx.Sign(Program.Wallet);

            JArray param = new JArray(tx.ToArray());
            SendCommand(Config.BlockVersion, RpcCommand.Wallet.SendTo, param);

            return true;
        }

        public static bool OnLockBalance(string[] parameters)
        {
            if (Program.Wallet == null)
            {
                Console.WriteLine("Not loaded wallet account");
                return true;
            }

            string[] usage = new string[] { string.Format(
                    "{0} [command option] <balance>\n"
                    , RpcCommand.Wallet.LockBalance) };
            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters.Length == 1 || parameters.Length > 3)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            int index = 1;
            if (parameters.Length > index)
            {
                string option = parameters[index];
                if (option.ToLower().Equals("-help") || option.ToLower().Equals("-h"))
                {
                    OutputHelpMessage(usage, null, command_option, null);
                    index++;
                    return true;
                }
            }

            Fixed8 value = Fixed8.Parse(parameters[1]);

            LockTransaction trans = new LockTransaction()
            {
                From = Program.Wallet.AddressHash,
                LockValue = value
            };

            Transaction tx = new Transaction(eTransactionType.LockTransaction, DateTime.UtcNow.ToTimestamp(), trans);
            tx.Sign(Program.Wallet);

            JArray param = new JArray(tx.ToArray());
            SendCommand(Config.BlockVersion, RpcCommand.Wallet.LockBalance, param);

            return true;
        }

        public static bool OnUnlockBalance(string[] parameters)
        {
            if (Program.Wallet == null)
            {
                Console.WriteLine("Not loaded wallet account");
                return true;
            }

            string[] usage = new string[] { string.Format(
                    "{0} [command option]\n"
                    , RpcCommand.Wallet.UnlockBalance) };
            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters.Length > 2)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            int index = 1;
            if (parameters.Length > index)
            {
                string option = parameters[index];
                if (option.ToLower().Equals("-help") || option.ToLower().Equals("-h"))
                {
                    OutputHelpMessage(usage, null, command_option, null);
                    index++;
                    return true;
                }
            }

            UnlockTransaction trans = new UnlockTransaction()
            {
                From = Program.Wallet.AddressHash
            };

            Transaction tx = new Transaction(eTransactionType.UnlockTransaction, DateTime.UtcNow.ToTimestamp(), trans);
            tx.Sign(Program.Wallet);

            JArray param = new JArray(tx.ToArray());
            SendCommand(Config.BlockVersion, RpcCommand.Wallet.UnlockBalance, param);

            return true;
        }

        public static bool OnVoteWitness(string[] parameters)
        {
            if (Program.Wallet == null)
            {
                Console.WriteLine("Not loaded wallet account");
                return true;
            }

            string[] usage = new string[] { string.Format(
                    "{0} [command option] <to address> <vote balance> <to address> <vote balance> ...\n"
                    , RpcCommand.Wallet.VoteWitness) };
            string[] command_option = new string[] { HelpCommandOption.Help };

            if (parameters.Length == 1)
            {
                OutputHelpMessage(usage, null, command_option, null);
                return true;
            }

            int index = 1;
            if (parameters.Length > index)
            {
                string option = parameters[index];
                if (option.ToLower().Equals("-help") || option.ToLower().Equals("-h"))
                {
                    OutputHelpMessage(usage, null, command_option, null);
                    index++;
                    return true;
                }
            }

            Dictionary<UInt160, Fixed8> votes = new Dictionary<UInt160, Fixed8>();
            for (int i = 0; i < parameters.Length - 1; i+=2)
                votes.Add(WalletAccount.ToAddressHash(parameters[i]), Fixed8.Parse(parameters[i]));

            VoteTransaction trans = new VoteTransaction()
            {
                From = Program.Wallet.AddressHash,
                Votes = votes
            };

            Transaction tx = new Transaction(eTransactionType.VoteTransaction, DateTime.UtcNow.ToTimestamp(), trans);
            tx.Sign(Program.Wallet);

            JArray param = new JArray(tx.ToArray());
            SendCommand(Config.BlockVersion, RpcCommand.Wallet.VoteWitness, param);

            return true;
        }
    }
}