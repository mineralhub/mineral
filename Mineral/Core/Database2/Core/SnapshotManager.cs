using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Database;

namespace Mineral.Core.Database2.Core
{
    public class SnapshotManager : IRevokingDatabase
    {
        #region Field
        private static readonly int DEFAULT_STACK_MAX_SIZE = 256;
        public static readonly int DEFAULT_MAX_FLUSH_COUNT = 500;
        public static readonly int DEFAULT_MIN_FLUSH_COUNT = 1;
        #endregion


        #region Property
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Add(IRevokingDatabase revoking_db)
        {
            throw new NotImplementedException();
        }

        public ISession BuildSeesion(bool force_enable)
        {
            throw new NotImplementedException();
        }

        public ISession BuildSession()
        {
            throw new NotImplementedException();
        }

        public void Check()
        {
            throw new NotImplementedException();
        }

        public void Commit()
        {
            throw new NotImplementedException();
        }

        public void Disable()
        {
            throw new NotImplementedException();
        }

        public void Enable()
        {
            throw new NotImplementedException();
        }

        public void FastPop()
        {
            throw new NotImplementedException();
        }

        public void Merge()
        {
            throw new NotImplementedException();
        }

        public void Pop()
        {
            throw new NotImplementedException();
        }

        public void Revoke()
        {
            throw new NotImplementedException();
        }

        public void SetMaxFlushCount(int max_flush_count)
        {
            throw new NotImplementedException();
        }

        public void SetMaxSize(int max_size)
        {
            throw new NotImplementedException();
        }

        public void SetMode(bool mode)
        {
            throw new NotImplementedException();
        }

        public void Shutdown()
        {
            throw new NotImplementedException();
        }

        public int Size()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
