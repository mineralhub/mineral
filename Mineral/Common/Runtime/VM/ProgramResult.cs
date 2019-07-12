using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.LogsFilter.Trigger;
using Mineral.Core.Capsule;

namespace Mineral.Common.Runtime.VM
{
    public class ProgramResult
    {
        #region Field
        private long energy_used = 0;
        private long future_refund = 0;

        private byte[] h_return = new byte[0];
        private byte[] contract_address = new byte[0];
        private System.Exception exception = null;
        private bool is_revert;

        private HashSet<DataWord> delete_account = null;
        private HashSet<byte[]> touch_account = new HashSet<byte[]>();
        private List<LogInfo> log_infos = new List<LogInfo>();
        private List<InternalTransaction> internal_transactions = new List<InternalTransaction>();
        private List<ContractTrigger> triggers = new List<ContractTrigger>();
        private List<CallCreate> call_create = new List<CallCreate>();
        private TransactionResultCapsule transaction_result = new TransactionResultCapsule();
        #endregion


        #region Property
        public long EnergyUsed => energy_used;
        public HashSet<DataWord> DeleteAccount => this.delete_account;
        public HashSet<byte[]> TouchAccount => this.touch_account;
        public List<LogInfo> LogInfos => this.log_infos;
        public List<CallCreate> CallCreate => this.call_create;
        public List<InternalTransaction> InternalTransactions => this.internal_transactions;
        public long FutureRefund => this.future_refund;

        public bool IsRevert
        {
            get { return this.is_revert; }
            set { this.is_revert = value; }
        }

        public List<ContractTrigger> Triggers
        {
            get { return this.triggers; }
            set { this.triggers = value; }
        }

        public byte[] ContractAddress
        {
            get { return this.contract_address; }
            set { this.contract_address = value; }
        }

        public byte[] HReturn
        {
            get { return this.h_return; }
            set { this.h_return = value; }
        }

        public TransactionResultCapsule TransactionResult
        {
            get { return this.transaction_result; }
            set { this.transaction_result = value; }
        }

        public System.Exception Exception
        {
            get { return this.exception; }
            set { this.exception = value; }
        }       
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void SpendEnergy(long energy)
        {
            this.energy_used += energy;
        }

        public void RefundEnergy(long energy)
        {
            this.energy_used -= energy;
        }

        public void AddDeleteAccount(DataWord address)
        {
            this.delete_account.Add(address);
        }

        public void AddDeleteAccount(HashSet<DataWord> addresses)
        {
            foreach (DataWord address in addresses)
            {
                this.delete_account.Add(address);
            }
        }

        public void AddTouchAccount(byte[] address)
        {
            this.touch_account.Add(address);
        }

        public void AddTouchAccount(HashSet<byte[]> addresses)
        {
            foreach (byte[] address in addresses)
            {
                this.touch_account.Add(address);
            }
        }

        public void AddLogInfo(LogInfo log_info)
        {
            this.log_infos.Add(log_info);
        }

        public void AddLogInfo(List<LogInfo> log_infos)
        {
            foreach (LogInfo log_info in log_infos)
            {
                this.log_infos.Add(log_info);
            }
        }

        public void AddCallCreate(byte[] data, byte[] destination, byte[] energy_limit, byte[] value)
        {
            this.call_create.Add(new CallCreate(data, destination, energy_limit, value));
        }

        public InternalTransaction AddInternalTransaction(byte[] parent_hash,
                                                          int deep,
                                                          byte[] sender_address,
                                                          byte[] transfer_address,
                                                          long value,
                                                          byte[] data,
                                                          string note,
                                                          long nonce,
                                                          Dictionary<string, long> token)
        {
            InternalTransaction transaction = new InternalTransaction(parent_hash,
                                                                      deep,
                                                                      this.internal_transactions.Count,
                                                                      sender_address,
                                                                      transfer_address,
                                                                      value,
                                                                      data,
                                                                      note,
                                                                      nonce,
                                                                      token);
            this.internal_transactions.Add(transaction);

            return transaction;
        }

        public void AddInternalTransaction(InternalTransaction transsaction)
        {
            this.internal_transactions.Add(transsaction);
        }

        public void AddInternalTransaction(List<InternalTransaction> transactions)
        {
            this.internal_transactions.AddRange(transactions);
        }

        public void AddFutureRefund(long energy)
        {
            this.future_refund += energy;
        }

        public void ResetFutureRefund()
        {
            this.future_refund = 0;
        }

        public void RejectInternalTransaction()
        {
            foreach (InternalTransaction tx in this.internal_transactions)
            {
                tx.Reject();
            }
        }

        public static ProgramResult CreateEmpty()
        {
            ProgramResult result = new ProgramResult();
            result.h_return = new byte[0];

            return result;
        }

        public void Merge(ProgramResult other)
        {
            AddInternalTransaction(other.InternalTransactions);
            if (other.Exception == null && !other.IsRevert)
            {
                AddDeleteAccount(other.DeleteAccount);
                AddLogInfo(other.LogInfos);
                AddFutureRefund(other.FutureRefund);
                AddTouchAccount(other.TouchAccount);
            }
        }

        public void Reset()
        {
            this.delete_account.Clear();
            this.log_infos.Clear();
            ResetFutureRefund();
        }
        #endregion
    }
}
