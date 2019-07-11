using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Runtime;
using Mineral.Common.Runtime.VM;
using Mineral.Common.Runtime.VM.Exception;
using Mineral.Common.Runtime.VM.Program.Invoke;
using Mineral.Common.Storage;
using Mineral.Common.Utils;
using Mineral.Core.Capsule;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Exception;
using Protocol;
using static Mineral.Common.Runtime.VM.InternalTransaction;
using static Protocol.SmartContract.Types;
using static Protocol.Transaction.Types.Contract.Types;
using static Protocol.Transaction.Types.Result.Types;

namespace Mineral.Core.Database
{
    using VMConfig = Common.Runtime.Config.VMConfig;
    using InternalTransaction = Common.Runtime.VM.InternalTransaction;

    public class TransactionTrace
    {
        public enum TimeResultType
        {
            Normal,
            LongRunning,
            OutOfTime,
        }

        #region Field
        private DatabaseManager db_manager = null;
        private TransactionCapsule transaction = null;
        private ReceiptCapsule receipt = null;
        private IRunTime runtime = null;
        private EnergyProcessor energy_processor = null;
        private InternalTransaction.TransactionType transaction_type = InternalTransaction.TransactionType.TX_UNKNOWN_TYPE;
        private TimeResultType time_result_type;
        private long tx_start_time = 0;
        #endregion


        #region Property
        public TransactionCapsule Transaction => this.transaction;

        public ReceiptCapsule Receipt
        {
            get { return this.receipt; }
        }

        public IRunTime Runtime
        {
            get { return this.runtime; }
        }

        public string RuntimeError
        {
            get { return this.runtime.RuntimeError; }
        }

        public ProgramResult Result
        {
            get { return this.runtime.Result; }
        }

        public TimeResultType TimeResult
        {
            get { return this.time_result_type; }
            set { this.time_result_type = value; }
        }

        public bool IsNeedVM
        {
            get
            {
                return this.transaction_type == InternalTransaction.TransactionType.TX_CONTRACT_CALL_TYPE ||
                    this.transaction_type == InternalTransaction.TransactionType.TX_CONTRACT_CREATION_TYPE;
            }
        }
        #endregion


