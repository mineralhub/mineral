using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Database.BlockChain;

namespace Mineral.Core
{
    public class Manager
    {
        #region Fields
        private LevelDBBlockChain _blockChain = null;
        private LevelDBWalletIndexer _walletIndexer = null;
        private LevelDBProperty _properties = null;
        #endregion


        #region Properties
        internal LevelDBBlockChain BlockChain { get { return _blockChain; } set { _blockChain = value; } }
        internal LevelDBWalletIndexer WalletIndexer { get { return _walletIndexer; } set { _walletIndexer = value; } }
        internal LevelDBProperty Properties { get { return _properties; } set { _properties = value; } }
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
