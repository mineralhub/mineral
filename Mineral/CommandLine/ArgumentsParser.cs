using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Mineral.CommandLine
{
    using ParseArgumentResult = Dictionary<string, string>;

    public class ParseResult
    {
        private List<string> errorResults = new List<string>();
        private ParseArgumentResult results = new ParseArgumentResult();

        public List<string> ErrorResults { get { return this.errorResults; } }
        public Dictionary<string, string> Results { get { return this.results; } }
    }

    public class ArgumentsParser
    {
        protected static string FindArgument(string arg, string[] arg_indicators)
        {
            foreach (string indicator in arg_indicators)
            {
                if (arg.IndexOf(indicator) > -1)
                    return arg;
            }
            return string.Empty;
        }

        public static ParseArgumentResult ParseArguments(string[] args, string[] arg_indicators)
        {
            ParseArgumentResult result = new ParseArgumentResult();

            string key, target;
            for (int i = 0; i < args.Length;)
            {
                if (string.IsNullOrEmpty(args[i]))
                    throw new ArgumentNullException();

                if (!string.IsNullOrEmpty(key = target = FindArgument(args[i], arg_indicators)))
                {
                    if (result.ContainsKey(target))

                    result.Add(target, string.Empty);

                    if (i == args.Length - 1) break;

                    if (string.IsNullOrEmpty(target = FindArgument(args[i + 1], arg_indicators)))
                    {
                        result[key] = args[++i];
                    }
                }
                i++;
            }
            return result;
        }

        public static ParseResult ApplyArgument<T>(ParseArgumentResult results, T target)
        {
            ParseResult parseResult = new ParseResult();
            Queue<string> keys = new Queue<string>(results.Keys);

            bool isFound = false;
            while (keys.Count > 0)
            {
                string key = keys.Dequeue();

                foreach (var p in typeof(T).GetProperties())
                {
                    Attribute attr = p.GetCustomAttribute(typeof(CommandAttribute));
                    if (attr != null)
                    {
                        if (((CommandAttribute)attr).Name == key)
                        {
                            p.SetValue(target, results[key]);
                            parseResult.Results.Add(key, results[key]);
                            isFound = true;
                            break;
                        }
                    }
                }
                if (!isFound)
                    parseResult.ErrorResults.Add(key);

            }
            return parseResult;
        }
    }
}
