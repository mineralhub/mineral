using System;
using System.Text;
using System.IO;
using Mineral;
using Mineral.Core.Config.Arguments;
using Mineral.Core;
using Mineral.Common.Application;
using Mineral.Core.Service;

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

            Logger.Info("Mineral full-node Start...........");

            if (Args.Instance.SetParam(args, DefineParameter.CONF_FILE))
            {
                Application app = new Application();

                if (Args.Instance.IsWitness)
                    app.AddService(new WitnessService(Manager.Instance));

                app.InitService(Args.Instance);
                app.StartService();
                app.Startup();
            }
        }
    }
}
