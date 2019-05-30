using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Mineral.Common.Utils;
using Protocol;

namespace Mineral.Core.Capsule
{
    public class BlockCapsule : IProtoCapsule<Protocol.Block>
    {
        public class BlockId : SHA256Hash
        {
            private long num;

            public long Num { get { return this.num; } }

            public BlockId() : base(SHA256Hash.ZERO_HASH.Hash) { this.num = 0; }
            public BlockId(SHA256Hash hash, long num) : base(num, hash) { this.num = num; }
            public BlockId(byte[] hash, long num) : base(num, hash) { this.num = num; }
            public BlockId(ByteString hash, long num) : base(num, hash) { this.num = num; }
            public BlockId(SHA256Hash block_id)
                : base(block_id.Hash)
            {
                byte[] block_num = new byte[8];
                Array.Copy(block_id.Hash, 0, block_num, 0, 8);
                num = BitConverter.ToInt64(block_num, 0);
            }
        }

        #region Field
        private BlockId block_id = new BlockId(SHA256Hash.ZERO_HASH, 0);
        private Protocol.Block block = null;
        private bool generate_by_myself = false;
        private List<TransactionCapsule> transaction = new List<TransactionCapsule>();
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
        public byte[] GetData()
        {
            throw new NotImplementedException();
        }

        public Block GetInstance()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
