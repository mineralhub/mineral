using Newtonsoft.Json.Linq;
using Sky.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sky.Network.RPC.Command
{
    public partial class RpcProcessCommand
    {
        public static JObject ProcessTransaction(LocalNode node, byte[] transaction)
        {
            JObject json = new JObject();

            Transaction tx = Transaction.DeserializeFrom(transaction);
            if (tx != null)
            {
                if (tx.Verify() && tx.VerifyBlockchain())
                    node.AddTransaction(tx);
                else
                {
                    json = RpcCommand.CreateErrorResult(null, (int)tx.TxResult, tx.TxResult.ToString());
                }
            }
            else
            {
                json = RpcCommand.CreateErrorResult(null, 0, "Invalid trasaction data");
            }

            return json;
        }
    }
}