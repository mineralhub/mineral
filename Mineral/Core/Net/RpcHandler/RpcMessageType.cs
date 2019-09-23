using Mineral.CommandLine.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Net.RpcHandler
{
    public static class RpcCommandType
    {
        [CommandLineAttribute(Name = "CreateAccount", Description = "")]
        public static readonly string CreateAccount = "createaccount";

        [CommandLineAttribute(Name = "CreateProposal", Description = "")]
        public static readonly string CreateProposal = "createproposal";

        [CommandLineAttribute(Name = "CreateWitness", Description = "")]
        public static readonly string CreateWitness = "createwitness";

        [CommandLineAttribute(Name = "CreateTransaction", Description = "")]
        public static readonly string CreateTransaction = "createtransaction";

        [CommandLineAttribute(Name = "GetTransactionSignWeight", Description = "")]
        public static readonly string GetTransactionSignWeight = "gettransactionsignweight";

        [CommandLineAttribute(Name = "BroadcastTransaction", Description = "")]
        public static readonly string BroadcastTransaction = "broadcasttransaction";

        [CommandLineAttribute(Name = "ImportWallet", Description = "")]
        public static readonly string ImportWallet = "importwallet";

        [CommandLineAttribute(Name = "BackupWallet", Description = "")]
        public static readonly string BackupWallet = "backupwallet";

        [CommandLineAttribute(Name = "RegisterWallet", Description = "")]
        public static readonly string RegisterWallet = "registerwallet";

        [CommandLineAttribute(Name = "Login", Description = "")]
        public static readonly string Login = "login";

        [CommandLineAttribute(Name = "Logout", Description = "")]
        public static readonly string Logout = "logout";

        [CommandLineAttribute(Name = "GetAddress", Description = "")]
        public static readonly string GetAddress = "getaddress";

        [CommandLineAttribute(Name = "GetBalance", Description = "")]
        public static readonly string GetBalance = "getbalance";

        [CommandLineAttribute(Name = "GetAccount", Description = "")]
        public static readonly string GetAccount = "getaccount";

        [CommandLineAttribute(Name = "GetBlock", Description = "")]
        public static readonly string GetBlock = "getblock";

        [CommandLineAttribute(Name = "GetBlockByLatestNum", Description = "")]
        public static readonly string GetBlockByLatestNum = "getblockbylatestnum";

        [CommandLineAttribute(Name = "GetBlockById", Description = "")]
        public static readonly string GetBlockById = "getblockbyid";

        [CommandLineAttribute(Name = "GetBlockByLimitNext", Description = "")]
        public static readonly string GetBlockByLimitNext = "getblockbylimitnext";

        [CommandLineAttribute(Name = "SendCoin", Description = "")]
        public static readonly string SendCoin = "sendcoin";

        [CommandLineAttribute(Name = "AssetIssueByAccount", Description = "")]
        public static readonly string AssetIssueByAccount = "assetissuebyaccount";

        [CommandLineAttribute(Name = "AssetIssueById", Description = "")]
        public static readonly string AssetIssueById = "assetissuebyid";

        [CommandLineAttribute(Name = "AssetIssueByName", Description = "")]
        public static readonly string AssetIssueByName = "assetissuebyname";

        [CommandLineAttribute(Name = "AssetIssueListByName", Description = "")]
        public static readonly string AssetIssueListByName = "assetissuelistbyname";

        [CommandLineAttribute(Name = "TransferAsset", Description = "")]
        public static readonly string TransferAsset = "transferasset";

    }
}
