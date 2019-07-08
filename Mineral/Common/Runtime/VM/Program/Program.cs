using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Google.Protobuf;
using Mineral.Common.Runtime.VM.Exception;
using Mineral.Common.Runtime.VM.Program.Invoke;
using Mineral.Common.Runtime.VM.Program.Listener;
using Mineral.Common.Runtime.VM.Trace;
using Mineral.Common.Storage;
using Mineral.Common.Utils;
using Mineral.Core;
using Mineral.Core.Actuator;
using Mineral.Core.Capsule;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Exception;
using Mineral.Cryptography;
using Mineral.Utils;
using Protocol;

namespace Mineral.Common.Runtime.VM.Program
{
    using VMConfig = Runtime.Config.VMConfig;

    public class Program
    {
        #region Field
        private static readonly int MAX_DEPTH = 64;
        private static readonly int MAX_STACK_SIZE = 1024;
        public static readonly string VALIDATE_FOR_SMART_CONTRACT_FAILURE = "validateForSmartContract failure";

        private long nonce = 0;
        private BlockCapsule block = null;
        private byte[] root_transaction_id = null;
        private InternalTransaction internal_transaction = null;
        private IProgramInvoke invoke = null;
        private IProgramInvokeFactory invoke_factory = null;
        private IProgramOutListener listener = null;
        private ProgramTraceListener trace_listener = null;
        private ProgramStorageChangeListener storage_diff_listener = null;
        private CompositeProgramListener program_listener = null;

        private DataWordStack stack = null;
        private Memory memory = null;
        private ContractState contract_state = null;
        private byte[] return_data = null;

        private ProgramResult result = new ProgramResult();
        private ProgramTrace trace = new ProgramTrace();

        private byte[] ops = null;
        private int pc = 0;
        private byte last_op = 0x00;
        private byte prev_executed_op = 0x00;
        private bool is_stopped = false;

        private ProgramPrecompile program_precompile = null;
        #endregion


        #region Property
        public Memory Memory => this.memory;
        public DataWordStack Stack => this.stack;
        public ContractState ContractState => this.contract_state;
        public ProgramTrace Trace => this.Trace;

        public byte[] Code
        {
            get { return ByteUtil.CopyRange(this.ops, 0, this.ops.Length); }
        }

        public byte[] RootTransactionId
        {
            get { return (byte[])this.root_transaction_id.Clone(); }
            set { this.root_transaction_id = (byte[])value.Clone(); }
        }

        public long Nonce
        {
            get { return this.nonce; }
            set { this.nonce = value; }
        }

        public ProgramPrecompile ProgramPrecompole
        {
            get { return this.program_precompile = this.program_precompile ?? ProgramPrecompile.Compile(this.ops); }
        }

        public ProgramResult Result
        {
            get { return this.result; }
        }

        public DataWord PrevHash
        {
            get { return this.invoke.PrevHash; }
        }

        public DataWord ContractAddress
        {
            get { return new DataWord(this.invoke.ContractAddress.Clone()); }
        }

        public DataWord OriginAddress
        {
            get { return new DataWord(this.invoke.OriginAddress.Clone()); }
        }

        public DataWord CallerAddress
        {
            get { return new DataWord(this.invoke.CallerAddress.Clone()); }
        }

        public DataWord CallValue
        {
            get { return new DataWord(this.invoke.CallValue.Clone()); }
        }

        public DataWord TokenValue
        {
            get { return new DataWord(this.invoke.TokenValue.Clone()); }
        }

        public DataWord TokenId
        {
            get { return new DataWord(this.invoke.TokenId.Clone()); }
        }

        public DataWord Coinbase
        {
            get { return new DataWord(this.invoke.Coinbase.Clone()); }
        }

        public DataWord Number
        {
            get { return new DataWord(this.invoke.Number.Clone()); }
        }

        public DataWord Difficulty
        {
            get { return new DataWord(this.invoke.Difficulty.Clone()); }
        }

        public DataWord Timestamp
        {
            get { return new DataWord(this.invoke.Timestamp.Clone()); }
        }

        public bool IsStaticCall
        {
            get { return this.invoke.IsStaticCall; }
            set { this.invoke.IsStaticCall = value; }
        }

        public int PC
        {
            get { return this.pc; }
            set
            {
                this.pc = value;
                if (this.pc >= this.ops.Length)
                    Stop();
            }
        }

        public byte Op
        {
            get { return this.ops?.Length <= this.pc ? (byte)0 : this.ops[this.pc]; }
        }

        public byte CurrentOp
        {
            get { return this.ops.IsNotNullOrEmpty() ? this.ops[pc] : (byte)0; }
        }

        public byte LastOp
        {
            get { return this.last_op; }
            set { this.last_op = value; }
        }

        public byte PrevExecutedOp
        {
            get { return this.prev_executed_op; }
            set { this.prev_executed_op = value; }
        }

        public Dictionary<DataWord, DataWord> StorageDifference
        {
            get { return this.storage_diff_listener.Defference; }
        }

        public int MemorySize
        {
            get { return this.memory.Size; }
        }

        public int CallDeep
        {
            get { return this.invoke.CallDeep; }
        }

