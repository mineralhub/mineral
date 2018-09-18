using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using Sky;
using Sky.Network.RPC.Command;
using SkyCLI.Network;

namespace SkyCLI.Commands
{
    public class BlockCommand : BaseCommand
    {
        public static bool OnGetBlock(string[] parameters)
        {
            if (parameters.Length != 2)
            {
                ErrorParamMessage();
                return true;
            }

            JArray param = new JArray(new ArraySegment<string>(parameters, 1, parameters.Length - 1));

            JObject obj = MakeCommand(Config.BlockVersion, RpcCommands.Block.GetBlock, param);
            obj = RcpClient.RequestPostAnsyc(Program.url, obj.ToString()).Result;

            TestOutput(obj);
            return true;
        }

        public static bool OnGetBlockHash(string[] parameters)
        {
            if (parameters.Length != 2)
            {
                ErrorParamMessage();
                return true;
            }

            JArray param = new JArray(new ArraySegment<string>(parameters, 1, parameters.Length - 1));

            JObject obj = MakeCommand(Config.BlockVersion, RpcCommands.Block.GetBlockHash, param);
            obj = RcpClient.RequestPostAnsyc(Program.url, obj.ToString()).Result;

            TestOutput(obj);
            return true;
        }

        public static bool OnGetHeight(string[] parameters)
        {
            JObject obj = MakeCommand(Config.BlockVersion, RpcCommands.Block.GetHeight, new JArray());
            obj = RcpClient.RequestPostAnsyc(Program.url, obj.ToString()).Result;

            TestOutput(obj);
            return true;
        }

        public static bool OnGetCurrentBlockHash(string[] parameters)
        {
            JObject obj = MakeCommand(Config.BlockVersion, RpcCommands.Block.GetCurrentBlockHash, new JArray());
            obj = RcpClient.RequestPostAnsyc(Program.url, obj.ToString()).Result;

            TestOutput(obj);
            return true;
        }

        public static bool OnGetTransaction(string[] parameters)
        {
            JObject obj = MakeCommand(Config.BlockVersion, RpcCommands.Block.GetCurrentBlockHash, new JArray());
            obj = RcpClient.RequestPostAnsyc(Program.url, obj.ToString()).Result;

            TestOutput(obj);
            return true;
        }
    }
}
