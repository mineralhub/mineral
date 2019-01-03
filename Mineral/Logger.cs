using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mineral
{
    public static class Logger
    {
        static public bool WriteConsole = false;
        static private ConcurrentQueue<string> _queue = new ConcurrentQueue<string>();

        static Logger()
        {
            Task.Run(() => Process());
        }

        static public void Log(string log)
        {
            _queue.Enqueue(log);
        }

        static void Process()
        {
            while (true)
            {
                if (_queue.TryDequeue(out string log))
                {
                    if (WriteConsole)
                        Console.WriteLine(log);
                    File.AppendAllText("./output-log", log + "\n");
                }
                else
                {
                    Thread.Sleep(50);
                }
            }

        }
    }
}
