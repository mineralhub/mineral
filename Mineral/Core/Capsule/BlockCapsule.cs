using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf;
using Mineral.Common.Utils;
using Mineral.Core.Config;
using Mineral.Utils;
using Protocol;

namespace Mineral.Core.Capsule
{
    public class BlockCapsule : IProtoCapsule<Block>
    {
        public class BlockId : SHA256Hash
        {
            private long num;

            public long Num { get { return this.num; } }

            public BlockId() : base(SHA256Hash.ZERO_HASH.Hash) { this.num = 0; }
            public BlockId(SHA256Hash hash, long num) : base(num, hash) { this.num = num; }
            public BlockId(byte[] hash, long num) : base(num, hash) { this.num = num; }
            public BlockId(ByteString hash, long num) : base(num, hash.ToByteArray()) { this.num = num; }
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
        private Block block = null;
        private bool generate_by_myself = false;
        private List<TransactionCapsule> transaction = new List<TransactionCapsule>();

        public Block Instance => throw new NotImplementedException();
        public byte[] Data => throw new NotImplementedException();
        #endregion


        #region Property
        #endregion


        #region Constructor
        public BlockCapsule(long number, SHA256Hash hash, long timestamp, ByteString witness_address)
        {
            BlockHeader header = new BlockHeader();
            header.RawData.Number = number;
            header.RawData.ParentHash = hash.Hash.ToByteString();
            header.RawData.Timestamp = timestamp;
            header.RawData.Version = Parameter.ChainParameters.BLOCK_VERSION;
            header.RawData.WitnessAddress = witness_address;

            this.block = new Block();
            this.block.BlockHeader = header;

            InitTransaction();
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private void InitTransaction()
        {
            this.transaction = this.block.Transactions.Select(tx => new TransactionCapsule(tx));
        }
        #endregion


        #region External Method
        #endregion
    }
}
