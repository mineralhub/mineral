using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Config.Arguments;

namespace Mineral.Common.Application
{
    public interface IService
    {
        void Init();
        void Init(Args args);
        void Start();
        void Stop();
    }
}
