using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Mineral.Wallets;
using Mineral.Wallets.KeyStore;

namespace MineralCLI
{
    class Program
    {
        internal static int id = new Random().Next(0, int.MaxValue);
        internal static string url = null;

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
            Console.WriteLine("IP : " + Config.Instance.Network.ListenAddress);
            Console.WriteLine("PORT : " + Config.Instance.Network.RpcPort);
            url = @"http:\\" + Config.Instance.Network.ListenAddress + ":" + Config.Instance.Network.RpcPort;
        }
    }
}
