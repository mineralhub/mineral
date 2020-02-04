using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace Mineral
{
    public enum LogLevel
    {
        REFACTORING,
        ERROR,
        WARNING,
        INFO,
        DEBUG,
        TRACE
    }

    internal class TypedLog
    {
        public DateTime TimeStamp;
        public LogLevel LogType;
        public string Message;
        public override string ToString()
        {
            return string.Format("{0} [{1}] {2}", TimeStamp.ToString("yyyy-MM-dd hh:mm:ss.fff"), LogType, Message);
        }
    }

    public static class Logger
    {
        public static bool WriteConsole { get; set; } = true;
        public static LogLevel WriteLogLevel { get; set; } = LogLevel.ERROR;
        private static ConcurrentQueue<TypedLog> _queue = new ConcurrentQueue<TypedLog>();
        private static Thread _thread;

        static Logger()
        {
            _thread = new Thread(Process);
            _thread.Start();
        }

        public static void Log(string log, LogLevel logLevel = LogLevel.INFO)
        {
            TypedLog logdata = new TypedLog() { TimeStamp = DateTime.Now, LogType = logLevel, Message = log };
            if (logdata.LogType <= WriteLogLevel)
            {
                _queue.Enqueue(logdata);
                if (WriteConsole)
                    Console.WriteLine(logdata);
            }
        }

        public static void Refactoring(string log)
        {
            Log(log, LogLevel.REFACTORING);
        }

        public static void Info(string log)
        {
            Log(log, LogLevel.INFO);
        }

        public static void Warning(string log)
        {
            Log(log, LogLevel.WARNING);
        }

        public static void Warning(string log, System.Exception exception)
        {
            Log(log, LogLevel.WARNING);
            Log(exception.Message, LogLevel.WARNING);
            Log(exception.StackTrace, LogLevel.WARNING);
        }

        public static void Error(string log)
        {
            Log(log, LogLevel.ERROR);
        }

        public static void Error(System.Exception exception)
        {
            Log(exception.Message, LogLevel.ERROR);
            Log(exception.StackTrace, LogLevel.ERROR);
        }

        public static void Error(string log, System.Exception exception)
        {
            Log(log, LogLevel.ERROR);
            Log(exception.Message, LogLevel.ERROR);
            Log(exception.StackTrace, LogLevel.ERROR);
        }

        public static void Debug(string log)
        {
            Log(log, LogLevel.DEBUG);
        }

        public static void Trace(string log)
        {
            Log(log, LogLevel.TRACE);
        }

        private const string _logFile = "./MineralHub.log";
        private const long _logSize = 300 * 1024 * 1024; // 300MB

        static void Process()
        {
            DateTime logDate = DateTime.Now;
            StreamWriter strm = File.AppendText(_logFile);
            strm.AutoFlush = true;

            while (true)
            {
                if (_queue.TryDequeue(out TypedLog log))
                {
                    if ((logDate.Date.ToTimestamp() != DateTime.Now.Date.ToTimestamp()) || (strm.BaseStream.Length + log.Message.Length * 2 + 28 >= _logSize))
                    {
                        strm.BaseStream.Flush();
                        strm.Close();
                        string logFileName = string.Format("./MineralHub-{0}.log", logDate.ToString("s").Substring(0, 10));
                        for (int i = 1; i <= 4096; i++)
                        {
                            string logWriteName = logFileName + "." + i;
                            if (File.Exists(logWriteName)) continue;
                            if (File.Exists(logWriteName + ".gz")) continue;
                            File.Move(_logFile, logWriteName);
                            Task.Run(() =>
                            {
                                string orgName = logWriteName.Substring(0);
                                string gzName = orgName + ".gz";

                                Stream inps = File.OpenRead(orgName);
                                Stream outs = File.OpenWrite(gzName);
                                using (GZipStream gzip = new GZipStream(outs, CompressionMode.Compress))
                                {
                                    inps.CopyTo(gzip);
                                }
                                outs.Close();
                                inps.Close();
                                FileStream stm = null;
                                if (File.Exists(gzName) && (stm = File.OpenRead(gzName)).Length > 0)
                                    File.Delete(orgName);
                                if (stm != null) stm.Close();
                            });
                            break;
                        }
                        logDate = DateTime.Now;
                        strm = File.AppendText(_logFile);
                        strm.AutoFlush = true;
                    }
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
