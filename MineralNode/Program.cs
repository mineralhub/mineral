using System;
using System.Text;
using System.IO;
using Mineral;

namespace MineralNode
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

            FullNode node = new FullNode();
            node.Run(args);
            //MainService service = new MainService();
            //if (service.Initialize(args))
            //    service.Run();
        }
    }
}
