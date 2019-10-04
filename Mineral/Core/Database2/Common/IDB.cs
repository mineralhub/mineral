using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Database2.Common
{
    public interface IDB
    {
        string Name { get; }
        string DBName { get; }

        void Reset();
        void Close();
    }
}