        public DataWord EnergyLimitLeft
        {
            get { return new DataWord(this.invoke.EnergyLimit - this.result.EnergyUsed); }
        }

        public long EnergylimitLeftLong
        {
            get { return this.invoke.EnergyLimit - this.result.EnergyUsed; }
        }

        public bool IsStopped
        {
            get { return this.is_stopped; }
        }
        #endregion


        #region Contructor
        public Program(byte[] ops, IProgramInvoke invoke) : this(ops, invoke, null) { }
        public Program(byte[] ops, IProgramInvoke invoke, InternalTransaction tx) : this(ops, invoke, tx, null) { }
        public Program(byte[] ops, IProgramInvoke invoke, InternalTransaction tx, BlockCapsule block)
        {
            this.invoke = invoke;
            this.internal_transaction = tx;
            this.block = block;
            this.ops = ops ?? new byte[0];

            this.trace_listener = new ProgramTraceListener(VMConfig.Instance.IsVmTrace);
            this.memory = SetupProgramListener(new Memory());
            this.stack = SetupProgramListener(new DataWordStack());
            this.contract_state = SetupProgramListener(new ContractState(invoke));
            this.trace = new ProgramTrace(invoke);
            this.nonce = tx.Nonce;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private InternalTransaction AddInternalTransaction(DataWord energy_limit,
                                                           byte[] sender_address,
                                                           byte[] transfer_address,
                                                           long value,
                                                           byte[] data,
                                                           string note,
                                                           long nonce,
                                                           Dictionary<string, long> token_info)
        {
            InternalTransaction result = null;
            if (this.internal_transaction != null)
            {
                result = this.result.AddInternalTransaction(
                                                internal_transaction.Hash,
                                                CallDeep,
                                                sender_address,
                                                transfer_address,
                                                value,
                                                data,
                                                note,
                                                nonce,
                                                token_info);
            }

            return result;
        }

        private T SetupProgramListener<T>(T program_listener_aware) where T : IProgramListenerAware
        {
            if (this.program_listener.IsEmpty)
            {
                this.program_listener.AddListener(this.trace_listener);
                this.program_listener.AddListener(this.storage_diff_listener);
            }
            program_listener_aware.SetProgramListener(this.program_listener);

            return program_listener_aware;
        }

        private void CreateContract(DataWord value, byte[] program_code, byte[] new_address)
        {
            byte[] sender_address = Wallet.ToMineralAddress(this.invoke.ContractAddress.GetLast20Bytes());

            Logger.Debug(string.Format("creating a new contract inside contract run: [{0}]", sender_address.ToHexString()));

            long endowment = (long)value.ToBigInteger();
            if (this.contract_state.GetBalance(sender_address) < endowment)
            {
                StackPushZero();
                return;
            }

            AccountCapsule exist_address = this.contract_state.GetAccount(new_address);
            bool already_exists = exist_address != null;

            IDeposit deposit = this.contract_state.NewDepositChild();
            long old_balance = deposit.GetBalance(new_address);

            SmartContract smart_contract = new SmartContract();
            smart_contract.ContractAddress = ByteString.CopyFrom(new_address);
            smart_contract.ConsumeUserResourcePercent = 100;
            smart_contract.OriginAddress = ByteString.CopyFrom(sender_address);

            deposit.CreateContract(new_address, new ContractCapsule(smart_contract));
            deposit.CreateAccount(new_address, "CreatedByContract", Protocol.AccountType.Contract);
            deposit.AddBalance(new_address, old_balance);

            long new_balance = 0;
            if (!IsTestingSuite() && endowment > 0)
            {
                try
                {
                    TransferActuator.ValidateForSmartContract(deposit, sender_address, new_address, endowment);
                }
                catch (ContractValidateException e)
                {
                    throw new ByteCodeExecutionException(VALIDATE_FOR_SMART_CONTRACT_FAILURE);
                }
                deposit.AddBalance(sender_address, -endowment);
                new_balance = deposit.AddBalance(new_address, endowment);
            }
            SpendEnergy(EnergyLimitLeft.ToLong(), "internal call");

            IncreaseNonce();

            InternalTransaction internal_transaction = AddInternalTransaction(null,
                                                                              sender_address,
                                                                              new_address,
                                                                              endowment,
                                                                              program_code,
                                                                              "create",
                                                                              nonce,
                                                                              null);
            long vm_start_in = Helper.NanoTime() / 1000;
            IProgramInvoke program_invoke = this.invoke_factory.CreateProgramInvoke(
                                                                    this,
                                                                    new DataWord(new_address),
                                                                    this.invoke.ContractAddress,
                                                                    value,
                                                                    new DataWord(0),
                                                                    new DataWord(0),
                                                                    new_balance,
                                                                    null,
                                                                    deposit,
                                                                    false,
                                                                    IsTestingSuite(),
                                                                    vm_start_in,
                                                                    this.invoke.VMShouldEndInUs,
                                                                    EnergyLimitLeft.ToLongSafety());

            ProgramResult create_result = ProgramResult.CreateEmpty();

            if (already_exists)
            {
                create_result.Exception = new ByteCodeExecutionException(
                        string.Format(
                            "Trying to create a contract with existing contract address: 0x" + new_address.ToHexString()));
            }
            else if (program_code.IsNotNullOrEmpty())
            {
                VM vm = new VM();
                Program program = new Program(program_code, program_invoke, internal_transaction, this.block);
                program.RootTransactionId = this.root_transaction_id;
                vm.Play(program);
                create_result = program.Result;
                this.trace.Merge(program.Trace);
                this.nonce = program.nonce;

            }

            byte[] code = create_result.HReturn;
            long save_code_energy = (long)code.Length * EnergyCost.CREATE_DATA;

            long after_spend = this.invoke.EnergyLimit - create_result.EnergyUsed - save_code_energy;
            if (!create_result.IsRevert)
            {
                if (after_spend < 0)
                {
                    create_result.Exception = VMExceptions.NotEnoughSpendEnergy(
                                                            "No energy to save just created contract code",
                                                            save_code_energy,
                                                            this.invoke.EnergyLimit - create_result.EnergyUsed
                                                            );
                }
                else
                {
                    create_result.SpendEnergy(save_code_energy);
                    deposit.SaveCode(new_address, code);
                }
            }

            this.result.Merge(create_result);

            if (create_result.Exception != null || create_result.IsRevert)
            {
                Logger.Debug(
                    string.Format(
                        "contract run halted by Exception: contract: [{0}], exception: [{1}]",
                        new_address.ToHexString(),
                        create_result.Exception));

                this.internal_transaction.Reject();
                create_result.RejectInternalTransaction();

                StackPushZero();

                if (create_result.Exception != null)
                    return;
                else
                    this.return_data = create_result.HReturn;
            }
            else
            {
                if (!IsTestingSuite())
                {
                    deposit.Commit();
                }

                StackPush(new DataWord(new_address));
            }

            RefundEnergyAfterVM(EnergyLimitLeft, create_result);
        }
        #endregion


        #region External Method
        public void CreateContract(DataWord value, DataWord mem_start, DataWord mem_size)
        {
            this.return_data = null;
            if (this.invoke.CallDeep == MAX_DEPTH)
            {
                StackPushZero();
                return;
            }

            byte[] program_code = MemoryChunk(mem_start.ToInt(), mem_size.ToInt());
            byte[] address = Wallet.GenerateContractAddress(this.root_transaction_id, this.nonce);

            CreateContract(value, program_code, address);
        }

        public void CreateContract2(DataWord value, DataWord mem_start, DataWord mem_size, DataWord salt)
        {
            byte[] sender_address = Wallet.ToMineralAddress(this.invoke.CallerAddress.GetLast20Bytes());
            byte[] program_code = MemoryChunk(mem_start.ToInt(), mem_size.ToInt());

            byte[] contract_address = Wallet.GenerateContractAddress2(sender_address, salt.Data, program_code);
            CreateContract(value, program_code, contract_address);
        }

        public bool IsTestingSuite()
        {
            return this.invoke.IsTestingSuite;
        }

        public void SpendEnergy(long energy_value, string op_name)
        {
            if (EnergylimitLeftLong < energy_value)
            {
                throw new OutOfEnergyException(string.Format(
                    "Not enough energy for '%s' operation executing: curInvokeEnergyLimit[%d],"
                        + " curOpEnergy[%d], usedEnergy[%d]",
                    op_name, this.invoke.EnergyLimit, energy_value, this.result.EnergyUsed));
            }
            this.result.SpendEnergy(energy_value);
        }

        public void SpendAllEnergy()
        {
            SpendEnergy(EnergyLimitLeft.ToLong(), "Spending all remaining");
        }

        public void RefundEnergy(long energy_value, string cause)
        {
            Logger.Debug(
                string.Format(
                    "[{0}] Refund for cause: [{1}], energy: [{2}]",
                    this.invoke.GetHashCode(),
                    cause,
                    energy_value));
            Result.RefundEnergy(energy_value);
        }

        public void RefundEnergyAfterVM(DataWord energy_limit, ProgramResult result)
        {
            long refund_energy = energy_limit.ToLongSafety() - result.EnergyUsed;
            if (refund_energy > 0)
            {
                RefundEnergy(refund_energy, "remain energy from the internal call");
                Logger.Debug(
                    string.Format(
                        "The remaining energy is refunded, account: [{0}], energy: [{1}] ",
                        Wallet.ToMineralAddress(ContractAddress.GetLast20Bytes()).ToHexString(),
                        refund_energy));
            }
        }

        public void ResetFutureRefund()
        {
            this.result.ResetFutureRefund();
        }

        public void CheckCPUTimeLimit(string opName)
        {
            if (Args.Instance.IsSolidityNode)
                return;

            long vm_now = Helper.NanoTime() / 1000;
            if (vm_now > this.invoke.VMShouldEndInUs)
            {
                Logger.Info(
                    string.Format(
                        "minTimeRatio: {0}, maxTimeRatio: {1}, vm should end time in us: {2}, "
                        + "vm now time in us: {3}, vm start time in us: {4}",
                        Args.Instance.VM.MinTimeRatio,
                        Args.Instance.VM.MaxTimeRatio,
                        this.invoke.VMShouldEndInUs, vm_now, this.invoke.VMStartInUs));

                throw VMExceptions.NotEnoughTime(opName);
            }
        }

        public void CheckTokenId(MessageCall msg)
        {
            if (VMConfig.AllowMultiSign)
            {
                long token_id = (long)msg.TokenId.ToBigInteger();
                if ((token_id <= VMParameter.MIN_TOKEN_ID && token_id != 0)
                    || (token_id == 0 && msg.IsTokenTransfer))
                {
                    throw new ByteCodeExecutionException(
                        VALIDATE_FOR_SMART_CONTRACT_FAILURE + ", not valid token id");
                }
            }
        }

        public bool IsTokenTransfer(MessageCall msg)
        {
            if (VMConfig.AllowMultiSign)
            {
                return msg.IsTokenTransfer;
            }
            else
            {
                return msg.TokenId.ToLong() != 0;
            }
        }

        public void VerifyStackSize(int size)
        {
            if (this.stack.Size < size)
            {
                throw VMExceptions.TooSmallStack(size, this.stack.Size);
            }
        }

        public void VerifyStackOverflow(int args_request, int return_request)
        {
            if ((this.stack.Size - args_request + return_request) > MAX_STACK_SIZE)
            {
                throw new StackTooLargeException(
                    "Expected: overflow " + MAX_STACK_SIZE + " elements stack limit");
            }
        }

        public int VerifyJumpDest(DataWord next_pc)
        {
            if (next_pc.BytesOccupied() > 4)
            {
                throw VMExceptions.BadJumpDestination(-1);
            }
            int result = next_pc.ToInt();
            if (!ProgramPrecompole.HasJumpDest(result))
            {
                throw VMExceptions.BadJumpDestination(result);
            }
            return result;
        }

        public void CallToPrecompiledAddress(MessageCall msg, PrecompiledContracts.PrecompiledContract contract)
        {
            this.return_data = null;

            if (CallDeep == MAX_DEPTH)
            {
                StackPushZero();
                RefundEnergy(msg.Energy.ToLong(), " call deep limit reach");
                return;
            }

            IDeposit deposit = this.contract_state.NewDepositChild();

            byte[] sender_address = Wallet.ToMineralAddress(ContractAddress.GetLast20Bytes());
            byte[] code_address = Wallet.ToMineralAddress(msg.CodeAddress.GetLast20Bytes());
            byte[] context_address = OpCodeUtil.ContainStateless(msg.Type) ? sender_address : code_address;

            long endowment = (long)msg.Endowment.ToBigInteger();
            long sender_balance = 0;
            byte[] token_id = null;

            CheckTokenId(msg);
            bool is_token_transfer = IsTokenTransfer(msg);
            // transfer trx validation
            if (!is_token_transfer)
            {
                sender_balance = deposit.GetBalance(sender_address);
            }
            else
            {
                token_id = Encoding.UTF8.GetBytes(msg.TokenId.ToLong().ToString());
                sender_balance = deposit.GetTokenBalance(sender_address, token_id);
            }
            if (sender_balance < endowment)
            {
                StackPushZero();
                RefundEnergy(msg.Energy.ToLong(), "refund energy from message call");
                return;
            }
            byte[] data = this.MemoryChunk(msg.InDataOffset.ToInt(), msg.InDataSize.ToInt());

            // Charge for endowment - is not reversible by rollback
            if (sender_address.IsNotNullOrEmpty()
                && context_address.IsNotNullOrEmpty()
                && sender_address != context_address && (long)msg.Endowment.ToBigInteger() > 0)
            {
                if (!is_token_transfer)
                {
                    try
                    {
                        MUtil.Transfer(deposit, sender_address, context_address, (long)msg.Endowment.ToBigInteger());
                    }
                    catch (ContractValidateException e)
                    {
                        throw new ByteCodeExecutionException("transfer failure");
                    }
                }
                else
                {
                    try
                    {
                        TransferAssetActuator.ValidateForSmartContract(deposit, sender_address, context_address, token_id, endowment);
                    }
                    catch (ContractValidateException e)
                    {
                        throw new ByteCodeExecutionException(VALIDATE_FOR_SMART_CONTRACT_FAILURE);
                    }
                    deposit.AddTokenBalance(sender_address, token_id, -endowment);
                    deposit.AddTokenBalance(context_address, token_id, endowment);
                }
            }

            long required_energy = contract.GetEnergyForData(data);
            if (required_energy > msg.Energy.ToLong())
            {
                RefundEnergy(0, "call pre-compiled");
                StackPushZero();
            }
            else
            {
                contract.CallerAddress = Wallet.ToMineralAddress(OpCodeUtil.ContainDelegate(msg.Type) ?
                    CallerAddress.GetLast20Bytes() : ContractAddress.GetLast20Bytes());

                contract.Desposit = deposit;
                contract.Result = this.result;
                contract.IsStaticCall = IsStaticCall;
                KeyValuePair<bool, byte[]> output = contract.Execute(data);

                if (output.Key)
                {
                    RefundEnergy(msg.Energy.ToLong() - required_energy, "call pre-compiled");
                    StackPushOne();
                    this.return_data = output.Value;
                    deposit.Commit();
                }
                else
                {
                    RefundEnergy(0, "call pre-compiled");
                    StackPushZero();
                    if (this.result.Exception != null)
                    {
                        throw result.Exception;
                    }
                }

                MemorySave(msg.OutDataOffset.ToInt(), output.Value);
            }
        }

        public void CallToAddress(MessageCall msg)
        {
            this.return_data = null;

            if (CallDeep == MAX_DEPTH)
            {
                StackPushZero();
                RefundEnergy(msg.Energy.ToLong(), " call deep limit reach");
                return;
            }

            byte[] data = MemoryChunk(msg.InDataOffset.ToInt(), msg.InDataSize.ToInt());
            byte[] code_address = Wallet.ToMineralAddress(msg.CodeAddress.GetLast20Bytes());
            byte[] sender_address = Wallet.ToMineralAddress(ContractAddress.GetLast20Bytes());
            byte[] context_address = OpCodeUtil.ContainStateless(msg.Type) ? sender_address : code_address;

            Logger.Debug(msg.Type.ToString()
                        + string.Format(" for existing contract: address: [{0}], outDataOffs: [{1}], outDataSize: [{2}]  ",
                                        context_address.ToHexString(), msg.OutDataOffset.ToLong(),msg.OutDataSize.ToLong()));

            IDeposit deposit = this.contract_state.NewDepositChild();
            long endowment = 0;

            try
            {
                endowment = (long)msg.Endowment.ToBigInteger();
            }
            catch (ArithmeticException e)
            {
                if (VMConfig.AllowTvmConstantinople)
                    throw new TransferException("endowment out of long range");
                else
                    throw e;
            }

            byte[] token_id = null;
            CheckTokenId(msg);

            bool is_token_transfer = IsTokenTransfer(msg);

            if (!is_token_transfer)
            {
                long sender_balance = deposit.GetBalance(sender_address);
                if (sender_balance < endowment)
                {
                    StackPushZero();
                    RefundEnergy(msg.Energy.ToLong(), "refund energy from message call");
                    return;
                }
            }
            else
            {
                token_id = Encoding.UTF8.GetBytes(msg.TokenId.ToLong().ToString());
                long senderBalance = deposit.GetTokenBalance(sender_address, token_id);
                if (senderBalance < endowment)
                {
                    StackPushZero();
                    RefundEnergy(msg.Energy.ToLong(), "refund energy from message call");
                    return;
                }
            }

            AccountCapsule account = this.contract_state.GetAccount(code_address);

            byte[] program_code =
                account != null ? this.contract_state.GetCode(code_address) : new byte[0];

            long context_balance = 0L;
            if (IsTestingSuite())
            {
                this.result.AddCallCreate(data, context_address, msg.Energy.GetNoLeadZeroesData(), msg.Endowment.GetNoLeadZeroesData());
            }
            else if (sender_address.IsNotNullOrEmpty() && context_address.IsNotNullOrEmpty()
                    && sender_address != context_address
                    && endowment > 0)
            {
                if (!is_token_transfer)
                {
                    try
                    {
                        TransferActuator.ValidateForSmartContract(deposit, sender_address, context_address, endowment);
                    }
                    catch (ContractValidateException e)
                    {
                        if (VMConfig.AllowTvmConstantinople)
                        {
                            RefundEnergy(msg.Energy.ToLong(), "refund energy from message call");
                            throw new TransferException("transfer trx failed: " + e.Message);
                        }
                        throw new ByteCodeExecutionException(VALIDATE_FOR_SMART_CONTRACT_FAILURE);
                    }
                    deposit.AddBalance(sender_address, -endowment);
                    context_balance = deposit.AddBalance(context_address, endowment);
                }
                else
                {
                    try
                    {
                        TransferAssetActuator.ValidateForSmartContract(deposit, sender_address, context_address, token_id, endowment);
                    }
                    catch (ContractValidateException e)
                    {
                        if (VMConfig.AllowTvmConstantinople)
                        {
                            RefundEnergy(msg.Energy.ToLong(), "refund energy from message call");
                            throw new TransferException("transfer trc10 failed: " + e.Message);
                        }
                        throw new ByteCodeExecutionException(VALIDATE_FOR_SMART_CONTRACT_FAILURE);
                    }
                    deposit.AddTokenBalance(sender_address, token_id, -endowment);
                    deposit.AddTokenBalance(context_address, token_id, endowment);
                }
            }

            IncreaseNonce();
            Dictionary<string, long> token_info = new Dictionary<string, long>();
            if (is_token_transfer)
            {
                token_info.Add(Encoding.UTF8.GetString(ByteUtil.StripLeadingZeroes(token_id)), endowment);
            }
            InternalTransaction internal_tx = AddInternalTransaction(
                                                            null,
                                                            sender_address,
                                                            context_address,
                                                            !is_token_transfer ? endowment : 0, data, "call", nonce,
                                                            !is_token_transfer ? null : token_info);
            ProgramResult call_result = null;
            if (program_code.IsNotNullOrEmpty())
            {
                long vm_start_in = Helper.NanoTime() / 1000;
                DataWord call_value = OpCodeUtil.ContainDelegate(msg.Type) ? CallValue : msg.Endowment;

                IProgramInvoke program_invoke =
                    this.invoke_factory.CreateProgramInvoke(this,
                                                            new DataWord(context_address),
                                                            OpCodeUtil.ContainDelegate(msg.Type) ? CallerAddress : ContractAddress,
                                                            !is_token_transfer ? CallValue : new DataWord(0),
                                                            !is_token_transfer ? new DataWord(0) : CallValue,
                                                            !is_token_transfer ? new DataWord(0) : msg.TokenId,
                                                            context_balance,
                                                            data,
                                                            deposit,
                                                            OpCodeUtil.ContainStatic(msg.Type) || IsStaticCall,
                                                            IsTestingSuite(),
                                                            vm_start_in,
                                                            this.invoke.VMShouldEndInUs,
                                                            msg.Energy.ToLongSafety());

                VM vm = new VM();
                Program program = new Program(program_code, program_invoke, internal_transaction, this.block);
                program.RootTransactionId = this.root_transaction_id;
                vm.Play(program);
                call_result = program.result;

                this.trace.Merge(program.Trace);
                this.result.Merge(call_result);
                // always commit nonce
                this.nonce = program.nonce;

                if (call_result.Exception != null || call_result.IsRevert)
                {
                    Logger.Debug(
                        string.Format("contract run halted by Exception: contract: [{0}], exception: [{1}]",
                                       context_address.ToHexString(),
                                       call_result.Exception));

                    internal_tx.Reject();
                    call_result.RejectInternalTransaction();
                    StackPushZero();

                    if (call_result.Exception != null)
                    {
                        return;
                    }
                }
                else
                {
                    deposit.Commit();
                    StackPushOne();
                }

                if (IsTestingSuite())
                {
                    Logger.Debug("Testing run, skipping storage diff listener");
                }
            }
            else
            {
                deposit.Commit();
                StackPushOne();
            }

            if (call_result != null)
            {
                byte[] buffer = call_result.HReturn;
                int offset = msg.OutDataOffset.ToInt();
                int size = msg.OutDataSize.ToInt();

                MemorySaveLimited(offset, buffer, size);

                this.return_data = buffer;
            }

            if (call_result != null)
            {
                BigInteger refund_energy = BigInteger.Subtract(msg.Energy.ToBigInteger(), new BigInteger(call_result.EnergyUsed));
                if (refund_energy.Sign > 0)
                {
                    RefundEnergy((long)refund_energy, "remaining energy from the internal call");
                    Logger.Debug(
                        string.Format("The remaining energy refunded, account: [{0}], energy: [{1}] ",
                                      sender_address.ToHexString(),
                                      refund_energy.ToString()));
                }
            }
            else
            {
                RefundEnergy(msg.Energy.ToLong(), "remaining esnergy from the internal call");
            }
        }

        public void StackPush(byte[] data)
        {
            StackPush(new DataWord(data));
        }

        public void StackPush(DataWord data)
        {
            VerifyStackOverflow(0, 1);
            this.stack.Push(data);
        }

        public void StackPushZero()
        {
            StackPush(new DataWord(0));
        }

        public void StackPushOne()
        {
            StackPush(new DataWord(1));
        }

        public DataWord StackPop()
        {
            return this.stack.Pop();
        }

        public void SetHReturn(byte[] buff)
        {
            this.result.HReturn = buff;
        }

        public void Step()
        {
            PC = this.pc + 1;
        }

        public byte[] Sweep(int n)
        {
            if (this.pc + n > this.ops.Length)
            {
                Stop();
            }

            byte[] data = ArrayUtils.SubArray(this.ops, pc, pc + n);
            this.pc += n;
            if (this.pc >= this.ops.Length)
            {
                Stop();
            }

            return data;
        }

        public void Stop()
        {
            this.is_stopped = true;
        }

        public void MemorySave(DataWord address, DataWord value)
        {
            memory.Write(address.ToInt(), value.Data, value.Data.Length, false);
        }

        public void MemorySave(int address, byte[] value)
        {
            memory.Write(address, value, value.Length, false);
        }

        public void MemorySave(int address, int alloc_size, byte[] value)
        {
            memory.ExtendAndWrite(address, alloc_size, value);
        }

        public void MemorySaveLimited(int address, byte[] data, int dataSize)
        {
            memory.Write(address, data, dataSize, true);
        }

        public void MemoryExpand(DataWord data_offset, DataWord data_size)
        {
            if (!data_offset.IsZero)
            {
                memory.Extend(data_offset.ToInt(), data_size.ToInt());
            }
        }

        public DataWord MemoryLoad(DataWord address)
        {
            return memory.ReadWord(address.ToInt());
        }

        public DataWord MemoryLoad(int address)
        {
            return memory.ReadWord(address);
        }

        public byte[] MemoryChunk(int offset, int size)
        {
            return memory.Read(offset, size);
        }

        public void AllocateMemory(int offset, int size)
        {
            memory.Extend(offset, size);
        }

        public void StorageSave(DataWord word1, DataWord word2)
        {
            DataWord key = new DataWord(word1.Clone());
            DataWord value = new DataWord(word2.Clone());

            this.contract_state.PutStorageValue(Wallet.ToMineralAddress(ContractAddress.GetLast20Bytes()), key, value);
        }

        public void IncreaseNonce()
        {
            this.nonce++;
        }

        public void ResetNonce()
        {
            this.nonce = 0;
        }

        public void Suicide(DataWord obtainer_address)
        {
            byte[] owner = Wallet.ToMineralAddress(ContractAddress.GetLast20Bytes());
            byte[] obtainer = Wallet.ToMineralAddress(obtainer_address.GetLast20Bytes());
            long balance = this.contract_state.GetBalance(owner);

            Logger.Debug(string.Format("Transfer to: [{0}] heritage: [{1}]", obtainer.ToHexString(), balance));

            IncreaseNonce();

            AddInternalTransaction(null, owner, obtainer, balance, null, "suicide", nonce,
                this.contract_state.GetAccount(owner).AssetV2);

            if (ByteUtil.Compare(owner, 0, 20, obtainer, 0, 20) == 0)
            {
                this.contract_state.AddBalance(owner, -balance);
                byte[] blackhole_address = this.contract_state.GetBlackHoleAddress();

                if (VMConfig.AllowTvmTransferTrc10)
                {
                    this.contract_state.AddBalance(blackhole_address, balance);
                    MUtil.TransferAllToken(this.contract_state, owner, blackhole_address);
                }
            }
            else
            {
                try
                {
                    MUtil.Transfer(this.contract_state, owner, obtainer, balance);
                    if (VMConfig.AllowTvmTransferTrc10)
                    {
                        MUtil.TransferAllToken(this.contract_state, owner, obtainer);
                    }
                }
                catch (ContractValidateException e)
                {
                    if (VMConfig.AllowTvmConstantinople)
                    {
                        throw new TransferException(string.Format(
                            "transfer all token or transfer all trx failed in suicide: %s", e.Message));
                    }
                    throw new ByteCodeExecutionException("transfer failure");
                }
            }
            this.result.AddDeleteAccount(ContractAddress);
        }

        public void FullTrace()
        {
            if (this.listener != null)
            {
                StringBuilder stack_data = new StringBuilder();
                for (int i = 0; i < stack.Size; ++i)
                {
                    stack_data.Append(" ")
                              .Append(stack.Get(i));
                    if (i < stack.Size - 1)
                    {
                        stack_data.Append("\n");
                    }
                }

                if (stack_data.Length > 0)
                {
                    stack_data.Insert(0, "\n");
                }

                StringBuilder memory_data = new StringBuilder();
                StringBuilder one_line = new StringBuilder();
                if (memory.Size > 320)
                {
                    memory_data.Append("... Memory Folded.... ")
                               .Append("(")
                               .Append(memory.Size)
                               .Append(") bytes");
                }
                else
                {
                    for (int i = 0; i < memory.Size; ++i)
                    {
                        byte value = memory.ReadByte(i);
                        one_line.Append(Helper.ToHexString(value))
                                .Append(" ");

                        if ((i + 1) % 16 == 0)
                        {
                            string tmp = string.Format("[{0}]-[{0}]",
                                                        (i - 15).ToString("X4"),
                                                        i.ToString("X4").Replace(" ", "0"));

                            memory_data.Append("")
                                       .Append(tmp)
                                       .Append(" ");
                            memory_data.Append(one_line);
                            if (i < memory.Size)
                            {
                                memory_data.Append("\n");
                            }
                            one_line.Length = 0;
                        }
                    }
                }
                if (memory_data.Length > 0)
                {
                    memory_data.Insert(0, "\n");
                }

                StringBuilder ops_string = new StringBuilder();
                for (int i = 0; i < ops.Length; ++i)
                {
                    string tmp_string = (ops[i] & 0xFF).ToString("X4");
                    tmp_string = tmp_string.Length == 1 ? "0" + tmp_string : tmp_string;

                    if (i != pc)
                    {
                        ops_string.Append(tmp_string);
                    }
                    else
                    {
                        ops_string.Append(" >>")
                                  .Append(tmp_string)
                                  .Append("");
                    }

                }
                if (pc >= ops.Length)
                {
                    ops_string.Append(" >>");
                }
                if (ops_string.Length > 0)
                {
                    ops_string.Insert(0, "\n ");
                }

                Logger.Trace(string.Format(" -- OPS --     {0}", ops_string));
                Logger.Trace(string.Format(" -- STACK --   {0}", stack_data));
                Logger.Trace(string.Format(" -- MEMORY --  {0}", memory_data));
                Logger.Trace(string.Format("\n  Spent Drop: [{0}]/[{1}]\n  Left Energy:  [{2}]\n",
                                           this.result.EnergyUsed,
                                           this.invoke.EnergyLimit,
                                           EnergyLimitLeft.ToLong()));

                StringBuilder global_output = new StringBuilder("\n");
                if (stack_data.Length > 0)
                {
                    stack_data.Append("\n");
                }

                if (pc != 0)
                {
                    global_output.Append("[Op: ")
                                 .Append(((OpCode)this.last_op).ToString())
                                 .Append("]\n");
                }

                global_output.Append(" -- OPS --     ").Append(ops_string).Append("\n");
                global_output.Append(" -- STACK --   ").Append(stack_data).Append("\n");
                global_output.Append(" -- MEMORY --  ").Append(memory_data).Append("\n");

                if (this.result.HReturn != null)
                {
                    global_output.Append("\n  HReturn: ")
                                 .Append(this.result.HReturn);
                }

                byte[] tx_data = this.invoke.GetDataCopy(DataWord.ZERO, GetDataSize());
                if (!tx_data.SequenceEqual(ops))
                {
                    global_output.Append("\n  msg.data: ")
                                 .Append(tx_data.ToHexString());
                }
                global_output.Append("\n\n  Spent Energy: ")
                             .Append(this.result.EnergyUsed);

                if (listener != null)
                {
                    listener.Output(global_output.ToString());
                }
            }
        }

        public void SaveOpTrace()
        {
            if (this.pc < this.ops.Length)
            {
                this.trace.AddOp(this.ops[this.pc], this.pc, CallDeep, EnergyLimitLeft, this.trace_listener.ResetActions());
            }
        }

        public DataWord GetBalance(DataWord address)
        {
            return new DataWord(this.contract_state.GetBalance(Wallet.ToMineralAddress(address.GetLast20Bytes())));
        }

        public void CheckTokenIdInTokenBalance(DataWord token_id)
        {
            if (VMConfig.AllowMultiSign)
            {
                long token_value = (long)token_id.ToBigInteger();
                if (token_value <= VMParameter.MIN_TOKEN_ID)
                {
                    throw new ByteCodeExecutionException(VALIDATE_FOR_SMART_CONTRACT_FAILURE + ", not valid token id");
                }
            }
        }

        public DataWord GetTokenBalance(DataWord address, DataWord token_id)
        {
            CheckTokenIdInTokenBalance(token_id);
            long result = this.contract_state.GetTokenBalance(
                                            Wallet.ToMineralAddress(address.GetLast20Bytes()),
                                            Encoding.UTF8.GetBytes(token_id.ToLong().ToString()));

            return result == 0 ? new DataWord(0) : new DataWord(result);
        }

        public DataWord StorageLoad(DataWord key)
        {
            DataWord result = this.contract_state.GetStorageValue(
                Wallet.ToMineralAddress(ContractAddress.GetLast20Bytes()), new DataWord(key.Clone()));

            return result == null ? null : new DataWord(result.Clone());
        }

        public void FutureRefundEnergy(long energy_value)
        {
            Logger.Debug(string.Format("Future refund added: [{0}]", energy_value));
            this.result.AddFutureRefund(energy_value);
        }

        public DataWord GetCallEnergy(OpCode op, DataWord requested_energy, DataWord available_energy)
        {
            return requested_energy.CompareTo(available_energy) > 0 ? available_energy : requested_energy;
        }

        public DataWord GetDataValue(DataWord index)
        {
            return this.invoke.GetDataValue(index);
        }

        public byte[] GetDataCopy(DataWord offset, DataWord length)
        {
            return this.invoke.GetDataCopy(offset, length);
        }

        public DataWord GetDataSize()
        {
            return new DataWord(this.invoke.DataSize.Clone());
        }

        public DataWord GetReturnDataBufferSize()
        {
            return new DataWord(this.return_data == null ? 0 : return_data.Length);
        }

        public byte[] GetReturnDataBufferData(DataWord offset, DataWord size)
        {
            if ((long)offset.ToIntSafety() + size.ToIntSafety() > GetReturnDataBufferSize().ToIntSafety())
                return null;

            return this.return_data == null ?
                new byte[0] : ByteUtil.CopyRange(this.return_data, offset.ToIntSafety(), offset.ToIntSafety() + size.ToIntSafety());
        }

        public byte[] GetCodeAt(DataWord address)
        {
            byte[] code = this.invoke.Deposit.GetCode(Wallet.ToMineralAddress(address.GetLast20Bytes()));
            return code ?? new byte[0];
        }

        public byte[] GetCodeHashAt(DataWord address)
        {
            byte[] mineral_address = Wallet.ToMineralAddress(address.GetLast20Bytes());
            AccountCapsule account = this.contract_state.GetAccount(mineral_address);
            if (account != null)
            {
                byte[] code_hash = null;
                ContractCapsule contract = this.contract_state.GetContract(mineral_address);

                if (contract != null)
                {
                    code_hash = contract.CodeHash;
                    if (code_hash.IsNullOrEmpty())
                    {
                        byte[] code = GetCodeAt(address);
                        code_hash = Hash.SHA3(code);
                        contract.CodeHash = code_hash;
                        this.contract_state.UpdateContract(mineral_address, contract);
                    }
                }
                else
                {
                    code_hash = Hash.SHA3(new byte[0]);
                }
                return code_hash;
            }
            else
            {
                return new byte[0];
            }
        }

        public DataWord GetBlockHash(int index)
        {
            if (index < this.invoke.Number.ToLong() && index >= Math.Max(256, this.invoke.Number.ToLong()) - 256)
            {
                BlockCapsule block = this.invoke.GetBlockByNum(index);
                if (block != null)
                    return new DataWord(block.Id.Hash);
                else
                    return new DataWord(DataWord.ZERO.Clone());
            }
            else
            {
                return new DataWord(DataWord.ZERO.Clone());
            }

        }
        #endregion
    }
}