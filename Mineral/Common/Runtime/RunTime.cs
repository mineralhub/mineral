using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Google.Protobuf;
using Mineral.Common.LogsFilter.Trigger;
using Mineral.Common.Runtime.VM;
using Mineral.Common.Runtime.VM.Exception;
using Mineral.Common.Runtime.VM.Program;
using Mineral.Common.Runtime.VM.Program.Invoke;
using Mineral.Common.Storage;
using Mineral.Common.Utils;
using Mineral.Core;
using Mineral.Core.Actuator;
using Mineral.Core.Capsule;
using Mineral.Core.Config;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Database;
using Mineral.Core.Exception;
using Protocol;
using static Mineral.Common.Runtime.VM.InternalTransaction;
using static Protocol.Transaction.Types;
using static Protocol.Transaction.Types.Contract.Types;
using static Protocol.Transaction.Types.Result.Types;

namespace Mineral.Common.Runtime
{
    using VMConfig = Config.VMConfig;
    using Vm = VM.VM;
    using InternalTransaction = VM.InternalTransaction;

    public class RunTime : IRunTime
    {
        #region Field
        private Transaction transaction = null;
        private BlockCapsule block = null;
        private IDeposit deposit = null;
        private IProgramInvokeFactory invoke_factory = null;
        private string runtime_error = "";

        private EnergyProcessor energy_processor = null;
        private ProgramResult result = new ProgramResult();

        private Vm vm = null;
        private Program program = null;
        private InternalTransaction root_internal_transaction = null;

        private InternalTransaction.TransactionType transaction_type = VM.InternalTransaction.TransactionType.TX_UNKNOWN_TYPE;
        private InternalTransaction.ExecutorType executor_type = VM.InternalTransaction.ExecutorType.ET_UNKNOWN_TYPE;
        private TransactionTrace trace = null;

        private bool is_static_call = false;
        private bool enable_listener = false;
        private LogInfoTriggerParser log_info_parser = null;

        #endregion


        #region Property
        public VM.InternalTransaction.TransactionType TransactionType
        {
            get { return this.transaction_type; }
        }

        public ProgramResult Result
        {
            get { return this.result; }
        }

        public string RuntimeError
        {
            get { return this.runtime_error; }
        }

        private bool IsCheckTransaction
        {
            get { return this.block != null && !this.block.Instance.BlockHeader.WitnessSignature.IsEmpty; }
        }
        #endregion


        #region Contructor
        public RunTime(TransactionTrace trace, BlockCapsule block, Deposit deposit, IProgramInvokeFactory invoke_factory)
        {
            this.trace = trace;
            this.transaction = trace.Transaction.Instance;

            if (block != null)
            {
                this.block = block;
                this.executor_type = ExecutorType.ET_NORMAL_TYPE;
            }
            else
            {
                this.block = new BlockCapsule(new Block());
                this.executor_type = ExecutorType.ET_PRE_TYPE;
            }
            this.deposit = deposit;
            this.invoke_factory = invoke_factory;
            this.energy_processor = new EnergyProcessor(deposit.DBManager);

            ContractType contract_type = this.transaction.RawData.Contract[0].Type;
            switch (contract_type)
            {
                case ContractType.TriggerSmartContract:
                    {
                        this.transaction_type = TransactionType.TX_CONTRACT_CALL_TYPE;
                    }
                    break;
                case ContractType.CreateSmartContract:
                    {
                        this.transaction_type = TransactionType.TX_CONTRACT_CREATION_TYPE;
                    }
                    break;
                default:
                    {
                        this.transaction_type = TransactionType.TX_PRECOMPILED_TYPE;
                    }
                    break;
            }
        }

        public RunTime(Transaction tx, BlockCapsule block, Deposit deposit, IProgramInvokeFactory invoke_factory, bool is_static_call)
            : this(tx, block, deposit, invoke_factory)
        {
            this.is_static_call = is_static_call;
        }

