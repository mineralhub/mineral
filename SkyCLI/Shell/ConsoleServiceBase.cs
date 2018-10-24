using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SkyCLI.Shell
{
    public class ConsoleServiceBase : IDisposable
    {
        public const string prompt = ">";
        public bool IsRunning { get; set; } = true;

        public virtual void Run(string[] args)
        {
            while (IsRunning)
            {
                Console.Write(prompt);
                string input = Console.ReadLine();
                string[] parameters = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (parameters.Length > 0)
                    IsRunning = OnCommand(parameters);
            }
        }

        public virtual bool OnCommand(string[] parameters)
        {
            bool result = true;

            switch (parameters[0].ToLower())
            {
                case "cls":
                case "clear":
                    {
                        Console.Clear();
                        result = true;
                    } break;
                case "help":
                    {
                        OnHelp(parameters);
                        result = true;
                    } break;
                case "version":
                    {
                        string msg = Assembly.GetEntryAssembly().GetName().Version.ToString();
                        Console.WriteLine(msg);
                    } break;
                case "exit":
                    {
                        result = false;
                    } break;
                default:
                    {
                        Console.WriteLine("Unknown command.");
                        result = true;
                    } break;

            }
            return result;
        }

        public virtual void OnHelp(string[] parameters) { }

        public static string ReadPasswordString()
        {
            string result = "";

            while (true)
            {
                var key = System.Console.ReadKey(true);

                if (key.Key == ConsoleKey.Backspace) continue;
                else if (key.Key == ConsoleKey.Enter) break;

                Console.Write("*");
                result += key.KeyChar;
            }
            Console.WriteLine();

            return result;
        }

        public void Dispose()
        {
            IsRunning = false;
        }
    }
}
