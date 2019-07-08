using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Mineral.Common.Runtime.VM;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Database;
using Mineral.Utils;
using Protocol;
using static Protocol.InternalTransaction.Types;

namespace Mineral.Core.Capsule
{
    public class TransactionInfoCapsule : IProtoCapsule<TransactionInfo>
    {
        #region Field
        private TransactionInfo transaction_info = null;
        #endregion


        #region Property
        public TransactionInfo Instance => this.transaction_info;
        public byte[] Data => this.transaction_info.ToByteArray();

        public long Fee
        {
            get { return this.transaction_info.Fee; }
        }

        public byte[] Id
        {
            get { return this.transaction_info.Id.ToByteArray(); }
            set { this.transaction_info.Id = ByteString.CopyFrom(value); }
        }

        public long BlockNumber
        {
            get { return this.transaction_info.BlockNumber; }
            set { this.transaction_info.BlockNumber = value; }
        }

        public long BlockTimestamp
        {
            get { return this.transaction_info.BlockTimeStamp; }
            set { this.transaction_info.BlockTimeStamp = value; }
        }

        public byte[] ContractAddress
        {
            get { return this.transaction_info.ContractAddress.ToByteArray(); }
            set { this.transaction_info.ContractAddress = ByteString.CopyFrom(value); }
        }

        public ResourceReceipt Receipt
        {
            get { return this.transaction_info.Receipt; }
            set { this.transaction_info.Receipt = value; }
        }

        public long UnfreezeAmount
        {
            get { return this.transaction_info.UnfreezeAmount; }
            set { this.transaction_info.UnfreezeAmount = value; }
        }

        public long WithdrawAmount
        {
            get { return this.transaction_info.WithdrawAmount; }
            set { this.transaction_info.WithdrawAmount = value; }
        }

        public TransactionInfo.Types.code Result
        {
            get { return this.transaction_info.Result; }
            set { this.transaction_info.Result = value; }
        }
        #endregion


        #region Contructor
        public TransactionInfoCapsule()
        {
            this.transaction_info = new TransactionInfo();
        }

        public TransactionInfoCapsule(TransactionInfo transaction_info)
        {
            this.transaction_info = transaction_info;
        }

        public TransactionInfoCapsule(byte[] data)
        {
            try
            {
                this.transaction_info = TransactionInfo.Parser.ParseFrom(data);
            }
            catch (InvalidProtocolBufferException e)
            {
                throw new ArgumentException("TransactionInfoCapsule proto data parse exception.");
            }
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void AddFee(long fee)
        {
            this.transaction_info.Fee += fee;
        }

        public void AddContractResult(byte[] result)
        {
            this.transaction_info.ContractResult.Add(ByteString.CopyFrom(result));
        }

        public void AddAllLog(List<Protocol.TransactionInfo.Types.Log> logs)
        {
            this.transaction_info.Log.AddRange(logs);
        }

        public static TransactionInfoCapsule BuildInstance(TransactionCapsule transaction, BlockCapsule block, TransactionTrace trace)
        {
            TransactionInfo result = new TransactionInfo();
            ReceiptCapsule receipt = trace.Receipt;

            result.Result = TransactionInfo.Types.code.Sucess;
            if (trace.RuntimeError.IsNotNullOrEmpty()
                || trace.Result.Exception != null)
            {
                result.Result = TransactionInfo.Types.code.Failed;
                result.ResMessage = ByteString.CopyFromUtf8(trace.RuntimeError);
            }

            result.Id = ByteString.CopyFrom(transaction.Id.Hash);
            ProgramResult program_result = trace.Result;

            long fee = program_result.TransactionResult.Fee
                       + receipt.EnergyFee
                       + receipt.NetFee
                       + receipt.MultiSignFee;


            result.Fee = fee;
            result.ContractResult.Add(ByteString.CopyFrom(program_result.HReturn));
            result.ContractAddress = ByteString.CopyFrom(program_result.ContractAddress);
            result.UnfreezeAmount = program_result.TransactionResult.UnfreezeAmount;
            result.AssetIssueID = program_result.TransactionResult.AssetIssueID;
            result.ExchangeId = program_result.TransactionResult.ExchangeId;
            result.WithdrawAmount = program_result.TransactionResult.WithdrawAmount;
            result.ExchangeReceivedAmount = program_result.TransactionResult.ExchangeReceivedAmount;
            result.ExchangeInjectAnotherAmount = program_result.TransactionResult.ExchangeInjectAnotherAmount;
            result.ExchangeWithdrawAnotherAmount = program_result.TransactionResult.ExchangeWithdrawAnotherAmount;

            List<TransactionInfo.Types.Log> logs = new List<TransactionInfo.Types.Log>();
            program_result.LogInfos.ForEach(info => logs.Add(LogInfo.BuildLog(info)));
            result.Log.AddRange(logs);

            if (block != null)
            {
                result.BlockNumber = block.Instance.BlockHeader.RawData.Number;
                result.BlockTimeStamp = block.Instance.BlockHeader.RawData.Timestamp;
            }

            result.Receipt = receipt.Receipt;

            if (Args.Instance.VM.SaveInternalTx == true && program_result.InternalTransactions != null)
            {
                foreach (var tx in program_result.InternalTransactions)
                {
                    Protocol.InternalTransaction internal_transaction = new Protocol.InternalTransaction();
                    internal_transaction.Hash = ByteString.CopyFrom(tx.Hash);
                    internal_transaction.CallerAddress = ByteString.CopyFrom(tx.SendAddress);
                    internal_transaction.TransferToAddress = ByteString.CopyFrom(tx.TransferToAddress);

                    CallValueInfo call_value_info = new CallValueInfo();
                    call_value_info.CallValue = tx.Value;
                    internal_transaction.CallValueInfo.Add(call_value_info);

                    foreach (var token_info in tx.TokenInfo)
                    {
                        call_value_info = new CallValueInfo();
                        call_value_info.TokenId = token_info.Key;
                        call_value_info.CallValue = token_info.Value;
                        internal_transaction.CallValueInfo.Add(call_value_info);
                    }

                internal_transaction.Note = ByteString.CopyFrom(Encoding.UTF8.GetBytes(tx.Note));
                internal_transaction.Rejected = tx.IsReject;
                result.InternalTransactions.Add(internal_transaction);
            }
        }

    return new TransactionInfoCapsule(result);
    }
    #endregion
}
}
