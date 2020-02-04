using System;
using System.Text;
using System.IO;
using Mineral;
using Mineral.Core.Config.Arguments;
using Mineral.Core;
using Mineral.Common.Application;
using Mineral.Core.Service;
using System.Threading;
using Mineral.Utils;

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
                Profiler.SetLogger(Logger.Refactoring);
                ThreadPool.SetMinThreads(128, 128);
                ThreadPool.GetAvailableThreads(out int worker, out int io);
                Logger.Refactoring(string.Format("ThreadPool available count worker {0}, io {1}", worker, io));
                Manager.Instance.Init();

                Application app = new Application();

                if (Args.Instance.IsWitness)
                    app.AddService(new WitnessService(Manager.Instance));

                app.AddService(new ProposalService());
                app.AddService(new RpcService());

                app.InitService(Args.Instance);
                app.StartService();
                app.Startup();
            }
        }
    }
}
