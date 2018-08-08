using System;
using System.Text;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Sky;
using Sky.Cryptography;
using Sky.Core;
using Sky.Wallets;
using Sky.Core.DPos;
using Sky.Network;

namespace Tester
{
    class Program
    {
        static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(ex.GetType().ToString());
            builder.AppendLine(ex.Message);
            builder.AppendLine(ex.StackTrace);
            builder.AppendLine();
            File.AppendAllText("./error-log", builder.ToString());
        }

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledException;
            MainService service = new MainService();
            service.Run();
        }
    }
}
