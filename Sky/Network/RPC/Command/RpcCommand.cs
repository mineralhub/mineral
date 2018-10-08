using System;
using System.Collections.Generic;
using System.Text;

namespace Sky.Network.RPC.Command
{
    public partial class RpcCommand
    {
        public struct Block
        {
            public const string GetBlock = "getblock";
            public const string GetBlocks = "getblocks";
            public const string GetBlockHash = "getblockhash";
            public const string GetHeight = "getheight";
            public const string GetCurrentBlockHash = "getcurrentblockhash";
            public const string GetTransaction = "gettransaction";
        }

        public struct Node
        {
            public const string NodeList = "nodelist";
        }

        public struct Wallet
        {
            public const string CreateAccount = "createaccount";
            public const string OpenAccount = "openaccount";
            public const string CloseAccount = "closeaccount";

            public const string GetAccount = "getaccount";
            public const string GetAddress = "getaddress";
            public const string GetBalance = "getbalance";
            public const string SendTo = "sendto";

            public const string FreezeBalance = "freezebalance";
            public const string UnfreezeBalance = "unfreezebalance";

            public const string VoteWitness = "votewitness";
        }
    }
}
