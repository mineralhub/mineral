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

        protected void RemoveHeader(UInt256 headerHash)
        {
            Delete(SliceBuilder.Begin(DataEntryPrefix.DATA_Block).Add(headerHash));
        }
        #endregion


        #region External Method
        public void SetHead(BlockHeader header)
        {
            _head = header;
        }

        public BlockHeader GetHead()
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
            _head = GetBlockHeader(_head.Hash);
            if (_head != null)
            {
                RemoveHeader(_head.Hash);
            }
        }

        public void Remove(UInt256 headerHash)
        {
            RemoveHeader(headerHash);
        }

        public KeyValuePair<List<BlockHeader>, List<BlockHeader>> GetBranch(UInt256 hash1, UInt256 hash2)
        {
            List<BlockHeader> keys = new List<BlockHeader>();
            List<BlockHeader> values = new List<BlockHeader>();
            BlockHeader blockHeader1 = GetBlockHeader(hash1);
            BlockHeader blockHeader2 = GetBlockHeader(hash2);

            if (blockHeader1 == null && blockHeader2 != null)
            {
                while (!object.Equals(blockHeader1.Hash, blockHeader2.Hash))
                {
                    if (blockHeader1.Height >= blockHeader2.Height)
                    {
                        keys.Add(blockHeader1);
                        blockHeader1 = GetBlockHeader(blockHeader1.PrevHash);
                    }

                    if (blockHeader1.Height <= blockHeader2.Height)
                    {
                        values.Add(blockHeader2);
                        blockHeader2 = GetBlockHeader(blockHeader2.PrevHash);
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
