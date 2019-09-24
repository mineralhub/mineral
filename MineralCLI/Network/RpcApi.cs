using Google.Protobuf;
using Mineral;
using Mineral.CommandLine;
using Mineral.Common.Utils;
using Mineral.Core;
using Mineral.Core.Capsule.Util;
using Mineral.Core.Exception;
using Mineral.Cryptography;
using Mineral.Utils;
using Mineral.Wallets.KeyStore;
using MineralCLI.Network;
using MineralCLI.Shell;
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
        public static readonly string FILE_PATH = "Wallet";
        public static readonly string FILE_EXTENTION = ".keystore";

        public static KeyStore KeyStore = null;
        #endregion


        #region Property
        public static bool IsLogin
        {
            get
            {
                bool result = RpcApi.KeyStore != null;
                if (result == false)
                {
                    Console.WriteLine("Please login first.");
                }

                return result;
            }
        }
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        protected static void OutputTransactionErrorMessage(int code, string message)
        {
            Console.WriteLine("Transaction Result :");
            Console.WriteLine("Code : " + code);
            Console.WriteLine("Message : " + message);
        }
        #endregion


        #region External Method
        public static JObject MakeCommand(string method, JArray parameters)
        {
            JObject cmd = new JObject();
            cmd["id"] = Program.id;
            cmd["method"] = method;
            cmd["params"] = parameters;
            return cmd;
        }

        public static JObject SendCommand(string method, JArray parameters)
        {
            JObject obj = MakeCommand(method, parameters);
            return RpcClient.RequestPostAnsyc(Program.url, obj.ToString()).Result;
        }
        #endregion
    }
}
