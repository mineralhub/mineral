using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using Sky;
using Sky.Network.RPC.Command;
using SkyCLI.Network;

namespace SkyCLI.Commands
{
    public class BaseCommand
    {
        public delegate bool CommandHandler(string[] parameters);

        public struct HelpCategory
        {
            public const string Usage = "\nUsage :\n";
            public const string Options = "\nOptions :\n";
            public const string Command_Options = "\nCommand Options :\n";
            public const string Help = "\nHelp :\n";
            public const string Option_Help = "\t -help -h\n";
        }

        public static bool AppendParameter(ref JObject cmd, JToken parameter)
        {
            if (!cmd.ContainsKey("params"))
            {
                cmd["params"] = new JArray(parameter);
            }
            else if (cmd["params"] is JArray)
            {
                JArray parameters = cmd["params"] as JArray;
                parameters.Add(parameter);
            }
            else
            {
                return false;
            }
            return true;
        }

        public static JObject MakeCommand(double id, string method, JArray parameters)
        {
            JObject cmd = new JObject();
            cmd["id"] = id;
            cmd["method"] = method;
            cmd["params"] = parameters;
            return cmd;
        }

        public static JObject SendCommand(double id, string method, JArray parameters)
        {
            JObject obj = MakeCommand(Config.BlockVersion, RpcCommand.Block.GetBlockHash, parameters);
            return RcpClient.RequestPostAnsyc(Program.url, obj.ToString()).Result;
        }

        public static void OutputHelpMessage(string usage_message, string option_message, string commandoption_message, string help_message)
        {
            string message =
                Program.version +
                (usage_message.Length > 0 ? HelpCategory.Usage + usage_message : "") +
                (option_message.Length > 0 ? HelpCategory.Options + option_message : "") +
                (commandoption_message.Length > 0 ? HelpCategory.Command_Options + commandoption_message : "") +
                (help_message.Length > 0 ? HelpCategory.Help + help_message : "");

            Console.WriteLine(message + "\n");
        }
    }
}
