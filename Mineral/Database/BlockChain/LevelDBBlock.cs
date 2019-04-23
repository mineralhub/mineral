using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core;
using Mineral.Core.State;
using Mineral.Database.LevelDB;
using Mineral.Utils;

namespace Mineral.Database.BlockChain
{
    internal class LevelDBBlock : BaseLevelDB, IDisposable
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Constructor
        public LevelDBBlock(string path)
            : base(path)
        {
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void PutBlockHeader(Block block)
        {
            BlockState blockState = new BlockState(block);
            Put(SliceBuilder.Begin(DataEntryPrefix.DATA_Block).Add(block.Hash), SliceBuilder.Begin().Add(blockState.ToArray()));
        }

        public BlockHeader GetBlockHeader(UInt256 headerHash)
        {
            Slice value;
            BlockHeader blockHeader = null;
            bool result = TryGet(new ReadOptions { FillCache = false }, SliceBuilder.Begin(DataEntryPrefix.DATA_Block).Add(headerHash), out value);
            if (result)
            {
                BlockState blockState = BlockState.DeserializeFrom(value.ToArray());
                blockHeader = blockState.Header;
            }
            return blockHeader;
        }

        public KeyValuePair<List<BlockHeader>, List<BlockHeader>> GetBranch(Block block1, Block block2)
        {
            //TODO : Get block branch
            return new KeyValuePair<List<BlockHeader>, List<BlockHeader>>();
        }
        #endregion

    }
}
