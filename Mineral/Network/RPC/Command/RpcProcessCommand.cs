using Newtonsoft.Json.Linq;
using Mineral.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Transactions;
using Mineral.Utils;

namespace Mineral.Network.RPC.Command
{
    public partial class RpcProcessCommand
    {
        public static JObject ProcessTransaction(LocalNode node, byte[] transaction)
        {
            JObject json = new JObject();

            Transaction tx = Transaction.DeserializeFrom(transaction);
            if (tx != null)
            {
                if (tx.Verify() && tx.VerifyBlockChain())
                {
                    node.AddTransaction(tx);
                    json["transaction"] = tx.ToJson();
                }
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

        public static JObject OnCadidateDelegates(object obj, JArray parameters)
        {
            JObject json = new JObject();
            json["cadidates"] = new JArray();
            List<DelegateState> list = BlockChain.Instance.GetDelegateStateAll();
            foreach (DelegateState state in list)
            {
                JObject jstate = new JObject();
                jstate["address"] = state.AddressHash.ToString();
                jstate["name"] = Encoding.UTF8.GetString(state.Name); ;
                (json["cadidates"] as JArray).Add(jstate);
            }
            return json;
        }

        public static JObject OnGetTurnTable(object obj, JArray parameters)
        {
            JObject json = new JObject();
            json["TurnTable"] = new JArray();
            TurnTableState table = BlockChain.Instance.GetTurnTable(parameters[0].Value<uint>());
            foreach (UInt160 hash in table.addrs)
            {
                DelegateState state = BlockChain.Instance.GetDelegateState(hash);
                JObject jstate = new JObject();
                jstate["address"] = state.AddressHash.ToString();
                jstate["name"] = Encoding.UTF8.GetString(state.Name); ;
                (json["TurnTable"] as JArray).Add(jstate);
            }
            return json;
        }
    }
}