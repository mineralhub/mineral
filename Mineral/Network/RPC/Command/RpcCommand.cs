using Mineral.CommandLine.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Network.RPC.Command
{
    public partial class RpcCommand
    {
        public class General
        {
            [CommandLineAttribute(Name = GetConfig, Description = "Returns information about the connected server")]
            public const string GetConfig = "getconfig";
        }

        public struct Block
        {
            [CommandLineAttribute(Name = GetBlock, Description = "Returns information about the block")]
            public const string GetBlock = "getblock";
            [CommandLineAttribute(Name = GetBlocks, Description = "Returns information about the blocks")]
            public const string GetBlocks = "getblocks";
            [CommandLineAttribute(Name = GetBlockHash, Description = "Returns hash of block")]
            public const string GetBlockHash = "getblockhash";
            [CommandLineAttribute(Name = GetHeight, Description = "Returns current block height")]
            public const string GetHeight = "getheight";
            [CommandLineAttribute(Name = GetCurrentBlockHash, Description = "Returns current block hash value")]
            public const string GetCurrentBlockHash = "getcurrentblockhash";
            [CommandLineAttribute(Name = GetTransaction, Description = "Returns information about the transaction")]
            public const string GetTransaction = "gettransaction";
            [CommandLineAttribute(Name = AddTransaction, Description = "Add transaction")]
            public const string AddTransaction = "addtransaction";

            [CommandLineAttribute(Name = GetCadidateDelegates, Description = "Returns Information about the delegates")]
            public const string GetCadidateDelegates = "getcandidatedelegates";
            [CommandLineAttribute(Name = GetTurnTable, Description = "Returns information about the turn table")]
            public const string GetTurnTable = "getturntable";
        }

        public struct Node
        {
            [CommandLineAttribute(Name = NodeList, Description = "Returns connected about the nodes")]
            public const string NodeList = "nodelist";
        }

        public struct Wallet
        {
            [CommandLineAttribute(Name = CreateAccount, Description = "Create a new wallet account")]
            public const string CreateAccount = "createaccount";
            [CommandLineAttribute(Name = OpenAccount, Description = "Open wallet account")]
            public const string OpenAccount = "openaccount";
            [CommandLineAttribute(Name = CloseAccount, Description = "Close wallet account")]
            public const string CloseAccount = "closeaccount";

            [CommandLineAttribute(Name = BackupAccount, Description = "Backup wallet account file")]
            public const string BackupAccount = "backupaccount";

            [CommandLineAttribute(Name = GetAccount, Description = "")]
            public const string GetAccount = "getaccount";
            [CommandLineAttribute(Name = GetAddress, Description = "Returns infomaction about the address and addresshash value")]
            public const string GetAddress = "getaddress";
            [CommandLineAttribute(Name = GetBalance, Description = "Returns the balance in the account")]
            public const string GetBalance = "getbalance";
            [CommandLineAttribute(Name = SendTo, Description = "Send balance to account")]
            public const string SendTo = "sendto";

            [CommandLineAttribute(Name = LockBalance, Description = "Lock balance")]
            public const string LockBalance = "lockbalance";
            [CommandLineAttribute(Name = UnlockBalance, Description = "Unlock balance")]
            public const string UnlockBalance = "unlockbalance";

            [CommandLineAttribute(Name = VoteWitness, Description = "Vote")]
            public const string VoteWitness = "votewitness";
            [CommandLineAttribute(Name = GetVoteWitness, Description = "Returns information about the vote")]
            public const string GetVoteWitness = "getvotewitness";
        }
    }
}
