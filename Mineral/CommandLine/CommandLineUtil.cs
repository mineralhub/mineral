using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.CommandLine
{
    public static class CommandLineUtil
    {
        public static string ReadPasswordString(string message)
        {
            string result = "";

            Console.Write(message);
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
    }
}
