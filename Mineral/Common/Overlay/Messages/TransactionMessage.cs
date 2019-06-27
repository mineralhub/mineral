using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Mineral.Core.Capsule;
using Mineral.Core.Net.Messages;

namespace Mineral.Common.Overlay.Messages
{
    public class TransactionMessage : MineralMessage
    {
        #region Field
        private TransactionCapsule transaction = null;
        #endregion


        #region Property
        public TransactionCapsule Transaction
        {
            get { return this.transaction; }
        }

        public override byte[] MessageId
        {
            get { return this.transaction.Id.Hash; }
        }
        #endregion


        #region Contructor
        public TransactionMessage(byte[] raw_data)
            : base(raw_data)
        {
            this.transaction = new TransactionCapsule(GetCodedInputStream(data));
            this.type = (byte)MessageTypes.MsgType.TX;
            if (Message.IsFilter)
            {
                CompareBytes(data, this.transaction.Data);
                TransactionCapsule.ValidContractProto(this.transaction.Instance.RawData.Contract[0]);
            }
        }

        public TransactionMessage(Protocol.Transaction tx)
        {
            this.transaction = new TransactionCapsule(tx);
            this.type = (byte)MessageTypes.MsgType.TX;
            this.data = tx.ToByteArray();
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override string ToString()
        {
            return new StringBuilder().Append(base.ToString()).Append("Message Id: ").Append(base.MessageId).ToString();
        }
        #endregion
    }
}
