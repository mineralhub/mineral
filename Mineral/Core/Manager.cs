using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Mineral.Core.State;
using Mineral.Database.BlockChain;
using Mineral.Utils;

namespace Mineral.Core
{
    public class Manager
    {
        #region Field
        private LevelDBBlockChain _blockChain = new LevelDBBlockChain("./output-database");
        private LevelDBWalletIndexer _walletIndexer = new LevelDBWalletIndexer("./output-wallet-index");
        private LevelDBProperty _properties = new LevelDBProperty("./output-property");
        private CacheBlocks _cacheBlocks = new CacheBlocks(_defaultCacheCapacity);

        private const uint _defaultCacheCapacity = 200000;
        #endregion


        #region Property
        internal LevelDBBlockChain BlockChain { get { return _blockChain; } }
        internal LevelDBWalletIndexer WalletIndexer { get { return _walletIndexer; } }
        internal LevelDBProperty Properties { get { return _properties; } }
        internal CacheBlocks CacheBlocks { get { return _cacheBlocks; } }
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void InitCacheBlock(uint capacity)
        {
            _cacheBlocks = new CacheBlocks(capacity);
        }

        public KeyValuePair<List<Block>, List<Block>> GetBranch(UInt256 hash1, UInt256 hash2)
        {
            List<Block> keys = new List<Block>();
            List<Block> values = new List<Block>();
            Block block1 = _cacheBlocks.GetBlock(hash1);
            Block block2 = _cacheBlocks.GetBlock(hash2);

            if (block1 == null && block2 != null)
            {
                while (!object.Equals(block1.Hash, block2.Hash))
                {
                    if (block1.Height >= block2.Height)
                    {
                        keys.Add(block1);
                        block1 = _cacheBlocks.GetBlock(block1.Header.PrevHash);
                        if (block1 == null)
                        {
                            block1 = _blockChain.GetBlock(block2.Header.PrevHash);
                        }
                    }

                    if (block1.Height <= block2.Height)
                    {
                        values.Add(block2);
                        block2 = _cacheBlocks.GetBlock(block2.Header.PrevHash);
                        if (block2 == null)
                        {
                            block2 = _blockChain.GetBlock(block2.Header.PrevHash);
                        }
                    }
                }
            }

            keys.Reverse();
            values.Reverse();

            return new KeyValuePair<List<Block>, List<Block>>(keys, values);
        }
        #endregion
    }
}
