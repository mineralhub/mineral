using Mineral.CommandLine.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Net.RpcHandler
{
    public static class RpcCommandType
    {
        public static readonly string CreateTransaction = "createtransaction";


        [CommandLineAttribute(Name = "GetAccount", Description = "")]
        public static readonly string GetAccount = "getaccount";

        [CommandLineAttribute(Name = "GetBlock", Description = "")]
        public static readonly string GetBlock = "getblock";

        [CommandLineAttribute(Name = "SendCoin", Description = "")]
        public static readonly string SendCoin = "sendcoin";
    }
}
