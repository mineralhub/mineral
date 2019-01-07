using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mineral
{
    public enum LogLevel
    {
        ERROR = 0,
        WARNING,
        INFO,
        DEBUG,
        TRACE
    }

    internal class TypedLog
    {
        public DateTime timeStamp;
        public LogLevel logType;
        public string message;
        public override string ToString()
        {
            return String.Format("{0} [{1}] {2}", timeStamp.ToString("s"), logType, message);
        }
    }

    public static class Logger
    {
        static public bool WriteConsole = true;
        static public LogLevel WriteLogLevel = LogLevel.INFO;
        static private ConcurrentQueue<TypedLog> _queue = new ConcurrentQueue<TypedLog>();

        static Logger()
        {
            Task.Run(() =>
            {
                Process();
            });
        }

        static public void Log(string log, LogLevel logLevel = LogLevel.INFO)
        {
            TypedLog logdata = new TypedLog() { timeStamp = DateTime.Now, logType = logLevel, message = log };
            if (logdata.logType <= WriteLogLevel)
            {
                _queue.Enqueue(logdata);
                if (WriteConsole)
                    Console.WriteLine(logdata);
            }
        }

        static public void Info(string log)
        {
            Log(log, LogLevel.INFO);
        }

        static public void Warning(string log)
        {
            Log(log, LogLevel.WARNING);
        }

        static public void Error(string log)
        {
            Log(log, LogLevel.ERROR);
        }

        static public void Debug(string log)
        {
            Log(log, LogLevel.DEBUG);
        }

        static public void Trace(string log)
        {
            Log(log, LogLevel.TRACE);
        }

        static void Process()
        {
            using (StreamWriter strm = File.AppendText("./output-log"))
            {
                while (true)
                {
                    if (_queue.TryDequeue(out TypedLog log))
                    {
                        strm.WriteLine(log);
                    }
                    else
                    {
                        Thread.Sleep(1000);
                    }
                }
            }
        }
    }
}
