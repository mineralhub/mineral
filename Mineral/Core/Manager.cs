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
        #endregion
    }
}
