using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace SkyCLI.Commands
{
    public class BaseCommand
    {
        public delegate bool CommandHandler(string[] parameters);

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

        public static void TestOutput(JObject obj)
        {
            Console.WriteLine(obj);
        }

        public static void ErrorParamMessage()
        {
            Console.WriteLine("Error parameter.");
        }
    }
}
