using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Mineral.Core.Exception;
using Protocol;

namespace Mineral.Core.Capsule
{
    public class TransactionResultCapsule : IProtoCapsule<Transaction.Types.Result>
    {
        #region Field
        private Transaction.Types.Result transaction_result = new Transaction.Types.Result();
        #endregion


        #region Property
        public Transaction.Types.Result Instance { get { return this.transaction_result; } }
        public byte[] Data { get { return this.transaction_result.ToByteArray(); } }

        public long Fee
        {
            get { return this.transaction_result.Fee; }
            set { this.transaction_result.Fee = value; }
        }

        public Transaction.Types.Result.Types.code Result
        {
            get { return this.transaction_result.Ret; }
            set { this.transaction_result.Ret = value; }
        }

        public long UnfreezeAmount
        {
            get { return this.transaction_result.UnfreezeAmount; }
            set { this.transaction_result.UnfreezeAmount = value; }
        }

        public string AssetIssueID
        {
            get { return this.transaction_result.AssetIssueID; }
            set { this.transaction_result.AssetIssueID = value; }
        }

        public long WithdrawAmount
        {
            get { return this.transaction_result.WithdrawAmount; }
            set { this.transaction_result.WithdrawAmount = value; }
        }

        public long ExchangeReceivedAmount
        {
            get { return this.transaction_result.ExchangeReceivedAmount; }
            set { this.transaction_result.ExchangeReceivedAmount = value; }
        }

        public long ExchangeWithdrawAnotherAmount
        {
            get { return this.transaction_result.ExchangeWithdrawAnotherAmount; }
            set { this.transaction_result.ExchangeWithdrawAnotherAmount = value; }
        }

        public long ExchangeInjectAnotherAmount
        {
            get { return this.transaction_result.ExchangeInjectAnotherAmount; }
            set { this.transaction_result.ExchangeInjectAnotherAmount = value; }
        }

        public long ExchangeId
        {
            get { return this.transaction_result.ExchangeId; }
            set { this.transaction_result.ExchangeId = value; }
        }
        #endregion


        #region Constructor
        public TransactionResultCapsule() { }
        public TransactionResultCapsule(Transaction.Types.Result transaction_result)
        {
            this.transaction_result = transaction_result;
        }

        public TransactionResultCapsule(byte[] data)
        {
            try
            {
                this.transaction_result = Transaction.Types.Result.Parser.ParseFrom(data);
            }
            catch (InvalidProtocolBufferException e)
            {
                throw new BadItemException("TransactionResult proto data parse exception");
            }
        }

        public TransactionResultCapsule(Transaction.Types.Result.Types.contractResult code)
        {
            this.transaction_result.ContractRet = code;
        }

        public TransactionResultCapsule(Transaction.Types.Result.Types.code code, long fee)
        {
            this.transaction_result.Ret = code;
            this.transaction_result.Fee = fee;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void AddFee(long fee)
        {
            this.transaction_result.Fee += fee;
        }

        public void SetStatus(long fee, Transaction.Types.Result.Types.code code)
        {
            this.transaction_result.Fee += fee;
            this.transaction_result.Ret = code;
        }
        #endregion
    }
}
