using System;
using System.Collections.Generic;
using System.Linq;
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
            public const string usage = "Usage :\n\n";
            public const string Options = "Options :\n\n";
            public const string Command_Options = "Command Options :\n\n";
            public const string Help = "Help :\n\n";
        }

        public struct HelpCommandOption
        {
            public const string Help = "-help -h\n";
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

        public static void OutputHelpMessage(string[] usage_message, string[] option_message, string[] commandoption_message, string[] help_message)
        {
            string output_message = Program.version;

            if (usage_message != null)
            {
                output_message += "\n" + "".PadLeft(2) + HelpCategory.usage;
                foreach (string msg in usage_message ?? Enumerable.Empty<string>())
                    output_message += "".PadLeft(4) + msg + "\n";
            }

            if (option_message != null)
            {
                output_message += "\n" + "".PadLeft(2) + HelpCategory.Options;
                foreach (string msg in option_message ?? Enumerable.Empty<string>())
                    output_message += "".PadLeft(4) + msg + "\n";
            }

            if (commandoption_message != null)
            {
                output_message += "\n" + "".PadLeft(2) + HelpCategory.Command_Options;
                foreach (string msg in commandoption_message ?? Enumerable.Empty<string>())
                    output_message += "".PadLeft(4) + msg + "\n";
            }

            if (help_message != null)
            {
                output_message += "\n" + "".PadLeft(2) + HelpCategory.Help;
                foreach (string msg in help_message ?? Enumerable.Empty<string>())
                    output_message += "".PadLeft(4) + msg + "\n";
            }

            Console.WriteLine(output_message);
        }
    }
}
