using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Sky.Wallets;

namespace SkyCLI
{
    class Program
    {
        internal static string url = null;
        internal static WalletAccount Wallet = null;

        static void Main(string[] args)
        {
            Initialize(args);
            Shell.ConsoleService service = new Shell.ConsoleService();
            service.Run(args);
        }

        private static void Initialize(string[] args)
        {
            Config.Instance.Initialize();
            Console.WriteLine(Config.Instance.GetVersion());
            url = @"http:\\" + Config.Instance.Network.ListenAddress + ":" + Config.Instance.Network.RpcPort;
        }
    }
}
