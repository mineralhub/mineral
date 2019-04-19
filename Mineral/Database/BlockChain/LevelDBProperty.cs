using System;
using Mineral.Database.LevelDB;

namespace Mineral.Database.BlockChain
{
    internal class LevelDBProperty : BaseLevelDB
    {
        public LevelDBProperty(string path)
            : base(path)
        {
            InitializeProperty(PropertyEntryPrefix.BLOCK_GENERATE_CYCLE_TIME, BitConverter.GetBytes(5));
        }

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

        public void PutProperty(byte[] prefix, byte[] value)
        {
            Put(SliceBuilder.Begin().Add(prefix), SliceBuilder.Begin().Add(value));
        }
    }
}
