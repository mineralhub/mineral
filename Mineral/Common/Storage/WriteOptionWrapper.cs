using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Storage
{
    public class WriteOptionWrapper
    {
        #region Field
        private RocksDbSharp.WriteOptions rocks;
        private Stroage.LevelDB.WriteOptions level;
        #endregion


        #region Property
        public RocksDbSharp.WriteOptions Rocks { get => this.rocks; }
        public Stroage.LevelDB.WriteOptions Level { get => this.level; }
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
            wrapper.rocks = new RocksDbSharp.WriteOptions();

            return wrapper;
        }

        public WriteOptionWrapper Sync(bool sync)
        {
            this.level.Sync = sync;
            this.rocks.SetSync(sync);

            return this;
        }
        #endregion
    }
}
