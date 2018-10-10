using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Sky.Network.RPC.Command
{
    public partial class RpcProcessCommand
    {
        public static JObject OnNodeList(object obj, RpcCommand.ParamType type, JArray parameters)
        {
            JObject json = new JObject();
            JArray nodes = new JArray();

            LocalNode node = obj as LocalNode;
            foreach (RemoteNode remote in node.CloneConnectedPeers())
                nodes.Add(string.Format("{0}:{1}", remote.RemoteEndPoint.Address, remote.RemoteEndPoint.Port));

            json["nodes"] = nodes;
            return json;
        }
    }
}
