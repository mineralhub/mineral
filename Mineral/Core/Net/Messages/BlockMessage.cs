using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Utils;
using Mineral.Core.Capsule;
using Mineral.Core.Net.Messages;

namespace Mineral.Core.Net.Messages
{
    public class BlockMessage : MineralMessage
    {
        #region Field
        private BlockCapsule block = null;
        #endregion


        #region Property
        public BlockCapsule Block
        {
            get { return this.block; }
        }

        public override SHA256Hash MessageId
        {
            get { return this.block.Id; }
        }

        public override Type AnswerMessage
        {
            get { return null; }
        }
        #endregion


        #region Contructor
        public BlockMessage(byte[] raw_data)
            : base(raw_data)
        {
            this.type = (byte)MessageTypes.MsgType.BLOCK;
            this.block = new BlockCapsule(GetCodedInputStream(data));
            if (Message.IsFilter)
            {
                Message.CompareBytes(data, block.Data);
                TransactionCapsule.ValidContractProto(new List<Protocol.Transaction>(block.Instance.Transactions));
            }
        }

        public BlockMessage(BlockCapsule block)
        {
            this.data = block.Data;
            this.type = (byte)MessageTypes.MsgType.BLOCK;
            this.block = block;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override string ToString()
        {
            return new StringBuilder().Append(base.ToString())
                                      .Append(block.Id.ToString())
                                      .Append(", tx size: ")
                                      .Append(block.Transactions.Count)
                                      .Append("\n")
                                      .ToString();
        }
        #endregion
    }
}
