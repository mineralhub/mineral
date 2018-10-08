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
            string usage = string.Format(
                "\t{0} [command option]\n"
                , RpcCommand.Node.NodeList);
            string command_option = HelpCategory.Option_Help;

            if (parameters.Length > 2)
            {
                OutputHelpMessage(usage, "", command_option, "");
                return true;
            }

            int index = 1;
            if (parameters.Length > index)
            {
                string option = parameters[index];
                if (option.ToLower().Equals("-help") || option.ToLower().Equals("-h"))
                {
                    OutputHelpMessage(usage, "", command_option, "");
                    index++;
                    return true;
                }
            }

            JArray param = new JArray(new ArraySegment<string>(parameters, index, parameters.Length - index));
            SendCommand(Config.BlockVersion, RpcCommand.Node.NodeList, new JArray());

            return true;
        }
    }
}
