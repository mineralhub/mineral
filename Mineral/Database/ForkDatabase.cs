using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Mineral.Core;
using Mineral.Utils;

namespace Mineral.Database
{
    public class ForkDatabase
    {
        
        #region Field
        private Block _head = null;

        private ConcurrentDictionary<UInt256, Block> _blocks = new ConcurrentDictionary<UInt256, Block>();
        private ConcurrentDictionary<uint, List<UInt256>> _heightBlocks = new ConcurrentDictionary<uint, List<UInt256>>();
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
        public void SetHead(Block block)
        {
            _head = block;
        }

        public Block GetHead()
        {
            return _head;
        }

        public Block GetBlock(UInt256 hash)
        {
            return _blocks.ContainsKey(hash) ? _blocks[hash] : null;
        }

        public void Push(Block block)
        {
            _blocks.TryAdd(block.Hash, block);
            if (!_heightBlocks.ContainsKey(block.Height))
                _heightBlocks.TryAdd(block.Height, new List<UInt256>() { block.Hash });
            else
                _heightBlocks[block.Height].Add(block.Hash);

            _head = block;
        }

        public void Pop()
        {
            _head = GetBlock(_head.Header.PrevHash);
        }

        public void Remove(UInt256 hash)
        {
            if (_blocks.TryRemove(hash, out Block block))
            {
                if (_heightBlocks.ContainsKey(block.Height))
                {
                    List<UInt256> hashs = _heightBlocks[block.Height];
                    hashs.Remove(block.Hash);
                    if (hashs.Count < 0)
                    {
                        _heightBlocks.TryRemove(block.Height, out _);
                    }
                }
            }
        }
        #endregion
    }
}
