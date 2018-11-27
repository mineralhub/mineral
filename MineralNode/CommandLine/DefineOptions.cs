using MineralNode.CommandLine.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MineralNode.CommandLine
{
    public class OptionName
    {
        // Default
        public const string ConfigDir = "--configidr";

        // Wallet
        public const string KeyStoreDir = "--keystoredir";
        public const string KeyStorePassword = "--keystorepassword";
        public const string PrivateKey = "--privatekey";

        // Misc
        public const string H = "-h";
        public const string Help = "-help";
    }

    public class OptionDefault
    {
        [DefaultAttribute(Name = OptionName.ConfigDir, Description = "Directory for the config file(config.json)")]
        public string ConfigDir { get; set; }
    }

    public class OptionWallet
    {
        [WalletAttribute(Name = OptionName.KeyStoreDir, Description = "Directory for the keystroe file(.keystore)")]
        public string KeyStoreDir { get; set; }

        [WalletAttribute(Name = OptionName.KeyStorePassword, Description = "Keystore password")]
        public string KeyStorePassword { get; set; }

        [WalletAttribute(Name = OptionName.PrivateKey, Description = "Wallet private key")]
        public string PrivateKey { get; set; }
    }
}
