using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Mineral.Core.Capsule;
using Mineral.Core.Net.Messages;
using Mineral.Utils;
using Protocol;

namespace Mineral.Core.Net.Messages
{
    public class BlocksMessage : MineralMessage
    {
        #region Field
        private List<Protocol.Block> blocks = null;
        #endregion


        #region Property
        public List<Protocol.Block> Blocks
        {
            get { return this.blocks; }
        }

        public override Type AnswerMessage
        {
            get { return null; }
        }
        #endregion


        #region Contructor
        public BlocksMessage(byte[] data)
            : base(data)
        {
            this.type = (byte)MessageTypes.MsgType.BLOCKS;

            Items items = Items.Parser.ParseFrom(GetCodedInputStream(data));
            if (items.Type == Items.Types.ItemType.Block)
            {
                blocks = new List<Protocol.Block>(items.Blocks);
            }

            if (IsFilter && blocks.IsNotNullOrEmpty())
            {
                CompareBytes(data, items.ToByteArray());
                foreach (Block block in blocks)
                {
                    TransactionCapsule.ValidContractProto(new List<Protocol.Transaction>(block.Transactions));
                }
            }
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override string ToString()
        {
            return base.ToString() + "size: " + (blocks.IsNotNullOrEmpty() ? blocks.Count : 0);
        }
        #endregion
    }
}