        #region Constructor
        public TransactionTrace(TransactionCapsule tx, DatabaseManager db_manager)
        {
            this.transaction = tx;
            this.db_manager = db_manager;
            this.receipt = new ReceiptCapsule(SHA256Hash.ZERO_HASH);
            this.energy_processor = new EnergyProcessor(this.db_manager);

            ContractType contract_type = this.transaction.Instance.RawData.Contract[0].Type;
            switch (contract_type)
            {
                case ContractType.TriggerSmartContract:
                    this.transaction_type = InternalTransaction.TransactionType.TX_CONTRACT_CALL_TYPE;
                    break;
                case ContractType.CreateSmartContract:
                    this.transaction_type = InternalTransaction.TransactionType.TX_CONTRACT_CREATION_TYPE;
                    break;
                default:
                    this.transaction_type = InternalTransaction.TransactionType.TX_PRECOMPILED_TYPE;
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
            Init(block, false);
        }

        public void Init(BlockCapsule block, bool event_plugin_loaded)
        {
            this.tx_start_time = Helper.CurrentTimeMillis();
            Deposit deposit = Deposit.CreateRoot(this.db_manager);
            this.runtime = new RunTime(this, block, deposit, new ProgramInvokeFactory());
            this.runtime.SetEnableEventListener(event_plugin_loaded);
        }

        public bool CheckNeedRetry()
        {
            if (!IsNeedVM)
                return false;

            return this.transaction.ContractResult != contractResult.OutOfTime
                   && this.receipt.Result == contractResult.OutOfTime;
        }

        public void Check()
        {
            if (!IsNeedVM)
                return;

            if (this.transaction.ContractResult != contractResult.Unknown)
            {
                throw new ReceiptCheckErrorException("null result_code");
            }
            if (!this.transaction.ContractResult.Equals(this.receipt.Result))
            {
                Logger.Info(
                    string.Format(
                        "this tx id: {0}, the resultCode in received block: {1}, the resultCode in self: {2}",
                        this.transaction.Id.Hash.ToHexString(),
                        this.transaction.ContractResult,
                        this.receipt.Result));

                throw new ReceiptCheckErrorException("Different resultCode");
            }
        }

        public void CheckIsConstant()
        {
            if (VMConfig.AllowTvmConstantinople)
            {
                return;
            }

            TriggerSmartContract trigger_contract = ContractCapsule.GetTriggerContractFromTransaction(this.Transaction.Instance);
            if (this.transaction_type == TransactionType.TX_CONTRACT_CALL_TYPE)
            {
                Deposit deposit = Deposit.CreateRoot(this.db_manager);
                ContractCapsule contract = deposit.GetContract(trigger_contract.ContractAddress.ToByteArray());
                if (contract == null)
                {
                    Logger.Info(string.Format("contract: {0} is not in contract store",
                                              Wallet.Encode58Check(trigger_contract.ContractAddress.ToByteArray())));

                    throw new ContractValidateException("contract: "
                                                        + Wallet.Encode58Check(trigger_contract.ContractAddress.ToByteArray())
                                                        + " is not in contract store");
                }
                ABI abi = contract.Instance.Abi;
                if (Wallet.IsConstant(abi, trigger_contract))
                {
                    throw new VMIllegalException("cannot call constant method");
                }
            }
        }

        public void SetBill(long energy_usage)
        {
            if (energy_usage < 0)
                energy_usage = 0L;

            this.receipt.EnergyUsageTotal = energy_usage;
        }

        public void AddNetBill(long netFee)
        {
            receipt.AddNetFee(netFee);
        }

        public void SetNetBill(long net_usage, long net_fee)
        {
            this.receipt.NetUsage = net_usage;
            this.receipt.NetFee = net_fee;
        }

        public void Execute()
        {
            this.runtime.Execute();
            this.runtime.Go();

            if (this.runtime.TransactionType != TransactionType.TX_PRECOMPILED_TYPE)
            {
                if (this.receipt.Result.Equals(contractResult.OutOfTime))
                {
                    this.time_result_type = TimeResultType.OutOfTime;
                }
                else if (Helper.CurrentTimeMillis() - this.tx_start_time > Args.Instance.LongRunningTime)
                {
                    this.time_result_type = TimeResultType.LongRunning;
                }
            }
        }

        public void Finalization()
        {
            try
            {
                Pay();
            }
            catch (BalanceInsufficientException e)
            {
                throw new ContractExeException(e.Message);
            }

            this.runtime.Finalization();
        }

        public void Pay()
        {
            byte[] origin_account = null;
            byte[] caller_account = null;
            long percent = 0;
            long origin_energy_limit = 0;

            switch (this.transaction_type)
            {
                case TransactionType.TX_CONTRACT_CREATION_TYPE:
                    {
                        caller_account = TransactionCapsule.GetOwner(this.transaction.Instance.RawData.Contract[0]);
                        origin_account = caller_account;
                    }
                    break;
                case TransactionType.TX_CONTRACT_CALL_TYPE:
                    {
                        TriggerSmartContract trigger_contract = ContractCapsule.GetTriggerContractFromTransaction(this.transaction.Instance);
                        ContractCapsule contract = this.db_manager.Contract.Get(trigger_contract.ContractAddress.ToByteArray());

                        caller_account = trigger_contract.OwnerAddress.ToByteArray();
                        origin_account = contract.OriginAddress;
                        percent = Math.Max(DefineParameter.ONE_HUNDRED - contract.GetConsumeUserResourcePercent(), 0);
                        percent = Math.Min(percent, DefineParameter.ONE_HUNDRED);
                        origin_energy_limit = contract.GetOriginEnergyLimit();
                    }
                    break;
                default:
                    return;
            }

            this.receipt.PayEnergyBill(this.db_manager,
                                       this.db_manager.Account.Get(origin_account),
                                       this.db_manager.Account.Get(caller_account),
                                       percent,
                                       origin_energy_limit,
                                       energy_processor,
                                       this.db_manager.WitnessController.GetHeadSlot());
        }

        public void SetResult()
        {
            if (!IsNeedVM)
                return;

            System.Exception exception = this.runtime.Result.Exception;
            if (exception == null
                && this.runtime.RuntimeError == null
                && this.runtime.RuntimeError.Length == 0
                && !this.runtime.Result.IsRevert)
            {
                this.receipt.Result = contractResult.Success;
                return;
            }
            if (this.runtime.Result.IsRevert)
            {
                this.receipt.Result = contractResult.Revert;
                return;
            }
            if (exception is IllegalOperationException)
            {
                this.receipt.Result = contractResult.IllegalOperation;
                return;
            }
            if (exception is OutOfEnergyException)
            {
                this.receipt.Result = contractResult.OutOfEnergy;
                return;
            }
            if (exception is BadJumpDestinationException)
            {
                this.receipt.Result = contractResult.BadJumpDestination;
                return;
            }
            if (exception is OutOfTimeException)
            {
                this.receipt.Result = contractResult.OutOfTime;
                return;
            }
            if (exception is Common.Runtime.VM.Exception.OutOfMemoryException)
            {
                this.receipt.Result = contractResult.OutOfMemory;
                return;
            }
            if (exception is PrecompiledContractException)
            {
                this.receipt.Result = contractResult.PrecompiledContract;
                return;
            }
            if (exception is StackTooSmallException)
            {
                this.receipt.Result = contractResult.StackTooSmall;
                return;
            }
            if (exception is StackTooLargeException)
            {
                this.receipt.Result = contractResult.StackTooLarge;
                return;
            }
            if (exception is VMStackOverFlowException)
            {
                this.receipt.Result = contractResult.StackOverflow;
                return;
            }
            if (exception is TransferException)
            {
                this.receipt.Result = contractResult.TransferFailed;
                return;
            }

            Logger.Error("Uncaught exception");
            this.receipt.Result = contractResult.Unknown;
        }
        #endregion
    }
}
