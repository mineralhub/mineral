using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Storage;
using Protocol;

namespace Mineral.Common.Runtime.VM.Program.Invoke
{
    public interface IProgramInvokeFactory
    {
        IProgramInvoke CreateProgramInvoke(
                    InternalTransaction.TransactionType tx_type,
                    InternalTransaction.ExecutorType executor_type,
                    Transaction tx,
                    long token_value,
                    long token_id,
                    Block block,
                    IDeposit deposit,
                    long vm_start_us,
                    long vm_should_end_us,
                    long energy_limit);

        IProgramInvoke CreateProgramInvoke(
                    Program program,
                    DataWord to_adderess,
                    DataWord caller_address,
                    DataWord in_value,
                    DataWord token_value,
                    DataWord token_id,
                    long balance,
                    byte[] data,
                    IDeposit deposit,
                    bool static_call,
                    bool by_testing_suite,
                    long vm_start_us,
                    long vm_should_end_us,
                    long energy_limit);
    }
}
