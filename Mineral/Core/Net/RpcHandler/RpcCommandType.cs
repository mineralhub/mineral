using Mineral.CommandLine.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Net.RpcHandler
{
    public static class RpcCommand
    {
        public static class Node
        {
            [CommandLineAttribute(Name = "ListNode", Description = "")]
            public static readonly string ListNode = "listnode";
        }

        public static class Wallet
        {
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

            [CommandLineAttribute(Name = "ListWitness", Description = "")]
            public static readonly string ListWitness = "listwitness";
        }

        public static class AssetIssue
        {
            [CommandLineAttribute(Name = "AssetIssue", Description = "")]
            public static readonly string CreateAssetIssue = "createassetissue";

            [CommandLineAttribute(Name = "UpdateAsset", Description = "")]
            public static readonly string UpdateAsset = "updateasset";

            [CommandLineAttribute(Name = "AssetIssueByAccount", Description = "")]
            public static readonly string AssetIssueByAccount = "assetissuebyaccount";

            [CommandLineAttribute(Name = "AssetIssueById", Description = "")]
            public static readonly string AssetIssueById = "assetissuebyid";

            [CommandLineAttribute(Name = "AssetIssueByName", Description = "")]
            public static readonly string AssetIssueByName = "assetissuebyname";

            [CommandLineAttribute(Name = "AssetIssueListByName", Description = "")]
            public static readonly string AssetIssueListByName = "assetissuelistbyname";

            [CommandLineAttribute(Name = "ListAssetIssue", Description = "")]
            public static readonly string ListAssetIssue = "listassetissue";

            [CommandLineAttribute(Name = "ListExchange", Description = "")]
            public static readonly string ListExchange = "listexchange";

            [CommandLineAttribute(Name = "ListExchangePaginated", Description = "")]
            public static readonly string ListExchangePaginated = "listexchangepaginated";

            [CommandLineAttribute(Name = "TransferAsset", Description = "")]
            public static readonly string TransferAsset = "transferasset";

            [CommandLineAttribute(Name = "UnFreezeAsset", Description = "")]
            public static readonly string UnfreezeAsset = "unfreezeasset";
        }

        public static class Block
        {
            [CommandLineAttribute(Name = "GetBlock", Description = "")]
            public static readonly string GetBlock = "getblock";

            [CommandLineAttribute(Name = "GetBlockByLatestNum", Description = "")]
            public static readonly string GetBlockByLatestNum = "getblockbylatestnum";

            [CommandLineAttribute(Name = "GetBlockById", Description = "")]
            public static readonly string GetBlockById = "getblockbyid";

            [CommandLineAttribute(Name = "GetBlockByLimitNext", Description = "")]
            public static readonly string GetBlockByLimitNext = "getblockbylimitnext";
        }

        public static class Transaction
        {
            [CommandLineAttribute(Name = "GetTotalTransaction", Description = "")]
            public static readonly string GetTotalTransaction = "gettotaltransaction";

            [CommandLineAttribute(Name = "GetTransactionApprovedList", Description = "")]
            public static readonly string GetTransactionApprovedList = "gettransactionapprovedlist";

            [CommandLineAttribute(Name = "GetTransactionById", Description = "")]
            public static readonly string GetTransactionById = "gettransactionbyid";

            [CommandLineAttribute(Name = "GetTransactionInfoById", Description = "")]
            public static readonly string GetTransactionInfoById = "gettransactioninfobyid";

            [CommandLineAttribute(Name = "GetTransactionCountByBlockNum", Description = "")]
            public static readonly string GetTransactionCountByBlockNum = "gettransactioncountbyblocknum";

            [CommandLineAttribute(Name = "GetTransactionsFromThis", Description = "")]
            public static readonly string GetTransactionsFromThis = "gettransactionsfromthis";

            [CommandLineAttribute(Name = "GetTransactionsToThis", Description = "")]
            public static readonly string GetTransactionsToThis = "gettransactionstothis";

            [CommandLineAttribute(Name = "GetTransactionSignWeight", Description = "")]
            public static readonly string GetTransactionSignWeight = "gettransactionsignweight";

            [CommandLineAttribute(Name = "CreateAccount", Description = "")]
            public static readonly string CreateAccount = "createaccount";

            [CommandLineAttribute(Name = "CreateProposal", Description = "")]
            public static readonly string CreateProposal = "createproposal";

            [CommandLineAttribute(Name = "CreateWitness", Description = "")]
            public static readonly string CreateWitness = "createwitness";

            [CommandLineAttribute(Name = "CreateTransaction", Description = "")]
            public static readonly string CreateTransaction = "createtransaction";

            [CommandLineAttribute(Name = "ApproveProposal", Description = "")]
            public static readonly string ApproveProposal = "approveproposal";

            [CommandLineAttribute(Name = "UpdateAccount", Description = "")]
            public static readonly string UpdateAccount = "updateaccount";

            [CommandLineAttribute(Name = "UpdateWitness", Description = "")]
            public static readonly string UpdateWitness = "updatewitness";

            [CommandLineAttribute(Name = "UpdateEnergyLimit", Description = "")]
            public static readonly string UpdateEnergyLimit = "updateenergylimit";

            [CommandLineAttribute(Name = "UpdateAccountPermission", Description = "")]
            public static readonly string UpdateAccountPermission = "updateaccountpermission";

            [CommandLineAttribute(Name = "UpdateSetting", Description = "")]
            public static readonly string UpdateSetting = "updatesetting";

            [CommandLineAttribute(Name = "DeleteProposal", Description = "")]
            public static readonly string DeleteProposal = "deleteproposal";

            [CommandLineAttribute(Name = "BroadcastTransaction", Description = "")]
            public static readonly string BroadcastTransaction = "broadcasttransaction";

            [CommandLineAttribute(Name = "FreezeBalance", Description = "")]
            public static readonly string FreezeBalance = "freezebalance";

            [CommandLineAttribute(Name = "UnFreezeBalance", Description = "")]
            public static readonly string UnfreezeBalance = "unfreezebalance";

            [CommandLineAttribute(Name = "VoteWitness", Description = "")]
            public static readonly string VoteWitness = "votewitness";

            [CommandLineAttribute(Name = "WithdrawBalance", Description = "")]
            public static readonly string WithdrawBalance = "withdrawbalance";

            [CommandLineAttribute(Name = "SendCoin", Description = "")]
            public static readonly string SendCoin = "sendcoin";

            [CommandLineAttribute(Name = "ListProposal", Description = "")]
            public static readonly string ListProposal = "listproposal";

            [CommandLineAttribute(Name = "ListProposalPaginated", Description = "")]
            public static readonly string ListProposalPaginated = "listproposalpaginated";

            [CommandLineAttribute(Name = "GetParameters", Description = "")]
            public static readonly string GetParameters = "getparameters";
        }
    }
}
