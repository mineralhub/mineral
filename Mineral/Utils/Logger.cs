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
        static private Thread _thread;

        static Logger()
        {
            _thread = new Thread(Process);
            _thread.Start();
        }

        static public void Log(string log, LogLevel logLevel = LogLevel.INFO)
        {
            TypedLog logdata = new TypedLog() { timeStamp = DateTime.Now, logType = logLevel, message = log };
            if (logdata.logType <= WriteLogLevel)
            {
                if (WriteConsole)
                    Console.WriteLine(logdata);
                _queue.Enqueue(logdata);
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

        private const string logFile = "./MineralHub.log";
        private const long logSize = 300 * 1024*1024; // 300MB
        static void Process()
        {
            DateTime logDate = DateTime.Now;
            StreamWriter strm = File.AppendText(logFile);
            strm.AutoFlush = true;

            while (true)
            {
                if (_queue.TryDequeue(out TypedLog log))
                {
                    if ((logDate.Date.ToTimestamp() != DateTime.Now.Date.ToTimestamp()) || (strm.BaseStream.Length + log.message.Length * 2 + 28 >= logSize))
                    {
                        strm.BaseStream.Flush();
                        strm.Close();
                        string logFileName = string.Format("./MineralHub-{0}.log", logDate.ToString("s").Substring(0, 10));
                        for (int i = 1; i <= 4096; i++)
                        {
                            string logWriteName = logFileName + "." + i;
                            if (File.Exists(logWriteName)) continue;
                            if (File.Exists(logWriteName + ".gz")) continue;
                            File.Move(logFile, logWriteName);
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
                                if (File.Exists(gzName) && File.OpenRead(gzName).Length > 0)
                                    File.Delete(orgName);
                            });
                            break;
                        }
                        logDate = DateTime.Now;
                        strm = File.AppendText(logFile);
                        strm.AutoFlush = true;
                    }
                    strm.WriteLine(log);
                }
                else
                {
                    Thread.Sleep(50);
                }
            }
        }
    }
}
