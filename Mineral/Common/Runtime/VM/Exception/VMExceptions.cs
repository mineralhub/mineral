
using Org.BouncyCastle.Math;

namespace Mineral.Common.Runtime.VM.Exception
{
    public static class VMExceptions
    {
        public static OutOfEnergyException NotEnoughOpEnergy(OpCode op, long op_energy, long program_energy)
        {
            return new OutOfEnergyException(
                string.Format(
                        "Not enough energy for '{0}' operation executing: op_energy[{1}], program_energy[{2}];",
                        op,
                        op_energy,
                        program_energy));
        }

        public static OutOfEnergyException NotEnoughOpEnergy(OpCode op, DataWord op_energy, DataWord program_energy)
        {
            return NotEnoughOpEnergy(op, op_energy.ToLong(), program_energy.ToLong());
        }

        public static OutOfEnergyException NotEnoughSpendEnergy(string hint, long need_energy, long left_energy)
        {
            return new OutOfEnergyException(
                string.Format(
                        "Not enough energy for '{0}' executing: need_energy[{1}], left_energy[{2}];",
                        hint,
                        need_energy,
                        left_energy));
        }

        public static OutOfTimeException NotEnoughTime(string op)
        {
            return new OutOfTimeException(
                string.Format("CPU timeout for '{0}' operation executing", op));
        }

        public static OutOfTimeException AlreadyTimeOut()
        {
            return new OutOfTimeException("Already Time Out");
        }


        public static OutOfMemoryException MemoryOverflow(OpCode op)
        {
            return new OutOfMemoryException(
                string.Format("Out of Memory when '{0}' operation executing", op.ToString()));
        }

        public static OutOfStorageException NotEnoughStorage()
        {
            return new OutOfStorageException("Not enough ContractState resource");
        }

        public static PrecompiledContractException ContractValidateException(System.Exception e)
        {
            return new PrecompiledContractException(e.Message);
        }

        public static PrecompiledContractException ContractExecuteException(System.Exception e)
        {
            return new PrecompiledContractException(e.Message);
        }

        public static OutOfEnergyException EnergyOverflow(BigInteger actual_energy, BigInteger energy_limit)
        {
            return new OutOfEnergyException(
                string.Format(
                        "Energy value overflow: actual_energy[{0}], energy_limit[{1}]",
                        actual_energy.LongValue,
                        energy_limit.LongValue));
        }

        public static IllegalOperationException InvalidOpCode(byte code)
        {
            return new IllegalOperationException(
                string.Format(
                        "Invalid operation code: op_code[{0}]",
                        Helper.ToHexString(code)));
        }

        public static BadJumpDestinationException BadJumpDestination(int pc)
        {
            return new BadJumpDestinationException(
                string.Format("Operation with pc isn't 'JUMPDEST': PC[{0}];", pc));
        }

        public static StackTooSmallException TooSmallStack(int expected_size, int actual_size)
        {
            return new StackTooSmallException(
                string.Format(
                        "Expected stack size %d but actual {0}",
                        expected_size,
                        actual_size));
        }
    }
}
