using Mineral.Common.Utils;
using Mineral.Core.Capsule;
using System;
using System.Collections.Generic;
using System.Text;
using static Mineral.Core.Capsule.BlockCapsule;

namespace Mineral.Core.Database
{
    public partial class DatabaseManager
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public BlockCapsule GetBlockById(SHA256Hash hash)
        {
            BlockCapsule block = this.khaos_database.GetBlock(hash);
            if (block == null)
            {
                block = this.block_store.Get(hash.Hash);
            }

            return block;
        }

        public BlockCapsule GetBlockByNum(long num)
        {
            return GetBlockById(GetBlockIdByNum(num));
        }

        public BlockId GetBlockIdByNum(long num)
        {
            return this.block_index_store.Get(num);
        }
        #endregion
    }
}
