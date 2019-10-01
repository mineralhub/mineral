using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Database2.Common;
using Mineral.Core.Database2.Core;

namespace Mineral.Core.Database
{
    public interface IRevokingDatabase
    {
        int Size { get; }
        int MaxSize { get; set; }
        int MaxFlushCount { get; set; }

        ISession BuildSession();
        ISession BuildSession(bool force_enable);

        void Add(IRevokingDB revoking_db);
        void Merge();
        void Revoke();
        void Commit();
        void Pop();
        void FastPop();
        void Enable();
        void Disable();
        void Check();
        void Shutdown();
        void SetMode(bool mode);
    }
}
