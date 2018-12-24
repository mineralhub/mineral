using Newtonsoft.Json.Linq;

namespace Mineral.Network.RPC.Command
{
    public partial class RpcProcessCommand
    {
        public static JObject OnNodeList(object obj, JArray parameters)
        {
            JObject json = new JObject();
            JArray nodes = new JArray();

            //LocalNode node = obj as LocalNode;
            foreach (RemoteNode remote in NetworkManager.Instance.CloneConnectedPeers())
                nodes.Add(string.Format("{0}:{1}", remote.RemoteEndPoint.Address, remote.RemoteEndPoint.Port));

            json["nodes"] = nodes;
            return json;
        }
    }
}
