using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Capsule;
using Mineral.Core.Net.Messages;

namespace Mineral.Core.Net.Messages
{
    public class SyncBlockChainMessage : BlockInventoryMessage
    {
        #region Field
        #endregion


        #region Property
        public override Type AnswerMessage
        {
            get { return typeof(ChainInventoryMessage); }
        }
        #endregion


        #region Contructor
        public SyncBlockChainMessage(byte[] packed)
            : base(packed)
        {
            this.type = (byte)MessageTypes.MsgType.SYNC_BLOCK_CHAIN;
        }

        public SyncBlockChainMessage(List<BlockCapsule.BlockId> blockIds)
            : base(blockIds, Protocol.BlockInventory.Types.Type.Sync)
        {
            this.type = (byte)MessageTypes.MsgType.SYNC_BLOCK_CHAIN;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToString())
              .Append("size: ")
              .Append(Ids.Count);
            if (Ids.Count >= 1)
            {
                sb.Append(", start block: " + Ids[0].ToString());
                if (Ids.Count > 1)
                {
                    sb.Append(", end block " + Ids[Ids.Count - 1].ToString());
                }
            }
            return sb.ToString();
        }
        #endregion
    }
}
