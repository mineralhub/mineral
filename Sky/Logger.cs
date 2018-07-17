using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sky
{
    public static class Logger
    {
        static private object _writeLock = new object();
        static public bool WriteConsole = false;

        static public void Log(string log)
        {
            lock(_writeLock)
            {
                if (WriteConsole)
                    Console.WriteLine(log);

                File.AppendAllText("./output-log", log + "\n");
            }
        }
    }
}
