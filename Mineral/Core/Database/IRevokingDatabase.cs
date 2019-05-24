using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Database2.Core;

namespace Mineral.Core.Database
{
    public interface IRevokingDatabase
    {
        ISession BuildSession();
        ISession BuildSeesion(bool force_enable);

        void Add(IRevokingDatabase revoking_db);
        void Merge();
        void Revoke();
        void Commit();
        void Pop();
        void FastPop();
        void Enable();
        void Disable();
        void Check();
        void Shutdown();
        int Size();

        void SetMode(bool mode);
        void SetMaxSize(int max_size);
        void SetMaxFlushCount(int max_flush_count);
    }
}
