using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Utils;
using Mineral.Core.Capsule;
using static Mineral.Core.Capsule.BlockCapsule;

namespace Mineral.Core.Database
{
    public class KhaosBlock
    {
        #region Field
        private BlockCapsule block = null;
        private WeakReference<KhaosBlock> parent = new WeakReference<KhaosBlock>(null);
        private BlockId id = null;
        private bool is_invalid = false;
        private long num = 0;
        #endregion


        #region Property
        public BlockCapsule Block => this.block;
        public BlockId Id => this.id;
        public bool IsInvalid => is_invalid;
        public long Num => this.num;

        public KhaosBlock Parent
        {
            get
            {
                this.parent.TryGetTarget(out KhaosBlock block);
                return block;
            }
            set { this.parent = new WeakReference<KhaosBlock>(value); }
        }

        public SHA256Hash ParentHash
        {
            get { return SHA256Hash.Wrap(this.block.ParentId.Hash); }
        }
        #endregion


        #region Contructor
        public KhaosBlock(BlockCapsule block)
        {
            this.block = block;
            this.id = block.Id;
            this.num = block.Num;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;

            if (obj == null || GetType() != obj.GetType())
                return false;

            KhaosBlock blk = (KhaosBlock)obj;

            return object.Equals(this.id, blk.id);
        }
        #endregion
    }
}
