using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Database;

namespace Mineral.Common.Application
{
    public interface IApplication
    {
        void SetOption(Args args);
        void Init(Args args);
        void InitService(Args args);
        void Startup();
        void Shutdown();
        void StartService();
        void ShutdownService();
        void AddService(IService service);
    }
}
