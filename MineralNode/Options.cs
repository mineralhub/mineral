using Mineral;
using MineralNode.CommandLine;
using MineralNode.CommandLine.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MineralNode
{
    public class Options
    {
        public readonly string[] arg_indicator = new string[] { "-", "--" };
        private Dictionary<string, string> data = new Dictionary<string, string>();
        private Dictionary<string, string> error = new Dictionary<string, string>();

        public OptionDefault Default { get; set; }
        public OptionWallet Wallet { get; set; }

        public bool IsHelp { get; private set; }
        public bool IsValid { get { return IsValidOption(); } }

        public Options(string[] args)
        {
            Dictionary<string, string> arguments = ArgumentsParser.ParseArguments(args, arg_indicator);

            if (arguments.ContainsKey(OptionName.H) || arguments.ContainsKey(OptionName.Help))
            {
                IsHelp = true;
                arguments.Remove(OptionName.H);
                arguments.Remove(OptionName.Help);
            }

            Default = new OptionDefault();
            Wallet = new OptionWallet();

            ApplyArgument<OptionDefault, DefaultAttribute>(arguments);
            ApplyArgument<OptionWallet, WalletAttribute>(arguments);
            error = arguments;
        }

        private bool IsValidOption()
        {
            if (data == null)
            {
                Logger.Warning("option data not loaded.");
                return false;
            }

            if (error.Count > 0)
            {
                System.Collections.IEnumerator em = error.Keys.GetEnumerator();
                em.MoveNext();
                string message = "Invalid option : " + em.Current;
                Logger.Warning(message);
                return false;
            }

            if (data.ContainsKey(OptionName.KeyStoreDir) && data.ContainsKey(OptionName.PrivateKey))
            {
                string message = OptionName.KeyStoreDir + "and " + OptionName.PrivateKey + "can't used together.";
                Logger.Warning(message);
                return false;
            }
            return true;
        }

        public void ApplyArgument<T1, T2>(Dictionary<string, string> argument)
        {
            Queue<string> keys = new Queue<string>(argument.Keys);
            while (keys.Count > 0)
            {
                string key = keys.Dequeue();
                foreach (PropertyInfo p in typeof(T1).GetProperties())
                {
                    Attribute attr = p.GetCustomAttribute(typeof(T2));
                    if (attr != null)
                    {
                        if (((ICommandLineAttribute)attr).Name == key)
                        {
                            p.SetValue(Wallet, argument[key]);
                            data.Add(key, argument[key]);
                            argument.Remove(key);
                            break;
                        }
                    }
                }
            }
        }

        public void ShowHelpMessage()
        {
            AssemblyName assembly = Assembly.GetEntryAssembly().GetName();

            // VERSION
            string message = string.Empty
                + "\n"
                + (assembly.Name + "".PadLeft(2) + assembly.Version.ToString())
                ;

            // USAGE
            message += string.Empty
                + "\n"
                + "\n" + "".PadLeft(0) + "USAGE : "
                + "\n" + "".PadLeft(10) + string.Format("Mineral.dll {0} <dir> {1} <password> [options]", OptionName.KeyStoreDir, OptionName.KeyStorePassword)
                + "\n" + "".PadLeft(10) + string.Format("Mineral.dll {0} <key> [options]", OptionName.PrivateKey)
                ;

            // DEFAULT OPTIONS
            message += string.Empty
                + "\n"
                + "\n" + "".PadLeft(1) + "--DEFAULT OPTIONS : ";
            foreach (PropertyInfo info in typeof(OptionDefault).GetProperties())
            {
                DefaultAttribute attr = (DefaultAttribute)info.GetCustomAttribute(typeof(DefaultAttribute));
                if (attr != null)
                {
                    message += "\n" + "".PadLeft(4);
                    message += string.Format("{0,-25} {1}", attr.Name, attr.Description);
                }
            }

            // WALLET OPTIONS
            message += string.Empty
                + "\n"
                + "\n" + "".PadLeft(1) + "--WALLET OPTIONS : ";
            foreach (PropertyInfo info in typeof(OptionWallet).GetProperties())
            {
                WalletAttribute attr = (WalletAttribute)info.GetCustomAttribute(typeof(WalletAttribute));
                if (attr != null)
                {
                    message += "\n" + "".PadLeft(4);
                    message += string.Format("{0,-25} {1}", attr.Name, attr.Description);
                }
            }

            message += string.Empty
                + "\n"
                + "\n" + "".PadLeft(0) + "MISC OPTION :"
                + "\n" + "".PadLeft(4) + "-h   -help"
                ;

            Console.WriteLine(message);
        }
    }
}