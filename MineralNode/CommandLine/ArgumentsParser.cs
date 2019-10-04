using System;
using System.Collections.Generic;
using System.Text;

namespace MineralNode.CommandLine
{
    using ParseArgumentResult = Dictionary<string, string>;

    public class ArgumentsParser
    {
        public static string FindArgument(string arg, string[] arg_indicators)
        {
            foreach (string indicator in arg_indicators)
            {
                if (arg.IndexOf(indicator) == 0)
                    return arg;
            }
            return "";
        }

        public static ParseArgumentResult ParseArguments(string[] args, string[] arg_indicators)
        {
            ParseArgumentResult result = new ParseArgumentResult();

            string key;
            for (int i = 0; i < args.Length; i++)
            {
                if (string.IsNullOrEmpty(args[i])) continue;

                if (!string.IsNullOrEmpty(key = FindArgument(args[i], arg_indicators)))
                {
                    if (i + 1 < args.Length)
                    {
                        if (!string.IsNullOrEmpty(FindArgument(args[i + 1], arg_indicators)))
                        {
                            result.Add(key, "");
                            continue;
                        }
                        result.Add(key, args[++i]);
                    }
                    else
                    {
                        result.Add(key, "");
                    }
                }
            }
            return result;
        }
    }
}
