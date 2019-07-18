using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Storage
{
    public class WriteOptionWrapper
    {
        #region Field
        private Stroage.LevelDB.WriteOptions level;
        #endregion


        #region Property
        public Stroage.LevelDB.WriteOptions Level
        {
            get { return this.level; }
        }
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public static WriteOptionWrapper GetInstance()
        {
            WriteOptionWrapper wrapper = new WriteOptionWrapper();
            wrapper.level = new Stroage.LevelDB.WriteOptions();

            return wrapper;
        }

        public WriteOptionWrapper Sync(bool sync)
        {
            this.level.Sync = sync;

            return this;
        }
        #endregion
    }
}
