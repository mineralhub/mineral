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
        private LevelDBBlockChain _blockChain = null;
        private LevelDBWalletIndexer _walletIndexer = null;
        private LevelDBProperty _properties = null;
        private CacheBlocks _cacheBlocks = null;

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
        public void InitGenesistBlock(string path)
        {
            _blockChain = new LevelDBBlockChain(path);
        }

        public void InitWalletIndexer(string path)
        {
            _walletIndexer = new LevelDBWalletIndexer(path);
        }

        public void InitProperty(string path)
        {
            _properties = new LevelDBProperty(path);
        }

        public void InitCacheBlocks(uint capacity)
        {
            _cacheBlocks = new CacheBlocks(capacity);
        }
        #endregion
    }
}
