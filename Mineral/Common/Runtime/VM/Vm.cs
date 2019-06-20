using System;
using System.Collections.Generic;
using System.Numerics;
using Mineral.Common.Runtime.VM.Exception;
using Mineral.Common.Runtime.VM.Program;
using Mineral.Core;
using Mineral.Core.Exception;
using Mineral.Cryptography;

namespace Mineral.Common.Runtime.VM
{
    using VMConfig = Runtime.Config.VMConfig;
    using VMProgram = Runtime.VM.Program.Program;

    public class VM
    {
        #region Field
        private static readonly BigInteger _32_ = new BigInteger(32);
        private static readonly String ENERGY_LOG_FORMATE = "{} Op:[{}]  Energy:[{}] Deep:[{}] Hint:[{}]";
        private static readonly BigInteger MEM_LIMIT = new BigInteger(3L * 1024 * 1024);
        public static readonly String ADDRESS_LOG = "address: ";
        #endregion


        #region Property
        #endregion


        #region Contructor
        public VM() { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private void CheckMemorySize(OpCode op, BigInteger mem_size)
        {
            if (mem_size.CompareTo(MEM_LIMIT) > 0)
            {
                throw new MemoryOverflowException(op);
            }
        }

        private long CalcMemoryEnergy(long old_mem_size, BigInteger new_mem_size, long copy_size, OpCode op)
        {
            long energy_cost = 0;

            CheckMemorySize(op, new_mem_size);

            long memory_usage = ((long)new_mem_size + 31) / 32 * 32;
            if (memory_usage > old_mem_size)
            {
                long mem_words = (memory_usage / 32);
                long mem_words_old = (old_mem_size / 32);
                long mem_energy = (EnergyCost.MEMORY * mem_words + mem_words * mem_words / 512)
                    - (EnergyCost.MEMORY * mem_words_old + mem_words_old * mem_words_old / 512);
                energy_cost += mem_energy;
            }

            if (copy_size > 0)
            {
                long copy_energy = EnergyCost.COPY_ENERGY * ((copy_size + 31) / 32);
                energy_cost += copy_energy;
            }

            return energy_cost;
        }

        private bool IsDeadAccount(VMProgram program, DataWord address)
        {
            return program.ContractState.GetAccount(Wallet.ToMineralAddress(address.GetLast20Bytes())) == null;
        }

        private static BigInteger MemoryNeeded(DataWord offset, DataWord size)
        {
            return size.IsZero ? BigInteger.Zero : BigInteger.Add(offset.ToBigInteger(), size.ToBigInteger());
        }
        #endregion


        #region External Method
        public void Play(VMProgram program)
        {
            try
            {
                if (program.ByTestingSuite())
                    return;

                while (!program.IsStopped)
                {
                    this.Step(program);
                }

            }
            catch (VMStackOverFlowException e)
            {
                throw e;
            }
            catch (OutOfTimeException e)
            {
                throw e;
            }
            catch (StackOverflowException)
            {
                Logger.Info("\n !!! StackOverflowError: update your java run command with -Xss !!!\n");
                throw new VMStackOverFlowException();
            }
            catch (System.Exception e)
            {
                if (e.Message.Length > 0)
                {
                    program.Result.Exception = e;
                }
                else
                {
                    Logger.Warning("Unknown Exception occurred, tx id: " + program.RootTransactionId.ToHexString());
                    program.Result.Exception = new System.Exception("Unknown Exception");
                }
            }
        }

        public void Step(VMProgram program)
        {
            if (VMConfig.Instance.IsVmTrace)
                program.SaveOpTrace();

            try
            {
                if (Enum.IsDefined(typeof(OpCode), program.CurrentOp))
                    throw VMExceptions.InvalidOpCode(program.CurrentOp);

                OpCode op = (OpCode)program.CurrentOp;

                if (!VMConfig.AllowTvmTransferTrc10)
                {
                    if (op == OpCode.CALLTOKEN || op == OpCode.TOKENBALANCE || op == OpCode.CALLTOKENVALUE || op == OpCode.CALLTOKENID)
                    {
                        throw VMExceptions.InvalidOpCode(program.CurrentOp);
                    }
                }

                if (!VMConfig.AllowTvmConstantinople)
                {
                    if (op == OpCode.SHL || op == OpCode.SHR || op == OpCode.SAR || op == OpCode.CREATE2 || op == OpCode.EXTCODEHASH)
                    {
                        throw VMExceptions.InvalidOpCode(program.CurrentOp);
                    }
                }

                program.LastOp = (byte)op;
                program.VerifyStackSize(OpCodeUtil.ToRequire(op));
                program.VerifyStackOverflow(OpCodeUtil.ToRequire(op), OpCodeUtil.ToResult(op));

                long old_mem_size = program.MemorySize;

                string hint = "";
                long energy_cost = (int)OpCodeUtil.ToTier(op);
                DataWord adjusted_call_energy = null;

                DataWordStack stack = program.Stack;

                switch (op)
                {
                    case OpCode.STOP:
                        {
                            energy_cost = EnergyCost.STOP;
                        }
                        break;
                    case OpCode.SUICIDE:
                        {
                            energy_cost = EnergyCost.SUICIDE;
                            DataWord suicide_address = stack.Get(stack.Size - 1);

                            if (IsDeadAccount(program, suicide_address) && !program.GetBalance(program.ContractAddress).IsZero)
                            {
                                energy_cost += EnergyCost.NEW_ACCT_SUICIDE;
                            }
                        }
                        break;
                    case OpCode.SSTORE:
                        {
                            DataWord new_value = stack.Get(stack.Size - 2);
                            DataWord old_value = program.StorageLoad(stack.Peek());
                            if (old_value == null && !new_value.IsZero)
                            {
                                energy_cost = EnergyCost.SET_SSTORE;
                            }
                            else if (old_value != null && new_value.IsZero)
                            {
                                program.FutureRefundEnergy(EnergyCost.REFUND_SSTORE);
                                energy_cost = EnergyCost.CLEAR_SSTORE;
                            }
                            else
                            {
                                energy_cost = EnergyCost.RESET_SSTORE;
                            }
                        }
                        break;
                    case OpCode.SLOAD:
                        {
                            energy_cost = EnergyCost.SLOAD;
                        }
                        break;
                    case OpCode.TOKENBALANCE:
                    case OpCode.BALANCE:
                        {
                            energy_cost = EnergyCost.BALANCE;
                        }
                        break;
                    case OpCode.MSTORE:
                        {
                            energy_cost = CalcMemoryEnergy(old_mem_size,
                                                           MemoryNeeded(stack.Peek(), new DataWord(32)),
                                                           0,
                                                           op);
                        }
                        break;
                    case OpCode.MSTORE8:
                        {
                            energy_cost = CalcMemoryEnergy(old_mem_size,
                                                           MemoryNeeded(stack.Peek(), new DataWord(1)),
                                                           0,
                                                           op);
                        }
                        break;
                    case OpCode.MLOAD:
                        {
                            energy_cost = CalcMemoryEnergy(old_mem_size,
                                                           MemoryNeeded(stack.Peek(), new DataWord(32)),
                                                           0,
                                                           op);

                        }
                        break;
                    case OpCode.RETURN:
                    case OpCode.REVERT:
                        {
                            energy_cost = EnergyCost.STOP + CalcMemoryEnergy(old_mem_size,
                                                                             MemoryNeeded(stack.Peek(), stack.Get(stack.Size - 2)),
                                                                             0,
                                                                             op);
                        }
                        break;
                    case OpCode.SHA3:
                        {
                            energy_cost = EnergyCost.SHA3 + CalcMemoryEnergy(old_mem_size,
                                                                             MemoryNeeded(stack.Peek(), stack.Get(stack.Size - 2)),
                                                                             0,
                                                                             op);
                            DataWord size = stack.Get(stack.Size - 2);
                            long chunk_used = (size.ToLongSafety() + 31) / 32;
                            energy_cost += chunk_used * EnergyCost.SHA3_WORD;
                        }
                        break;
                    case OpCode.CALLDATACOPY:
                    case OpCode.RETURNDATACOPY:
                        {
                            energy_cost = CalcMemoryEnergy(old_mem_size,
                                                           MemoryNeeded(stack.Peek(), stack.Get(stack.Size - 3)),
                                                           stack.Get(stack.Size - 3).ToLongSafety(),
                                                           op);
                        }
                        break;
                    case OpCode.CODECOPY:
                        {
                            energy_cost = CalcMemoryEnergy(old_mem_size,
                                                           MemoryNeeded(stack.Peek(), stack.Get(stack.Size - 3)),
                                                           stack.Get(stack.Size - 3).ToLongSafety(),
                                                           op);
                        }
                        break;
                    case OpCode.EXTCODESIZE:
                        {
                            energy_cost = EnergyCost.EXT_CODE_SIZE;
                        }
                        break;
                    case OpCode.EXTCODECOPY:
                        {
                            energy_cost = EnergyCost.EXT_CODE_COPY + CalcMemoryEnergy(old_mem_size,
                                                                                      MemoryNeeded(stack.Get(stack.Size - 2),
                                                                                      stack.Get(stack.Size - 4)),
                                                                                      stack.Get(stack.Size - 4).ToLongSafety(),
                                                                                      op);
                        }
                        break;
                    case OpCode.EXTCODEHASH:
                        {
                            energy_cost = EnergyCost.EXT_CODE_HASH;
                        }
                        break;
                    case OpCode.CALL:
                    case OpCode.CALLCODE:
                    case OpCode.DELEGATECALL:
                    case OpCode.STATICCALL:
                    case OpCode.CALLTOKEN:
                        {
                            energy_cost = EnergyCost.CALL;
                            DataWord call_energy = stack.Get(stack.Size - 1);
                            DataWord call_address = stack.Get(stack.Size - 2);
                            DataWord value = OpCodeUtil.ContainHasValue(op) ? stack.Get(stack.Size - 3) : DataWord.ZERO;

                            if (op == OpCode.CALL || op == OpCode.CALLTOKEN)
                            {
                                if (IsDeadAccount(program, call_address) && !value.IsZero)
                                {
                                    energy_cost += EnergyCost.NEW_ACCT_CALL;
                                }
                            }

                            if (!value.IsZero)
                            {
                                energy_cost += EnergyCost.VT_CALL;
                            }

                            int opOff = OpCodeUtil.ContainHasValue(op) ? 4 : 3;
                            if (op == OpCode.CALLTOKEN)
                                opOff++;

                            BigInteger in_size = MemoryNeeded(stack.Get(stack.Size - opOff), stack.Get(stack.Size - opOff - 1));
                            BigInteger out_size = MemoryNeeded(stack.Get(stack.Size - opOff - 2), stack.Get(stack.Size - opOff - 3));

                            energy_cost += CalcMemoryEnergy(old_mem_size,
                                                            BigInteger.Max(in_size, out_size),
                                                            0,
                                                            op);

                            CheckMemorySize(op, BigInteger.Max(in_size, out_size));

                            if (energy_cost > program.EnergyLimitLeft.ToLongSafety())
                            {
                                throw new OutOfEnergyException(
                                    string.Format(
                                            "Not enough energy for '{0}' operation executing: opEnergy[{1}], programEnergy[{2}]",
                                            op.ToString(),
                                            energy_cost,
                                            program.EnergyLimitLeft.ToLongSafety()));
                            }

                            DataWord energy_limit_left = new DataWord(program.EnergyLimitLeft.Clone());
                            energy_limit_left.Sub(new DataWord(energy_cost));

                            adjusted_call_energy = program.GetCallEnergy(op, call_energy, energy_limit_left);
                            energy_cost += adjusted_call_energy.ToLongSafety();
                        }
                        break;
                    case OpCode.CREATE:
                        {
                            energy_cost = EnergyCost.CREATE + CalcMemoryEnergy(old_mem_size,
                                                                           MemoryNeeded(stack.Get(stack.Size - 2), stack.Get(stack.Size - 3)),
                                                                           0,
                                                                           op);
                        }
                        break;
                    case OpCode.CREATE2:
                        {
                            DataWord code_size = stack.Get(stack.Size - 3);
                            energy_cost = EnergyCost.CREATE;
                            energy_cost += CalcMemoryEnergy(old_mem_size,
                                                            MemoryNeeded(stack.Get(stack.Size - 2),
                                                            stack.Get(stack.Size - 3)),
                                                            0,
                                                            op);

                            energy_cost += DataWord.SizeInWords(code_size.ToIntSafety()) * EnergyCost.SHA3_WORD;
                        }
                        break;
                    case OpCode.LOG0:
                    case OpCode.LOG1:
                    case OpCode.LOG2:
                    case OpCode.LOG3:
                    case OpCode.LOG4:
                        {
                            int topics = op - OpCode.LOG0;
                            BigInteger data_size = stack.Get(stack.Size - 2).ToBigInteger();
                            BigInteger data_cost = BigInteger.Multiply(data_size, new BigInteger(EnergyCost.LOG_DATA_ENERGY));
                            if (program.EnergyLimitLeft.ToBigInteger().CompareTo(data_cost) < 0)
                            {
                                throw new OutOfEnergyException(
                                    string.Format(
                                        "Not enough energy for '%s' operation executing: opEnergy[%d], programEnergy[%d]",
                                        op.ToString(),
                                        (long)data_cost,
                                        program.EnergyLimitLeft.
                                        ToLongSafety()));
                            }

                            energy_cost = EnergyCost.LOG_ENERGY
                                + EnergyCost.LOG_TOPIC_ENERGY * topics
                                + EnergyCost.LOG_DATA_ENERGY * stack.Get(stack.Size - 2).ToLong()
                                + CalcMemoryEnergy(old_mem_size,
                                                   MemoryNeeded(stack.Peek(), stack.Get(stack.Size - 2)),
                                                   0,
                                                   op);

                            CheckMemorySize(op, MemoryNeeded(stack.Peek(), stack.Get(stack.Size - 2)));
                        }
                        break;
                    case OpCode.EXP:
                        {
                            DataWord exp = stack.Get(stack.Size - 2);
                            int bytes_occupied = exp.BytesOccupied();
                            energy_cost = (long)EnergyCost.EXP_ENERGY + EnergyCost.EXP_BYTE_ENERGY * bytes_occupied;
                        }
                        break;
                    default:
                        break;
                }

                program.SpendEnergy(energy_cost, op.ToString());
                program.CheckCPUTimeLimit(op.ToString());

                switch (op)
                {
                    case OpCode.STOP:
                        {
                            program.SetHReturn(new byte[0]);
                            program.Stop();
                        }
                        break;
                    case OpCode.ADD:
                        {
                            DataWord word1 = program.StackPop();
                            DataWord word2 = program.StackPop();

                            hint = word1.ToBigInteger() + " + " + word2.ToBigInteger();
                            word1.Add(word2);
                            program.StackPush(word1);
                            program.Step();

                        }
                        break;
                    case OpCode.MUL:
                        {
                            DataWord word1 = program.StackPop();
                            DataWord word2 = program.StackPop();

                            hint = word1.ToBigInteger() + " * " + word2.ToBigInteger();
                            word1.Multiply(word2);
                            program.StackPush(word1);
                            program.Step();
                        }
                        break;
                    case OpCode.SUB:
                        {
                            DataWord word1 = program.StackPop();
                            DataWord word2 = program.StackPop();

                            hint = word1.ToBigInteger() + " - " + word2.ToBigInteger();
                            word1.Sub(word2);
                            program.StackPush(word1);
                            program.Step();
                        }
                        break;
                    case OpCode.DIV:
                        {
                            DataWord word1 = program.StackPop();
                            DataWord word2 = program.StackPop();

                            hint = word1.ToBigInteger() + " / " + word2.ToBigInteger();
                            word1.Divide(word2);
                            program.StackPush(word1);
                            program.Step();
                        }
                        break;
                    case OpCode.SDIV:
                        {
                            DataWord word1 = program.StackPop();
                            DataWord word2 = program.StackPop();

                            hint = word1.ToBigInteger() + " / " + word2.ToBigInteger();
                            word1.Divide(word2);
                            program.StackPush(word1);
                            program.Step();
                        }
                        break;
                    case OpCode.MOD:
                        {
                            DataWord word1 = program.StackPop();
                            DataWord word2 = program.StackPop();

                            hint = word1.ToBigInteger() + " % " + word2.ToBigInteger();
                            word1.Mod(word2);
                            program.StackPush(word1);
                            program.Step();
                        }
                        break;
                    case OpCode.SMOD:
                        {
                            DataWord word1 = program.StackPop();
                            DataWord word2 = program.StackPop();

                            hint = word1.ToBigInteger() + " #% " + word2.ToBigInteger();
                            word1.Mod(word2);
                            program.StackPush(word1);
                            program.Step();
                        }
                        break;
                    case OpCode.EXP:
                        {
                            DataWord word1 = program.StackPop();
                            DataWord word2 = program.StackPop();

                            hint = word1.ToBigInteger() + " ** " + word2.ToBigInteger();
                            word1.ModPow(word2);
                            program.StackPush(word1);
                            program.Step();
                        }
                        break;
                    case OpCode.SIGNEXTEND:
                        {
                            DataWord word1 = program.StackPop();
                            BigInteger k = word1.ToBigInteger();

                            if (k.CompareTo(_32_) < 0)
                            {
                                DataWord word2 = program.StackPop();

                                hint = word1 + "  " + word2.ToBigInteger();
                                word2.SignExtend((byte)((int)k));
                                program.StackPush(word2);
                            }
                            program.Step();
                        }
                        break;
                    case OpCode.NOT:
                        {
                            DataWord word1 = program.StackPop();
                            word1.BNot();

                            hint = "" + word1.ToBigInteger();
                            program.StackPush(word1);
                            program.Step();
                        }
                        break;
                    case OpCode.LT:
                        {
                            // TODO: can be improved by not using BigInteger
                            DataWord word1 = program.StackPop();
                            DataWord word2 = program.StackPop();

                            hint = word1.ToBigInteger() + " < " + word2.ToBigInteger();
                            if (word1.ToBigInteger().CompareTo(word2.ToBigInteger()) < 0)
                            {
                                word1.AND(DataWord.ZERO);
                                word1.Data[31] = 1;
                            }
                            else
                            {
                                word1.AND(DataWord.ZERO);
                            }
                            program.StackPush(word1);
                            program.Step();
                        }
                        break;
                    case OpCode.SLT:
                        {
                            // TODO: can be improved by not using BigInteger
                            DataWord word1 = program.StackPop();
                            DataWord word2 = program.StackPop();

                            hint = word1.ToBigInteger() + " < " + word2.ToBigInteger();
                            if (word1.ToBigInteger().CompareTo(word2.ToBigInteger()) < 0)
                            {
                                word1.AND(DataWord.ZERO);
                                word1.Data[31] = 1;
                            }
                            else
                            {
                                word1.AND(DataWord.ZERO);
                            }
                            program.StackPush(word1);
                            program.Step();
                        }
                        break;
                    case OpCode.SGT:
                        {
                            // TODO: can be improved by not using BigInteger
                            DataWord word1 = program.StackPop();
                            DataWord word2 = program.StackPop();

                            hint = word1.ToBigInteger() + " > " + word2.ToBigInteger();
                            if (word1.ToBigInteger().CompareTo(word2.ToBigInteger()) > 0)
                            {
                                word1.AND(DataWord.ZERO);
                                word1.Data[31] = 1;
                            }
                            else
                            {
                                word1.AND(DataWord.ZERO);
                            }
                            program.StackPush(word1);
                            program.Step();
                        }
                        break;
                    case OpCode.GT:
                        {
                            // TODO: can be improved by not using BigInteger
                            DataWord word1 = program.StackPop();
                            DataWord word2 = program.StackPop();

                            hint = word1.ToBigInteger() + " > " + word2.ToBigInteger();
                            if (word1.ToBigInteger().CompareTo(word2.ToBigInteger()) > 0)
                            {
                                word1.AND(DataWord.ZERO);
                                word1.Data[31] = 1;
                            }
                            else
                            {
                                word1.AND(DataWord.ZERO);
                            }
                            program.StackPush(word1);
                            program.Step();
                        }
                        break;
                    case OpCode.EQ:
                        {
                            DataWord word1 = program.StackPop();
                            DataWord word2 = program.StackPop();

                            hint = word1.ToBigInteger() + " == " + word2.ToBigInteger();
                            if (word1.XOR(word2).IsZero)
                            {
                                word1.AND(DataWord.ZERO);
                                word1.Data[31] = 1;
                            }
                            else
                            {
                                word1.AND(DataWord.ZERO);
                            }
                            program.StackPush(word1);
                            program.Step();
                        }
                        break;
                    case OpCode.ISZERO:
                        {
                            DataWord word1 = program.StackPop();
                            if (word1.IsZero)
                            {
                                word1.Data[31] = 1;
                            }
                            else
                            {
                                word1.AND(DataWord.ZERO);
                            }

                            hint = "" + word1.ToBigInteger();
                            program.StackPush(word1);
                            program.Step();
                        }
                        break;

                    /**
                     * Bitwise Logic Operations
                     */
                    case OpCode.AND:
                        {
                            DataWord word1 = program.StackPop();
                            DataWord word2 = program.StackPop();

                            hint = word1.ToBigInteger() + " && " + word2.ToBigInteger();
                            word1.AND(word2);
                            program.StackPush(word1);
                            program.Step();
                        }
                        break;
                    case OpCode.OR:
                        {
                            DataWord word1 = program.StackPop();
                            DataWord word2 = program.StackPop();

                            hint = word1.ToBigInteger() + " || " + word2.ToBigInteger();
                            word1.OR(word2);
                            program.StackPush(word1);
                            program.Step();
                        }
                        break;
                    case OpCode.XOR:
                        {
                            DataWord word1 = program.StackPop();
                            DataWord word2 = program.StackPop();

                            hint = word1.ToBigInteger() + " ^ " + word2.ToBigInteger();
                            word1.XOR(word2);
                            program.StackPush(word1);
                            program.Step();
                        }
                        break;
                    case OpCode.BYTE:
                        {
                            DataWord word1 = program.StackPop();
                            DataWord word2 = program.StackPop();
                            DataWord result = null;
                            if (word1.ToBigInteger().CompareTo(_32_) < 0)
                            {
                                byte tmp = word2.Data[word1.ToInt()];
                                word2.AND(DataWord.ZERO);
                                word2.Data[31] = tmp;
                                result = word2;
                            }
                            else
                            {
                                result = new DataWord();
                            }

                            hint = "" + result.ToBigInteger();
                            program.StackPush(result);
                            program.Step();
                        }
                        break;
                    case OpCode.SHL:
                        {
                            DataWord word1 = program.StackPop();
                            DataWord word2 = program.StackPop();
                            DataWord result = word2.ShiftLeft(word1);

                            hint = "" + result.ToBigInteger();
                            program.StackPush(result);
                            program.Step();
                        }
                        break;
                    case OpCode.SHR:
                        {
                            DataWord word1 = program.StackPop();
                            DataWord word2 = program.StackPop();
                            DataWord result = word2.ShiftRight(word1);

                            hint = "" + result.ToBigInteger();
                            program.StackPush(result);
                            program.Step();
                        }
                        break;
                    case OpCode.SAR:
                        {
                            DataWord word1 = program.StackPop();
                            DataWord word2 = program.StackPop();
                            DataWord result = word2.shiftRightSigned(word1);

                            hint = "" + result.ToBigInteger();
                            program.StackPush(result);
                            program.Step();
                        }
                        break;
                    case OpCode.ADDMOD:
                        {
                            DataWord word1 = program.StackPop();
                            DataWord word2 = program.StackPop();
                            DataWord word3 = program.StackPop();
                            word1.AddMod(word2, word3);
                            program.StackPush(word1);
                            program.Step();
                        }
                        break;
                    case OpCode.MULMOD:
                        {
                            DataWord word1 = program.StackPop();
                            DataWord word2 = program.StackPop();
                            DataWord word3 = program.StackPop();
                            word1.MultipyMod(word2, word3);
                            program.StackPush(word1);
                            program.Step();
                        }
                        break;
                    case OpCode.SHA3:
                        {
                            DataWord mem_offset_data = program.StackPop();
                            DataWord length_data = program.StackPop();
                            byte[] buffer = program.MemoryChunk(mem_offset_data.ToIntSafety(), length_data.ToIntSafety());

                            byte[] encoded = Hash.SHA3(buffer);
                            DataWord word = new DataWord(encoded);

                            hint = word.ToString();
                            program.StackPush(word);
                            program.Step();
                        }
                        break;
                    case OpCode.ADDRESS:
                        {
                            DataWord address = program.ContractAddress;
                            if (VMConfig.AllowMultiSign)
                            {
                                address = new DataWord(address.GetLast20Bytes());
                            }

                            hint = ADDRESS_LOG + address.GetLast20Bytes().ToHexString();
                            program.StackPush(address);
                            program.Step();
                        }
                        break;
                    case OpCode.BALANCE:
                        {
                            DataWord address = program.StackPop();
                            DataWord balance = program.GetBalance(address);

                            hint = ADDRESS_LOG + address.GetLast20Bytes().ToHexString() + " balance: " + balance.ToString();
                            program.StackPush(balance);
                            program.Step();
                        }
                        break;
                    case OpCode.ORIGIN:
                        {
                            DataWord origin_address = program.OriginAddress;
                            if (VMConfig.AllowMultiSign)
                            {
                                origin_address = new DataWord(origin_address.GetLast20Bytes());
                            }

                            hint = ADDRESS_LOG + origin_address.GetLast20Bytes().ToHexString();
                            program.StackPush(origin_address);
                            program.Step();
                        }
                        break;
                    case OpCode.CALLER:
                        {
                            DataWord caller_address = program.CallerAddress;
                            caller_address = new DataWord(caller_address.GetLast20Bytes());

                            hint = ADDRESS_LOG + caller_address.GetLast20Bytes().ToHexString();
                            program.StackPush(caller_address);
                            program.Step();
                        }
                        break;
                    case OpCode.CALLVALUE:
                        {
                            DataWord call_value = program.CallValue;

                            hint = "call_value: " + call_value;
                            program.StackPush(call_value);
                            program.Step();
                        }
                        break;
                    case OpCode.CALLTOKENVALUE:
                        {
                            DataWord token_value = program.TokenValue;

                            hint = "token_value : " + token_value;
                            program.StackPush(token_value);
                            program.Step();
                        }
                        break;
                    case OpCode.CALLTOKENID:
                        {
                            DataWord token_id = program.TokenId;

                            hint = "token_id : " + token_id;
                            program.StackPush(token_id);
                            program.Step();
                        }
                        break;
                    case OpCode.CALLDATALOAD:
                        {
                            DataWord data_offset = program.StackPop();
                            DataWord value = program.GetDataValue(data_offset);

                            hint = "data : " + value;
                            program.StackPush(value);
                            program.Step();
                        }
                        break;
                    case OpCode.CALLDATASIZE:
                        {
                            DataWord data_size = program.GetDataSize();

                            hint = "size: " + data_size.ToBigInteger();
                            program.StackPush(data_size);
                            program.Step();
                        }
                        break;
                    case OpCode.CALLDATACOPY:
                        {
                            DataWord mem_offset_data = program.StackPop();
                            DataWord data_offset_data = program.StackPop();
                            DataWord length_data = program.StackPop();

                            byte[] msg_data = program.GetDataCopy(data_offset_data, length_data);

                            hint = "data: " + msg_data.ToHexString();
                            program.MemorySave(mem_offset_data.ToIntSafety(), msg_data);
                            program.Step();
                        }
                        break;
                    case OpCode.RETURNDATASIZE:
                        {
                            DataWord dataSize = program.GetReturnDataBufferSize();

                            hint = "size: " + dataSize.ToBigInteger();
                            program.StackPush(dataSize);
                            program.Step();
                        }
                        break;
                    case OpCode.RETURNDATACOPY:
                        {
                            DataWord mem_offset_data = program.StackPop();
                            DataWord data_offset_data = program.StackPop();
                            DataWord length_data = program.StackPop();

                            byte[] msg_data = program.GetReturnDataBufferData(data_offset_data, length_data);

                            if (msg_data == null)
                            {
                                throw new ReturnDataCopyIllegalBoundsException(data_offset_data, length_data,
                                    program.GetReturnDataBufferSize().ToLongSafety());
                            }

                            hint = "data: " + msg_data.ToHexString();
                            program.MemorySave(mem_offset_data.ToIntSafety(), msg_data);
                            program.Step();
                        }
                        break;
                    case OpCode.CODESIZE:
                    case OpCode.EXTCODESIZE:
                        {

                            int length;
                            if (op == OpCode.CODESIZE)
                            {
                                length = program.Code.Length;
                            }
                            else
                            {
                                DataWord address = program.StackPop();
                                length = program.GetCodeAt(address).Length;
                            }
                            DataWord codeLength = new DataWord(length);

                            hint = "size: " + length;
                            program.StackPush(codeLength);
                            program.Step();
                            break;
                        }
                    case OpCode.CODECOPY:
                    case OpCode.EXTCODECOPY:
                        {
                            byte[] full_code = new byte[0];
                            if (op == OpCode.CODECOPY)
                            {
                                full_code = program.Code;
                            }

                            if (op == OpCode.EXTCODECOPY)
                            {
                                DataWord address = program.StackPop();
                                full_code = program.GetCodeAt(address);
                            }

                            int mem_offset = program.StackPop().ToIntSafety();
                            int code_offset = program.StackPop().ToIntSafety();
                            int length_data = program.StackPop().ToIntSafety();

                            int size_copied = ((long)code_offset + length_data) > full_code.Length ?
                                                    (full_code.Length < code_offset ? 0 : full_code.Length - code_offset) : length_data;

                            byte[] code_copy = new byte[length_data];

                            if (code_offset < full_code.Length)
                            {
                                Array.Copy(full_code, code_offset, code_copy, 0, size_copied);
                            }

                            hint = "code: " + code_copy.ToHexString();
                            program.MemorySave(mem_offset, code_copy);
                            program.Step();
                            break;
                        }
                    case OpCode.EXTCODEHASH:
                        {
                            DataWord address = program.StackPop();
                            byte[] code_hash = program.GetCodeHashAt(address);

                            program.StackPush(code_hash);
                            program.Step();
                        }
                        break;
                    case OpCode.GASPRICE:
                        {
                            DataWord energy_price = new DataWord(0);

                            hint = "price: " + energy_price.ToString();
                            program.StackPush(energy_price);
                            program.Step();
                        }
                        break;
                    case OpCode.BLOCKHASH:
                        {
                            int block_index = program.StackPop().ToIntSafety();
                            DataWord block_hash = program.GetBlockHash(block_index);

                            hint = "blockHash: " + block_hash;
                            program.StackPush(block_hash);
                            program.Step();
                        }
                        break;
                    case OpCode.COINBASE:
                        {
                            DataWord coinbase = program.CoinBase;

                            hint = "coinbase: " + coinbase.GetLast20Bytes().ToHexString();
                            program.StackPush(coinbase);
                            program.Step();
                        }
                        break;
                    case OpCode.TIMESTAMP:
                        {
                            DataWord timestamp = program.Timestamp;

                            hint = "timestamp: " + timestamp.ToBigInteger();
                            program.StackPush(timestamp);
                            program.Step();
                        }
                        break;
                    case OpCode.NUMBER:
                        {
                            DataWord number = program.Number;

                            hint = "number: " + number.ToBigInteger();
                            program.StackPush(number);
                            program.Step();
                        }
                        break;
                    case OpCode.DIFFICULTY:
                        {
                            DataWord difficulty = program.Difficulty;

                            hint = "difficulty: " + difficulty;
                            program.StackPush(difficulty);
                            program.Step();
                        }
                        break;
                    case OpCode.GASLIMIT:
                        {
                            DataWord energy_limit = new DataWord(0);

                            hint = "energylimit: " + energy_limit;
                            program.StackPush(energy_limit);
                            program.Step();
                        }
                        break;
                    case OpCode.POP:
                        {
                            program.StackPop();
                            program.Step();
                        }
                        break;
                    case OpCode.DUP1:
                    case OpCode.DUP2:
                    case OpCode.DUP3:
                    case OpCode.DUP4:
                    case OpCode.DUP5:
                    case OpCode.DUP6:
                    case OpCode.DUP7:
                    case OpCode.DUP8:
                    case OpCode.DUP9:
                    case OpCode.DUP10:
                    case OpCode.DUP11:
                    case OpCode.DUP12:
                    case OpCode.DUP13:
                    case OpCode.DUP14:
                    case OpCode.DUP15:
                    case OpCode.DUP16:
                        {
                            int n = op - OpCode.DUP1 + 1;
                            DataWord word_1 = stack.Get(stack.Size - n);
                            program.StackPush(word_1.Clone());
                            program.Step();
                        }
                        break;
                    case OpCode.SWAP1:
                    case OpCode.SWAP2:
                    case OpCode.SWAP3:
                    case OpCode.SWAP4:
                    case OpCode.SWAP5:
                    case OpCode.SWAP6:
                    case OpCode.SWAP7:
                    case OpCode.SWAP8:
                    case OpCode.SWAP9:
                    case OpCode.SWAP10:
                    case OpCode.SWAP11:
                    case OpCode.SWAP12:
                    case OpCode.SWAP13:
                    case OpCode.SWAP14:
                    case OpCode.SWAP15:
                    case OpCode.SWAP16:
                        {
                            int n = op - OpCode.SWAP1 + 2;
                            stack.Swap(stack.Size - 1, stack.Size - n);
                            program.Step();
                        }
                        break;
                    case OpCode.LOG0:
                    case OpCode.LOG1:
                    case OpCode.LOG2:
                    case OpCode.LOG3:
                    case OpCode.LOG4:
                        {
                            if (program.IsStaticCall)
                                throw new StaticCallModificationException();

                            DataWord address = program.ContractAddress;
                            DataWord mem_start = stack.Pop();
                            DataWord mem_offset = stack.Pop();

                            int topic_count = op - OpCode.LOG0;
                            List<DataWord> topics = new List<DataWord>();

                            for (int i = 0; i < topic_count; ++i)
                            {
                                DataWord topic = stack.Pop();
                                topics.Add(topic);
                            }

                            byte[] data = program.MemoryChunk(mem_start.ToIntSafety(), mem_offset.ToIntSafety());
                            LogInfo log_info = new LogInfo(address.GetLast20Bytes(), topics, data);

                            hint = log_info.ToString();
                            program.Result.AddLogInfo(log_info);
                            program.Step();
                        }
                        break;
                    case OpCode.MLOAD:
                        {
                            DataWord address = program.StackPop();
                            DataWord data = program.MemoryLoad(address);

                            hint = "data: " + data;
                            program.StackPush(data);
                            program.Step();
                        }
                        break;
                    case OpCode.MSTORE:
                        {
                            DataWord address = program.StackPop();
                            DataWord value = program.StackPop();

                            hint = "addr: " + address + " value: " + value;
                            program.MemorySave(address, value);
                            program.Step();
                        }
                        break;
                    case OpCode.MSTORE8:
                        {
                            DataWord address = program.StackPop();
                            DataWord value = program.StackPop();
                            byte[] byte_value = { value.Data[31] };
                            program.MemorySave(address.ToIntSafety(), byte_value);
                            program.Step();
                        }
                        break;
                    case OpCode.SLOAD:
                        {
                            DataWord key = program.StackPop();
                            DataWord val = program.StorageLoad(key);

                            hint = "key: " + key + " value: " + val;
                            if (val == null)
                                val = key.AND(DataWord.ZERO);

                            program.StackPush(val);
                            program.Step();
                        }
                        break;
                    case OpCode.SSTORE:
                        {
                            if (program.IsStaticCall)
                                throw new StaticCallModificationException();

                            DataWord address = program.StackPop();
                            DataWord value = program.StackPop();

                            hint = "[" + program.ContractAddress.ToPrefixString() + "] key: " + address + " value: " + value;
                            program.StorageSave(address, value);
                            program.Step();
                        }
                        break;
                    case OpCode.JUMP:
                        {
                            DataWord pos = program.StackPop();
                            int next_pc = program.VerifyJumpDest(pos);

                            hint = "~> " + next_pc;
                            program.PC = next_pc;

                        }
                        break;
                    case OpCode.JUMPI:
                        {
                            DataWord pos = program.StackPop();
                            DataWord cond = program.StackPop();

                            if (!cond.IsZero)
                            {
                                int next_pc = program.VerifyJumpDest(pos);

                                hint = "~> " + next_pc;
                                program.PC = next_pc;
                            }
                            else
                            {
                                program.Step();
                            }
                        }
                        break;
                    case OpCode.PC:
                        {
                            int pc = program.PC;
                            DataWord pc_word = new DataWord(pc);

                            hint = pc_word.ToString();
                            program.StackPush(pc_word);
                            program.Step();
                        }
                        break;
                    case OpCode.MSIZE:
                        {
                            DataWord mem_size = new DataWord(program.Memory.Size);

                            hint = "" + program.Memory.Size;
                            program.StackPush(mem_size);
                            program.Step();
                        }
                        break;
                    case OpCode.GAS:
                        {
                            DataWord energy = program.EnergyLimitLeft;

                            hint = "" + energy;
                            program.StackPush(energy);
                            program.Step();
                        }
                        break;
                    case OpCode.PUSH1:
                    case OpCode.PUSH2:
                    case OpCode.PUSH3:
                    case OpCode.PUSH4:
                    case OpCode.PUSH5:
                    case OpCode.PUSH6:
                    case OpCode.PUSH7:
                    case OpCode.PUSH8:
                    case OpCode.PUSH9:
                    case OpCode.PUSH10:
                    case OpCode.PUSH11:
                    case OpCode.PUSH12:
                    case OpCode.PUSH13:
                    case OpCode.PUSH14:
                    case OpCode.PUSH15:
                    case OpCode.PUSH16:
                    case OpCode.PUSH17:
                    case OpCode.PUSH18:
                    case OpCode.PUSH19:
                    case OpCode.PUSH20:
                    case OpCode.PUSH21:
                    case OpCode.PUSH22:
                    case OpCode.PUSH23:
                    case OpCode.PUSH24:
                    case OpCode.PUSH25:
                    case OpCode.PUSH26:
                    case OpCode.PUSH27:
                    case OpCode.PUSH28:
                    case OpCode.PUSH29:
                    case OpCode.PUSH30:
                    case OpCode.PUSH31:
                    case OpCode.PUSH32:
                        {
                            program.Step();
                            int push = op - OpCode.PUSH1 + 1;
                            byte[] data = program.Sweep(push);

                            hint = "" + data.ToHexString();
                            program.StackPush(data);
                        }
                        break;
                    case OpCode.JUMPDEST:
                        {
                            program.Step();
                        }
                        break;
                    case OpCode.CREATE:
                        {
                            if (program.IsStaticCall)
                                throw new StaticCallModificationException();

                            DataWord value = program.StackPop();
                            DataWord in_offset = program.StackPop();
                            DataWord in_size = program.StackPop();

                            program.CreateContract(value, in_offset, in_size);
                            program.Step();
                        }
                        break;
                    case OpCode.CREATE2:
                        {
                            if (program.IsStaticCall)
                                throw new StaticCallModificationException();

                            DataWord value = program.StackPop();
                            DataWord in_offset = program.StackPop();
                            DataWord in_size = program.StackPop();
                            DataWord salt = program.StackPop();

                            program.CreateContract2(value, in_offset, in_size, salt);
                            program.Step();
                        }
                        break;
                    case OpCode.TOKENBALANCE:
                        {
                            DataWord token_id = program.StackPop();
                            DataWord address = program.StackPop();
                            DataWord token_balance = program.GetTokenBalance(address, token_id);

                            program.StackPush(token_balance);
                            program.Step();
                        }
                        break;
                    case OpCode.CALL:
                    case OpCode.CALLCODE:
                    case OpCode.CALLTOKEN:
                    case OpCode.DELEGATECALL:
                    case OpCode.STATICCALL:
                        {
                            program.StackPop();

                            DataWord token_id = new DataWord(0);
                            DataWord code_address = program.StackPop();
                            DataWord value = OpCodeUtil.ContainHasValue(op) ? program.StackPop() : DataWord.ZERO;

                            if (program.IsStaticCall && (op == OpCode.CALL || op == OpCode.CALLTOKEN) && !value.IsZero)
                                throw new StaticCallModificationException();

                            if (!value.IsZero)
                            {
                                adjusted_call_energy.Add(new DataWord(EnergyCost.STIPEND_CALL));
                            }

                            bool is_token_transfer = false;
                            if (op == OpCode.CALLTOKEN)
                            {
                                token_id = program.StackPop();
                                if (VMConfig.AllowMultiSign)
                                {
                                    is_token_transfer = true;
                                }
                            }

                            DataWord in_data_offset = program.StackPop();
                            DataWord in_data_size = program.StackPop();
                            DataWord out_data_offset = program.StackPop();
                            DataWord out_data_size = program.StackPop();

                            hint = "address: "
                                    + code_address.GetLast20Bytes()
                                    + " energy: " + adjusted_call_energy.ToShortHex()
                                    + " in_offset: " + in_data_offset.ToShortHex()
                                    + " in_size: " + in_data_size.ToShortHex();

                            Logger.Debug(
                                string.Format(
                                    ENERGY_LOG_FORMATE,
                                    string.Format("{0}", "[" + program.PC + "]"),
                                    string.Format("{0}", op.ToString()),
                                    program.EnergyLimitLeft.ToBigInteger(), program.CallDeep, hint));

                            program.MemoryExpand(out_data_offset, out_data_size);

                            MessageCall msg = new MessageCall(op,
                                                              adjusted_call_energy,
                                                              code_address,
                                                              value,
                                                              in_data_offset,
                                                              in_data_size,
                                                              out_data_offset,
                                                              out_data_size,
                                                              token_id,
                                                              is_token_transfer);

                            PrecompiledContracts.PrecompiledContract contract =
                                PrecompiledContracts.getContractForAddress(code_address);

                            if (!OpCodeUtil.ContainStateless(op))
                            {
                                program.Result.AddTouchAccount(code_address.GetLast20Bytes());
                            }

                            if (contract != null)
                            {
                                program.CallToPrecompiledAddress(msg, contract);
                            }
                            else
                            {
                                program.CallToAddress(msg);
                            }

                            program.Step();
                            break;
                        }
                    case OpCode.RETURN:
                    case OpCode.REVERT:
                        {
                            DataWord offset = program.StackPop();
                            DataWord size = program.StackPop();

                            byte[] h_return = program.MemoryChunk(offset.ToIntSafety(), size.ToIntSafety());
                            program.SetHReturn(h_return);

                            hint = "data: " + h_return.ToHexString()
                                + " offset: " + offset.ToBigInteger()
                                + " size: " + size.ToBigInteger();
                            program.Step();
                            program.Stop();

                            if (op == OpCode.REVERT)
                            {
                                program.Result.IsRevert = true;
                            }
                            break;
                        }
                    case OpCode.SUICIDE:
                        {
                            if (program.IsStaticCall)
                            {
                                throw new StaticCallModificationException();
                            }

                            DataWord address = program.StackPop();
                            program.Suicide(address);
                            program.Result.AddTouchAccount(address.GetLast20Bytes());

                            hint = ADDRESS_LOG + program.ContractAddress.GetLast20Bytes();
                            program.Stop();
                        }
                        break;
                    default:
                        break;
                }

                program.PrevExecutedOp = (byte)op;
            }
            catch (System.Exception e)
            {
                Logger.Info(string.Format("VM halted: [{0}]", e.Message));

                if (!(e is TransferException)) {
                    program.SpendAllEnergy();
                }

                program.ResetFutureRefund();
                program.Stop();
                throw e;
            }
            finally
            {
                program.FullTrace();
            }
        }
        #endregion
    }
}
