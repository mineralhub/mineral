using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using Sky;
using Sky.Network.RPC.Command;
using SkyCLI.Network;

namespace SkyCLI.Commands
{
    public class NodeCommand : BaseCommand
    {
        public static bool OnNodeList(string[] parameters)
        {
            JObject obj = MakeCommand(Config.BlockVersion, RpcCommands.Node.NodeList, new JArray());
            obj = RcpClient.RequestPostAnsyc(Program.url, obj.ToString()).Result;

            TestOutput(obj);

            return true;
        }
    }
}
