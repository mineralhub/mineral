using Newtonsoft.Json.Linq;
using Sky.Network.RPC.Command;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyCLI.Commands
{
    public class GeneralCommand : BaseCommand
    {
        public static bool OnGetConfig(string[] parameters)
        {
            int index = 1;

            JArray param = new JArray(new ArraySegment<string>(parameters, index, parameters.Length - index));
            SendCommand(Config.Instance.BlockVersion, RpcCommand.General.GetConfig, param);

            return true;
        }
    }
}
