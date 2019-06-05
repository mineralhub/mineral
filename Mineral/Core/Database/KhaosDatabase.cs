using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Utils;
using Mineral.Core.Capsule;
using static Mineral.Core.Capsule.BlockCapsule;

namespace Mineral.Core.Database
{
    public class KhaosDatabase : MineralDatabase<BlockCapsule>
    {
        public class KhaosBlock
        {
            private BlockCapsule block = null;
            private KhaosBlock parent = null;
            private BlockId id = null;
            private bool invalid = false;
            private long num = 0;

            public SHA256Hash GetParentHash()
            {
                return this.block.
            }
        }


        #region Field
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
        public override bool Contains(byte[] key)
        {
            throw new NotImplementedException();
        }

        public override void Put(byte[] key, BlockCapsule value)
        {
            throw new NotImplementedException();
        }

        public override BlockCapsule Get(byte[] key)
        {
            throw new NotImplementedException();
        }

        public override void Delete(byte[] key)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
