using System;
using System.Collections.Concurrent;
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
        private BlockHeader _head = null;
        private ConcurrentDictionary<UInt256, BlockHeader> _blocks = new ConcurrentDictionary<UInt256, BlockHeader>();
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
        protected void PutBlockHeader(Block block)
        {
            BlockState blockState = new BlockState(block);
            Put(SliceBuilder.Begin(DataEntryPrefix.DATA_Block).Add(block.Hash), SliceBuilder.Begin().Add(blockState.ToArray()));
        }

        protected BlockHeader GetBlockHeader(UInt256 headerHash)
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
        #endregion


        #region External Method
        public void SetHead(BlockHeader header)
        {
            _head = header;
        }

        public BlockHeader GetHead(BlockHeader header)
        {
            return _head;
        }

        public void Push(Block block)
        {
            PutBlockHeader(block);
            _head = block.Header;
        }

        public void Pop()
        {
            BlockHeader header = GetBlockHeader(_head.Hash);
            _head = GetBlockHeader(header.PrevHash);
        }

        public KeyValuePair<List<BlockHeader>, List<BlockHeader>> GetBranch(Block block1, Block block2)
        {
            List<BlockHeader> keys = new List<BlockHeader>();
            List<BlockHeader> values = new List<BlockHeader>();
            BlockHeader blockHeader1 = GetBlockHeader(block1.Hash);
            BlockHeader blockHeader2 = GetBlockHeader(block1.Hash);

            if (block1 == null && block2 != null)
            {
                while (!object.Equals(block1.Hash, block2.Hash))
                {
                    if (blockHeader1.Height >= blockHeader2.Height)
                    {
                        keys.Add(blockHeader1);
                        blockHeader1 = GetBlockHeader(block1.Header.PrevHash);
                        if (blockHeader1 == null)
                        {
                            blockHeader1 = GetBlockHeader(block2.Header.PrevHash);
                        }
                    }

                    if (blockHeader1.Height <= blockHeader2.Height)
                    {
                        values.Add(blockHeader2);
                        blockHeader2 = GetBlockHeader(block2.Header.PrevHash);
                        if (blockHeader2 == null)
                        {
                            blockHeader2 = GetBlockHeader(block2.Header.PrevHash);
                        }
                    }
                }
            }

            keys.Reverse();
            values.Reverse();

            return new KeyValuePair<List<BlockHeader>, List<BlockHeader>>(keys, values);
        }
        #endregion

    }
}
