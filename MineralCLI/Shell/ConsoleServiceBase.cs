using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MineralCLI.Shell
{
    public class ConsoleServiceBase : IDisposable
    {
        #region Field
        private const string _prompt = ">";
        public bool IsRunning { get; set; } = true;
        #endregion


        #region Property
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public virtual void OnHelp(string[] parameters) { }

        public virtual void Run(string[] args)
        {
            while (IsRunning)
            {
                Console.Write(_prompt);
                string input = Console.ReadLine();
                string[] parameters = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (parameters.Length > 0)
                {
                    IsRunning = OnCommand(parameters);
                }
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
                    }
                    break;
                case "help":
                    {
                        OnHelp(parameters);
                        result = true;
                    }
                    break;
                case "version":
                    {
                        string msg = Assembly.GetEntryAssembly().GetName().Version.ToString();
                        Console.WriteLine(msg);
                    }
                    break;
                case "exit":
                    {
                        result = false;
                    }
                    break;
                default:
                    {
                        Console.WriteLine("Unknown command.");
                        result = true;
                    }
                    break;

            }
            return result;
        }

        public void Dispose()
        {
            IsRunning = false;
        }
        #endregion






    }
}
