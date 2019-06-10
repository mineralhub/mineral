using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Utils;
using Mineral.Core.Capsule;
using Mineral.Core.Exception;
using Mineral.Utils;
using static Mineral.Core.Capsule.BlockCapsule;

namespace Mineral.Core.Database
{
    public class BlockIndexStore : MineralStoreWithRevoking<BytesCapsule, object>
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public BlockIndexStore(string db_name = "block-index") : base(db_name) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Put(BlockId id)
        {
            Put(BitConverter.GetBytes(id.Num), new BytesCapsule(id.Hash));
        }

        public BlockId Get(long num)
        {
            BytesCapsule value = GetUnchecked(BitConverter.GetBytes(num));
            if (value == null || value.Data == null)
            {
                throw new ItemNotFoundException("number : " + num + " is no found");
            }

            return new BlockId(SHA256Hash.Wrap(value.Data), num);
        }

        public override BytesCapsule Get(byte[] key)
        {
            byte[] value = this.revoking_db.GetUnchecked(key);
            if (value.IsNullOrEmpty())
            {
                throw new ItemNotFoundException("number : " + key.ToString() + " is no found");
            }

            return new BytesCapsule(value);
        }
        #endregion
    }
}
