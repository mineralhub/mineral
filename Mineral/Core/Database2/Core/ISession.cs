using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Database2.Core
{
    public interface ISession : IDisposable
    {
        void Commit();
        void Revoke();
        void Merge();
        void Destroy();
        void Close();
    }
}
