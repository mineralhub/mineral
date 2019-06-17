using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Runtime;
using Mineral.Common.Runtime.VM;
using Mineral.Common.Storage;
using Mineral.Common.Utils;
using Mineral.Core.Capsule;
using static Protocol.Transaction.Types.Contract.Types;

namespace Mineral.Core.Database
{
    public class TransactionTrace
    {
        public enum TimeResultType
        {
            Normal,
            LongRunning,
            OutOfTime,
        }

        #region Field
        private TransactionCapsule tx = null;
        private ReceiptCapsule receipt = null;
        private Manager db_manager = null;
        private IRunTime runtime = null;
        private EnergyProcessor energy_processor = null;
        private InternalTransaction.TransactionType tx_type = InternalTransaction.TransactionType.TX_UNKNOWN_TYPE;
        private long tx_start_time = 0;
        #endregion


        #region Property
        public TransactionCapsule Transaction => this.tx;

        public bool NeedVM
        {
            get
            {
                return this.tx_type == InternalTransaction.TransactionType.TX_CONTRACT_CALL_TYPE ||
                    this.tx_type == InternalTransaction.TransactionType.TX_CONTRACT_CREATION_TYPE;
            }
        }
        #endregion


        #region Constructor
        public TransactionTrace(TransactionCapsule tx, Manager db_manager)
        {
            this.tx = tx;
            this.db_manager = db_manager;
            this.receipt = new ReceiptCapsule(SHA256Hash.ZERO_HASH);
            this.energy_processor = new EnergyProcessor(this.db_manager);

            ContractType contract_type = this.tx.Instance.RawData.Contract[0].Type;
            switch (contract_type)
            {
                case ContractType.TriggerSmartContract:
                    this.tx_type = InternalTransaction.TransactionType.TX_CONTRACT_CALL_TYPE;
                    break;
                case ContractType.CreateSmartContract:
                    this.tx_type = InternalTransaction.TransactionType.TX_CONTRACT_CREATION_TYPE;
                    break;
                default:
                    this.tx_type = InternalTransaction.TransactionType.TX_PRECOMPILED_TYPE;
                    break;
            }
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Init(BlockCapsule block)
        {
        }

        public void Init(BlockCapsule block, bool event_plugin_loaded)
        {
            this.tx_start_time = DateTime.Now.Ticks;
            Deposit deposit = Deposit.CreateRoot(this.db_manager);
            this.runtime = new IRunTime
        }
        #endregion
    }
}
