using System;
using Mineral.Database.LevelDB;

namespace Mineral.Database.BlockChain
{
    internal class LevelDBProperty : BaseLevelDB
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Constructor
        public LevelDBProperty(string path)
            : base(path)
        {
            InitializeProperty(PropertyEntryPrefix.BLOCK_GENERATE_CYCLE_TIME, BitConverter.GetBytes(5));
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void InitializeProperty(byte[] prefix, byte[] defaultValue)
        {
            Slice key = SliceBuilder.Begin().Add(prefix);
            Slice value = SliceBuilder.Begin().Add(defaultValue);
            if (!TryGet(key, out value)) Put(key, value);
        }

        public Slice GetProperty(byte[] prefix)
        {
            return Get(SliceBuilder.Begin().Add(prefix));
        }

        public void SetProperty(byte[] prefix, byte[] value)
        {
            Put(SliceBuilder.Begin().Add(prefix), SliceBuilder.Begin().Add(value));
        }
        #endregion
    }
}
