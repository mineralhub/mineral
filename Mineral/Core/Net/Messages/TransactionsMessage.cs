using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Mineral.Core.Capsule;
using Protocol;

namespace Mineral.Core.Net.Messages
{
    public class TransactionsMessage : MineralMessage
    {
        #region Field
        private Transactions transactions;
        #endregion


        #region Property
        public Transactions Transactions => this.transactions;

        public override Type AnswerMessage
        {
            get { return null; }
        }
        #endregion


        #region Constructor
        public TransactionsMessage(List<Transaction> txs)
        {
            txs.ForEach(tx => this.transactions.Transactions_.Add(tx));
            this.type = (byte)MessageTypes.MsgType.TXS;
            this.data = this.transactions.ToByteArray();
        }

        public TransactionsMessage(byte[] data)
            : base(data)
        {
            this.type = (byte)MessageTypes.MsgType.TXS;
            this.transactions = Transactions.Parser.ParseFrom(data);
            if (IsFilter)
            {
                CompareBytes(data, this.transactions.ToByteArray());
                TransactionCapsule.ValidContractProto(new List<Transaction>(this.transactions.Transactions_));
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
            StringBuilder builder = new StringBuilder();
            builder.Append(base.ToString())
                   .Append("tx size : ")
                   .Append(this.transactions.Transactions_.Count);

            return builder.ToString();
        }
        #endregion
    }
}
