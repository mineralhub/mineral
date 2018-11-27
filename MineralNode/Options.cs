using Mineral;
using Mineral.CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace MineralNode
{
    public class Options
    {
        public class OptionName
        {
            public const string ConfigDir = "--configidr";
            public const string KeyStoreDir = "--keystoredir";
            public const string KeyStorePassword = "--keystorepassword";
            public const string PrivateKey = "--privatekey";
        }

        #region Option Properties
        [CommandAttribute(Name = OptionName.ConfigDir, Description = "Directory for the config file(config.json)")]
        public string ConfigDir { get; set; }

        [CommandAttribute(Name = OptionName.KeyStoreDir, Description = "Directory for the keystroe file(.keystore)")]
        public string KeyStoreDir { get; set; }

        [CommandAttribute(Name = OptionName.KeyStorePassword, Description = "keystore password")]
        public string KeyStorePassword { get; set; }

        [CommandAttribute(Name = OptionName.PrivateKey, Description = "Wallet private key")]
        public string PrivateKey { get; set; }
        #endregion

        public readonly string[] arg_indicator = new string[] { "-", "--" };
        private ParseResult data = null;

        public Options(string[] args)
        {
            data = ArgumentsParser.ApplyArgument<Options>(ArgumentsParser.ParseArguments(args, new string[] { "-", "--" }), this);
        }

        public bool IsValid()
        {
            if (data == null)
            {
                Logger.Log("option data not loaded.");
                return false;
            }

            if (data.ErrorResults.Count > 0)
            {
                string message = "Invalid option : " + data.ErrorResults[0];
                Logger.Log(message);
                return false;
            }

            if (data.Results.ContainsKey(OptionName.KeyStoreDir) && data.Results.ContainsKey(OptionName.PrivateKey))
            {
                string message = OptionName.KeyStoreDir + "and " + OptionName.PrivateKey + "can't used together.";
                Logger.Log(message);
                return false;
            }
            return true;
        }
    }
}