        private RunTime(Transaction tx, BlockCapsule block, Deposit deposit, IProgramInvokeFactory invoke_factory)
        {
            this.transaction = tx;
            this.deposit = deposit;
            this.invoke_factory = invoke_factory;
            this.executor_type = ExecutorType.ET_PRE_TYPE;
            this.block = block;
            this.energy_processor = new EnergyProcessor(deposit.DBManager);
            ContractType contract_type = tx.RawData.Contract[0].Type;
            switch (contract_type)
            {
                case ContractType.TriggerSmartContract:
                    {
                        this.transaction_type = TransactionType.TX_CONTRACT_CALL_TYPE;
                    }
                    break;
                case ContractType.CreateSmartContract:
                    {
                        this.transaction_type = TransactionType.TX_CONTRACT_CREATION_TYPE;
                    }
                    break;
                default:
                    {
                        this.transaction_type = TransactionType.TX_PRECOMPILED_TYPE;
                    }
                    break;
            }
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private void Precompiled()
        {
            TransactionCapsule tx = new TransactionCapsule(this.transaction);
            List<IActuator> actuators = ActuatorFactory.CreateActuator(tx, this.deposit.DBManager);

            foreach (IActuator actuator in actuators)
            {
                actuator.Validate();
                actuator.Execute(this.result.TransactionResult);
            }
        }

        private void Create()
        {
            if (!this.deposit.DBManager.DynamicProperties.SupportVM())
            {
                throw new ContractValidateException("vm work is off, need to be opened by the committee");
            }

            CreateSmartContract contract = ContractCapsule.GetSmartContractFromTransaction(this.transaction);
            if (contract == null)
            {
                throw new ContractValidateException("Cannot get CreateSmartContract from transaction");
            }

            SmartContract new_contract = contract.NewContract;
            if (!contract.OwnerAddress.Equals(new_contract.OriginAddress))
            {
                Logger.Info("OwnerAddress not equals OriginAddress");
                throw new VMIllegalException("OwnerAddress is not equals OriginAddress");
            }

            byte[] contract_name = Encoding.UTF8.GetBytes(new_contract.Name);

            if (contract_name.Length > VMParameter.CONTRACT_NAME_LENGTH)
            {
                throw new ContractValidateException("contractName's length cannot be greater than 32");
            }

            long percent = contract.NewContract.ConsumeUserResourcePercent;
            if (percent < 0 || percent > DefineParameter.ONE_HUNDRED)
            {
                throw new ContractValidateException("percent must be >= 0 and <= 100");
            }

            byte[] contract_address = Wallet.GenerateContractAddress(this.transaction);
            if (this.deposit.GetAccount(contract_address) != null)
            {
                throw new ContractValidateException(
                    "Trying to create a contract with existing contract address: " + Wallet.AddressToBase58(contract_address));
            }

            new_contract.ContractAddress = ByteString.CopyFrom(contract_address);

            long call_value = new_contract.CallValue;
            long token_value = 0;
            long token_id = 0;
            if (VMConfig.AllowTvmTransferTrc10)
            {
                token_value = contract.CallTokenValue;
                token_id = contract.TokenId;
            }

            byte[] caller_address = contract.OwnerAddress.ToByteArray();
            try
            {
                long fee_limit = this.transaction.RawData.FeeLimit;
                if (fee_limit < 0 || fee_limit > VMConfig.MAX_FEE_LIMIT)
                {
                    Logger.Info(string.Format("invalid feeLimit {0}", fee_limit));
                    throw new ContractValidateException(
                        "feeLimit must be >= 0 and <= " + VMConfig.MAX_FEE_LIMIT);
                }

                AccountCapsule creator = this.deposit.GetAccount(new_contract.OriginAddress.ToByteArray());
                long energy_limit = 0;
                if (VMConfig.EnergyLimitHardFork)
                {
                    if (call_value < 0)
                    {
                        throw new ContractValidateException("callValue must >= 0");
                    }
                    if (token_value < 0)
                    {
                        throw new ContractValidateException("tokenValue must >= 0");
                    }
                    if (new_contract.OriginEnergyLimit <= 0)
                    {
                        throw new ContractValidateException("The originEnergyLimit must be > 0");
                    }
                    energy_limit = GetAccountEnergyLimitWithFixRatio(creator, fee_limit, call_value);
                }
                else
                {
                    energy_limit = GetAccountEnergyLimitWithFloatRatio(creator, fee_limit, call_value);
                }

                CheckTokenValueAndId(token_value, token_id);

                byte[] ops = new_contract.Bytecode.ToByteArray();
                this.root_internal_transaction = new InternalTransaction(this.transaction, this.transaction_type);

                long max_cpu_time_tx = this.deposit.DBManager.DynamicProperties.GetMaxCpuTimeOfOneTx() * DefineParameter.ONE_THOUSAND;
                long tx_cpu_limit = (long)(max_cpu_time_tx * GetCpuLimitInUsRatio());
                long vm_start = Helper.NanoTime() / DefineParameter.ONE_THOUSAND;
                long vm_should_end = vm_start + tx_cpu_limit;

                IProgramInvoke invoke = this.invoke_factory.CreateProgramInvoke(TransactionType.TX_CONTRACT_CREATION_TYPE,
                                                                                this.executor_type,
                                                                                this.transaction,
                                                                                token_value,
                                                                                token_id,
                                                                                this.block.Instance,
                                                                                this.deposit,
                                                                                vm_start,
                                                                                vm_should_end,
                                                                                energy_limit);

                this.vm = new Vm();
                this.program = new Program(ops, invoke, this.root_internal_transaction, this.block);
                byte[] tx_id = new TransactionCapsule(this.transaction).Id.Hash;
                this.program.RootTransactionId = tx_id;

                // TODO: EventPluginLoader is not Implementation
                //if (this.enable_listener
                //    && (EventPluginLoader.getInstance().isContractEventTriggerEnable()
                //    || EventPluginLoader.getInstance().isContractLogTriggerEnable())
                //    && IsCheckTransaction)
                //{
                //    logInfoTriggerParser = new LogInfoTriggerParser(this.block.getNum(), this.block.getTimeStamp(), txId, callerAddress);
                //}
            }
            catch (Exception e)
            {
                Logger.Info(e.Message);
                throw new ContractValidateException(e.Message);
            }
            this.program.Result.ContractAddress = contract_address;
            this.deposit.CreateAccount(contract_address, new_contract.Name, AccountType.Contract);
            this.deposit.CreateContract(contract_address, new ContractCapsule(new_contract));
            byte[] code = new_contract.Bytecode.ToByteArray();

            if (!VMConfig.AllowTvmConstantinople)
            {
                deposit.SaveCode(contract_address, ProgramPrecompile.GetCode(code));
            }

            if (call_value > 0)
            {
                MUtil.Transfer(this.deposit, caller_address, contract_address, call_value);
            }
            if (VMConfig.AllowTvmTransferTrc10)
            {
                if (token_value > 0)
                {
                    MUtil.TransferToken(this.deposit, caller_address, contract_address, token_id.ToString(), token_value);
                }
            }
        }

        private void Call()
        {
            if (!this.deposit.DBManager.DynamicProperties.SupportVM())
            {
                Logger.Info("vm work is off, need to be opened by the committee");
                throw new ContractValidateException("VM work is off, need to be opened by the committee");
            }

            TriggerSmartContract contract = ContractCapsule.GetTriggerContractFromTransaction(this.transaction);
            if (contract == null)
                return;

            if (contract.ContractAddress == null)
            {
                throw new ContractValidateException("Cannot get contract address from TriggerContract");
            }

            byte[] contract_address = contract.ContractAddress.ToByteArray();

            ContractCapsule deployed_contract = this.deposit.GetContract(contract_address);
            if (null == deployed_contract)
            {
                Logger.Info("No contract or not a smart contract");
                throw new ContractValidateException("No contract or not a smart contract");
            }

            long call_value = contract.CallValue;
            long token_value = 0;
            long token_id = 0;
            if (VMConfig.AllowTvmTransferTrc10)
            {
                token_value = contract.CallTokenValue;
                token_id = contract.TokenId;
            }

            if (VMConfig.EnergyLimitHardFork)
            {
                if (call_value < 0)
                {
                    throw new ContractValidateException("callValue must >= 0");
                }
                if (token_value < 0)
                {
                    throw new ContractValidateException("tokenValue must >= 0");
                }
            }

            byte[] caller_address = contract.OwnerAddress.ToByteArray();
            CheckTokenValueAndId(token_value, token_id);

            byte[] code = this.deposit.GetCode(contract_address);
            if (code != null && code.Length > 0)
            {

                long fee_limit = this.transaction.RawData.FeeLimit;
                if (fee_limit < 0 || fee_limit > VMConfig.MAX_FEE_LIMIT)
                {
                    Logger.Info(string.Format("invalid feeLimit {0}", fee_limit));
                    throw new ContractValidateException(
                        "feeLimit must be >= 0 and <= " + VMConfig.MAX_FEE_LIMIT);
                }

                AccountCapsule caller = this.deposit.GetAccount(caller_address);
                long energy_limit = 0;
                if (this.is_static_call)
                {
                    energy_limit = DefineParameter.ENERGY_LIMIT_IN_CONSTANT_TX;
                }
                else
                {
                    AccountCapsule creator = this.deposit.GetAccount(deployed_contract.Instance.OriginAddress.ToByteArray());
                    energy_limit = GetTotalEnergyLimit(creator, caller, contract, fee_limit, call_value);
                }

                long max_cpu_time_tx = this.deposit.DBManager.DynamicProperties.GetMaxCpuTimeOfOneTx() * DefineParameter.ONE_THOUSAND;
                long tx_cpu_limit =
                    (long)(max_cpu_time_tx * GetCpuLimitInUsRatio());
                long vm_start = Helper.NanoTime() / DefineParameter.ONE_THOUSAND;
                long vm_should_end = vm_start + tx_cpu_limit;
                IProgramInvoke invoke = this.invoke_factory.CreateProgramInvoke(TransactionType.TX_CONTRACT_CALL_TYPE,
                                                                                this.executor_type,
                                                                                this.transaction,
                                                                                token_value,
                                                                                token_id,
                                                                                this.block.Instance,
                                                                                this.deposit,
                                                                                vm_start,
                                                                                vm_should_end,
                                                                                energy_limit);

                if (this.is_static_call)
                {
                    invoke.IsStaticCall = true;
                }

                this.vm = new Vm();
                this.root_internal_transaction = new InternalTransaction(this.transaction, this.transaction_type);
                this.program = new Program(code, invoke, this.root_internal_transaction, this.block);
                byte[] tx_id = new TransactionCapsule(this.transaction).Id.Hash;
                this.program.RootTransactionId = tx_id;

                // // TODO: EventPluginLoader is not Implementation
                //if (enableEventLinstener &&
                //    (EventPluginLoader.getInstance().isContractEventTriggerEnable()
                //        || EventPluginLoader.getInstance().isContractLogTriggerEnable())
                //    && isCheckTransaction())
                //{
                //    logInfoTriggerParser = new LogInfoTriggerParser(this.block.getNum(), this.block.getTimeStamp(), txId, callerAddress);
                //}
            }

            this.program.Result.ContractAddress = contract_address;
            if (call_value > 0)
            {
                MUtil.Transfer(this.deposit, caller_address, contract_address, call_value);
            }

            if (VMConfig.AllowTvmTransferTrc10)
            {
                if (token_value > 0)
                {
                    MUtil.TransferToken(this.deposit, caller_address, contract_address, token_id.ToString(), token_value);
                }
            }
        }

        private static long GetEnergyFee(long Energy_usage, long energy_frozen, long energy_total)
        {
            if (energy_total <= 0)
                return 0;


            BigInteger result = new BigInteger(energy_frozen);
            result = BigInteger.Multiply(result, new BigInteger(Energy_usage));
            result = BigInteger.Divide(result, new BigInteger(energy_total));

            return (long)result;
        }

        private long GetAccountEnergyLimitWithFloatRatio(AccountCapsule account, long fee_limit, long call_value)
        {
            long sun_per_energy = DefineParameter.SUN_PER_ENERGY;
            if (deposit.DBManager.DynamicProperties.GetEnergyFee() > 0)
            {
                sun_per_energy = deposit.DBManager.DynamicProperties.GetEnergyFee();
            }

            long left_energy_freeze = this.energy_processor.GetAccountLeftEnergyFromFreeze(account);
            call_value = Math.Max(call_value, 0);

            long energy_balance = (long)Math.Floor((double)Math.Max(account.Balance - call_value, 0) / sun_per_energy);
            long energy_fee_limit = 0;
            long total_balance_energy_freeze = account.AllFrozenBalanceForEnergy;
            if (0 == total_balance_energy_freeze)
            {
                energy_fee_limit = fee_limit / sun_per_energy;
            }
            else
            {
                long total_Energy_Freeze = this.energy_processor.CalculateGlobalEnergyLimit(account);
                long left_balance_energy_freeze = GetEnergyFee(total_balance_energy_freeze, left_energy_freeze, total_Energy_Freeze);

                if (left_balance_energy_freeze >= fee_limit)
                {
                    BigInteger org = new BigInteger(total_Energy_Freeze);
                    org = BigInteger.Multiply(org, new BigInteger(fee_limit));
                    org = BigInteger.Divide(org, new BigInteger(total_balance_energy_freeze));
                    energy_fee_limit = (long)org;
                }
                else
                {
                    energy_fee_limit = (left_energy_freeze + (fee_limit - left_balance_energy_freeze)) / sun_per_energy;
                }
            }

            return Math.Min((left_energy_freeze + energy_balance), energy_fee_limit);
        }

        public long GetTotalEnergyLimitWithFixRatio(AccountCapsule creator,
                                                    AccountCapsule caller,
                                                    TriggerSmartContract trigger_contract,
                                                    long fee_limit,
                                                    long call_value)
        {

            long caller_energy_limit = GetAccountEnergyLimitWithFixRatio(caller, fee_limit, call_value);
            if (creator.Address.ToByteArray().SequenceEqual(caller.Address.ToByteArray()))
            {
                return caller_energy_limit;
            }

            long creator_energy_limit = 0;
            ContractCapsule contract = this.deposit.GetContract(trigger_contract.ContractAddress.ToByteArray());
            long consume_resource_percent = contract.GetConsumeUserResourcePercent();
            long origin_energy_limit = contract.GetOriginEnergyLimit();
            if (origin_energy_limit < 0)
            {
                throw new ContractValidateException("origin_energy_limit can't be < 0");
            }

            if (consume_resource_percent <= 0)
            {
                creator_energy_limit = Math.Min(this.energy_processor.GetAccountLeftEnergyFromFreeze(creator), origin_energy_limit);
            }
            else
            {
                if (consume_resource_percent < DefineParameter.ONE_HUNDRED)
                {
                    BigInteger left = new BigInteger(caller_energy_limit);
                    left = BigInteger.Multiply(left, new BigInteger(DefineParameter.ONE_HUNDRED - consume_resource_percent));
                    left = BigInteger.Divide(left, new BigInteger(consume_resource_percent));

                    long right = Math.Min(this.energy_processor.GetAccountLeftEnergyFromFreeze(creator), origin_energy_limit);

                    creator_energy_limit = Math.Min((long)left, right);

                }
            }

            return caller_energy_limit + creator_energy_limit;
        }

        private long GetTotalEnergyLimitWithFloatRatio(AccountCapsule creator,
                                                       AccountCapsule caller,
                                                       TriggerSmartContract trigger_contract,
                                                       long fee_limit,
                                                       long call_value)
        {

            long caller_energy_limit = GetAccountEnergyLimitWithFloatRatio(caller, fee_limit, call_value);
            if (creator.Address.ToByteArray().SequenceEqual(caller.Address.ToByteArray()))
            {
                return caller_energy_limit;
            }

            long creator_energy_limit = this.energy_processor.GetAccountLeftEnergyFromFreeze(creator);
            ContractCapsule contractCapsule = this.deposit.GetContract(trigger_contract.ContractAddress.ToByteArray());
            long consume_resource_percent = contractCapsule.GetConsumeUserResourcePercent();

            if (creator_energy_limit * consume_resource_percent
                > (DefineParameter.ONE_HUNDRED - consume_resource_percent) * caller_energy_limit)
            {
                return (long)Math.Floor(((double)caller_energy_limit * DefineParameter.ONE_HUNDRED) / consume_resource_percent);
            }
            else
            {
                return caller_energy_limit + creator_energy_limit;
            }
        }

        private double GetCpuLimitInUsRatio()
        {
            double reuslt = 0;

            if (this.executor_type == ExecutorType.ET_NORMAL_TYPE)
            {
                if (this.block != null
                    && this.block.IsGenerateMyself
                    && this.block.Instance.BlockHeader.WitnessSignature.IsEmpty)
                {
                    reuslt = 1.0;
                }
                else
                {
                    if (this.transaction.Ret[0].ContractRet == Transaction.Types.Result.Types.contractResult.OutOfTime)
                    {
                        reuslt = (double)Args.Instance.VM.MinTimeRatio;
                    }
                    else
                    {
                        reuslt = (double)Args.Instance.VM.MaxTimeRatio;
                    }
                }
            }
            else
            {
                reuslt = 1.0;
            }

            return reuslt;
        }
        #endregion


        #region External Method
        public void Execute()
        {
            switch (this.transaction_type)
            {
                case TransactionType.TX_PRECOMPILED_TYPE:
                    Precompiled();
                    break;
                case TransactionType.TX_CONTRACT_CREATION_TYPE:
                    Create();
                    break;
                case TransactionType.TX_CONTRACT_CALL_TYPE:
                    Call();
                    break;
                default:
                    throw new ContractValidateException("Unknown contract type");
            }
        }

        public void Finalization()
        {
            if (this.runtime_error == null || this.runtime_error.Length == 0)
            {
                foreach (DataWord contract in this.result.DeleteAccount)
                {
                    this.deposit.DeleteContract(Wallet.ToAddAddressPrefix(contract.GetLast20Bytes()));
                }
            }

            if (VMConfig.Instance.IsVmTrace && this.program != null)
            {
                string trace_content = this.program.Trace.SetResult(this.result.HReturn)
                                                         .SetError(this.result.Exception)
                                                         .ToString();

                if (VMConfig.Instance.IsVmTraceCompressed)
                {
                    trace_content = VMUtil.ZipAndEncode(trace_content);
                }

                string tx_hash = this.root_internal_transaction.Hash.ToHexString();
                VMUtil.SaveProgramTraceFile(VMConfig.Instance, tx_hash, trace_content);
            }
        }

        public void Go()
        {
            try
            {
                if (this.vm != null)
                {
                    TransactionCapsule tx = new TransactionCapsule(this.transaction);
                    if (null != this.block
                        && this.block.IsGenerateMyself
                        && tx.ContractResult != contractResult.Unknown
                        && tx.ContractResult == contractResult.OutOfTime)
                    {
                        this.result = this.program.Result;
                        this.program.SpendAllEnergy();

                        OutOfTimeException e = VMExceptions.AlreadyTimeOut();
                        this.runtime_error = e.Message;
                        this.result.Exception = e;
                        throw e;
                    }

                    vm.Play(program);
                    this.result = this.program.Result;

                    if (this.is_static_call)
                    {
                        long call_value = TransactionCapsule.GetCallValue(this.transaction.RawData.Contract[0]);
                        long call_token_value = TransactionCapsule.GetCallTokenValue(this.transaction.RawData.Contract[0]);
                        if (call_value > 0 || call_token_value > 0)
                        {
                            this.runtime_error = "constant cannot set call value or call token value.";
                            this.result.RejectInternalTransaction();
                        }

                        return;
                    }

                    if (this.transaction_type == TransactionType.TX_CONTRACT_CREATION_TYPE
                        && !this.result.IsRevert)
                    {
                        byte[] code = this.program.Result.HReturn;
                        long save_code_energy = (long)code.Length * EnergyCost.CREATE_DATA;
                        long after_Spend = this.program.EnergyLimitLeft.ToLong() - save_code_energy;
                        if (after_Spend < 0)
                        {
                            if (this.result.Exception == null)
                            {
                                this.result.Exception = VMExceptions.NotEnoughSpendEnergy(
                                                                "save just created contract code",
                                                                save_code_energy,
                                                                this.program.EnergyLimitLeft.ToLong());
                            }
                        }
                        else
                        {
                            this.result.SpendEnergy(save_code_energy);
                            if (VMConfig.AllowTvmConstantinople)
                            {
                                this.deposit.SaveCode(this.program.ContractAddress.GetNoLeadZeroesData(), code);
                            }
                        }
                    }

                    if (this.result.Exception != null || this.result.IsRevert)
                    {
                        this.result.DeleteAccount.Clear();
                        this.result.LogInfos.Clear();
                        this.result.ResetFutureRefund();
                        this.result.RejectInternalTransaction();

                        if (this.result.Exception != null)
                        {
                            if (!(this.result.Exception is TransferException)) {
                                this.program.SpendAllEnergy();
                            }
                            this.runtime_error = this.result.Exception.Message;
                            throw this.result.Exception;
                        }
                        else
                        {
                            this.runtime_error = "REVERT opcode executed";
                        }
                    }
                    else
                    {
                        this.deposit.Commit();

                        if (this.log_info_parser != null)
                        {
                            List<ContractTrigger> triggers = this.log_info_parser.ParseLogInfos(this.program.Result.LogInfos, this.deposit);
                            this.program.Result.Triggers = triggers;
                        }
                    }
                }
                else
                {
                    this.deposit.Commit();
                }
            }
            catch (VMStackOverFlowException e)
            {
                this.program.SpendAllEnergy();
                this.result = this.program.Result;
                this.result.Exception = e;
                this.result.RejectInternalTransaction();
                this.runtime_error = this.result.Exception.Message;
                Logger.Info("JVMStackOverFlowException : " + this.result.Exception.Message);
            }
            catch (OutOfTimeException e)
            {
                this.program.SpendAllEnergy();
                this.result = this.program.Result;
                this.result.Exception = e;
                this.result.RejectInternalTransaction();
                this.runtime_error = result.Exception.Message;
                Logger.Info("timeout : " + this.result.Exception.Message);
            }
            catch (System.Exception e)
            {
                if (!(e is TransferException)) {
                    this.program.SpendAllEnergy();
                }
                this.result = this.program.Result;
                this.result.RejectInternalTransaction();
                if (this.result.Exception == null)
                {
                    Logger.Error(e.Message);
                    this.result.Exception = new System.Exception("Unknown exception");
                }

                if (this.runtime_error == null || this.runtime_error.Length == 0)
                {
                    this.runtime_error = this.result.Exception.Message;
                }
                Logger.Info("runtime result is : " + this.result.Exception.Message);
            }

            this.trace.SetBill(this.result.EnergyUsed);
        }

        public void SetEnableEventListener(bool enable_event_listener)
        {
            this.enable_listener = enable_event_listener;
        }

        public long GetAccountEnergyLimitWithFixRatio(AccountCapsule account, long fee_limit, long call_value)
        {
            long sun_per_energy = DefineParameter.SUN_PER_ENERGY;
            if (this.deposit.DBManager.DynamicProperties.GetEnergyFee() > 0)
            {
                sun_per_energy = this.deposit.DBManager.DynamicProperties.GetEnergyFee();
            }

            long left_frozen_energy = this.energy_processor.GetAccountLeftEnergyFromFreeze(account);
            long energy_balance = Math.Max(account.Balance - call_value, 0) / sun_per_energy;
            long available_energy = left_frozen_energy + energy_balance;
            long energy_fee_limit = fee_limit / sun_per_energy;

            return Math.Min(available_energy, energy_fee_limit);
        }

        public long GetTotalEnergyLimit(AccountCapsule creator,
                                        AccountCapsule caller,
                                        TriggerSmartContract trigger_contract,
                                        long fee_limit,
                                        long call_value)
        {
            if (VMConfig.EnergyLimitHardFork)
            {
                return GetTotalEnergyLimitWithFixRatio(creator, caller, trigger_contract, fee_limit, call_value);
            }
            else
            {
                return GetTotalEnergyLimitWithFloatRatio(creator, caller, trigger_contract, fee_limit, call_value);
            }
        }

        public void CheckTokenValueAndId(long token_value, long token_id)
        {
            if (VMConfig.AllowTvmTransferTrc10)
            {
                if (VMConfig.AllowMultiSign)
                {
                    if (token_id <= VMParameter.MIN_TOKEN_ID && token_id != 0)
                    {
                        throw new ContractValidateException("tokenId must > " + VMParameter.MIN_TOKEN_ID);
                    }

                    if (token_value > 0 && token_id == 0)
                    {
                        throw new ContractValidateException("invalid arguments with tokenValue = " + token_value + ", tokenId = " + token_id);
                    }
                }
            }
        }
        #endregion
    }
}
