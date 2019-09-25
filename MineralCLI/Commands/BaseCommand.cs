using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace MineralCLI.Commands
{
    public class BaseCommand
    {
        public delegate bool CommandHandler(string command, string[] parameters);

        public struct HelpCategory
        {
            public const string usage = "Usage :\n";
            public const string Options = "Options :\n";
            public const string Command_Options = "Command Options :\n";
            public const string Help = "Help :\n";
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

        public static void OutputResultMessage(string method, bool result, int code, string message)
        {
            if (result)
            {
                Console.WriteLine(
                    string.Format("{0} Success.", method));
            }
            else
            {
                Console.WriteLine(string.Format("{0} Failed.", method));
                Console.WriteLine("Error code : " + code);
                Console.WriteLine("Message \n" + message);
            }
        }

        public static void OutputHelpMessage(string[] usage_message, string[] option_message, string[] commandoption_message, string[] help_message)
        {
            string output_message = Config.Instance.GetVersion() + "\n";

            if (usage_message != null)
            {
                output_message += "\n" + "".PadLeft(1) + HelpCategory.usage;
                foreach (string msg in usage_message ?? Enumerable.Empty<string>())
                    output_message += "".PadLeft(10) + msg;
            }

            if (option_message != null)
            {
                output_message += "\n" + "".PadLeft(1) + HelpCategory.Options;
                foreach (string msg in option_message ?? Enumerable.Empty<string>())
                    output_message += "".PadLeft(4) + msg;
            }

            if (commandoption_message != null)
            {
                output_message += "\n" + "".PadLeft(1) + HelpCategory.Command_Options;
                foreach (string msg in commandoption_message ?? Enumerable.Empty<string>())
                    output_message += "".PadLeft(4) + msg;
            }

            if (help_message != null)
            {
                output_message += "\n" + "".PadLeft(1) + HelpCategory.Help;
                foreach (string msg in help_message ?? Enumerable.Empty<string>())
                    output_message += "".PadLeft(4) + msg;
            }

            Console.WriteLine(output_message);
        }
    }
}
