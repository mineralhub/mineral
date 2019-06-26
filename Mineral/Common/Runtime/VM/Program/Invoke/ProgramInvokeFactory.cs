using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Storage;
using Mineral.Core;
using Mineral.Core.Capsule;
using Mineral.Core.Exception;
using Protocol;
using static Mineral.Common.Runtime.VM.InternalTransaction;

namespace Mineral.Common.Runtime.VM.Program.Invoke
{
    public class ProgramInvokeFactory : IProgramInvokeFactory
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public IProgramInvoke CreateProgramInvoke(TransactionType tx_type,
                                                  ExecutorType executor_type,
                                                  Transaction tx,
                                                  long token_value,
                                                  long token_id,
                                                  Block block,
                                                  IDeposit deposit,
                                                  long vm_start,
                                                  long vm_should_end,
                                                  long energy_limit)
        {
            byte[] data = null;
            byte[] last_hash = null;
            byte[] coinbase = null;
            byte[] contract_address = null;
            byte[] owner_address = null;
            long balance = 0;
            long number = -1;
            long timestamp = 0;

            if (tx_type == TransactionType.TX_CONTRACT_CREATION_TYPE)
            {
                CreateSmartContract contract = ContractCapsule.GetSmartContractFromTransaction(tx);
                contract_address = Wallet.GenerateContractAddress(tx);
                owner_address = contract.OwnerAddress.ToByteArray();
                balance = deposit.GetBalance(owner_address);
                data = new byte[0];

                long call_value = contract.NewContract.CallValue;
                switch (executor_type)
                {
                    case ExecutorType.ET_NORMAL_TYPE:
                    case ExecutorType.ET_PRE_TYPE:
                        {
                            if (null != block)
                            {
                                last_hash = block.BlockHeader.RawData.ParentHash.ToByteArray();
                                coinbase = block.BlockHeader.RawData.WitnessAddress.ToByteArray();
                                timestamp = block.BlockHeader.RawData.Timestamp / 1000;
                                number = block.BlockHeader.RawData.Number;
                            }
                        }
                        break;
                    default:
                        break;
                }

                return new ProgramInvoke(contract_address,
                                         owner_address,
                                         owner_address,
                                         balance,
                                         call_value,
                                         token_value,
                                         token_id,
                                         data,
                                         last_hash,
                                         coinbase,
                                         timestamp,
                                         number,
                                         deposit,
                                         vm_start,
                                         vm_should_end,
                                         energy_limit);
            }
            else if (tx_type == TransactionType.TX_CONTRACT_CALL_TYPE)
            {
                TriggerSmartContract contract = ContractCapsule.GetTriggerContractFromTransaction(tx);
                byte[] address = contract.ContractAddress.ToByteArray();
                byte[] origin = contract.OwnerAddress.ToByteArray();
                byte[] caller = contract.OwnerAddress.ToByteArray();
                balance = deposit.GetBalance(caller);
                long call_value = contract.CallValue;
                data = contract.Data.ToByteArray();

                switch (executor_type)
                {
                    case ExecutorType.ET_CONSTANT_TYPE:
                        break;
                    case ExecutorType.ET_PRE_TYPE:
                    case ExecutorType.ET_NORMAL_TYPE:
                        if (null != block)
                        {
                            last_hash = block.BlockHeader.RawData.ParentHash.ToByteArray();
                            coinbase = block.BlockHeader.RawData.WitnessAddress.ToByteArray();
                            timestamp = block.BlockHeader.RawData.Timestamp / 1000;
                            number = block.BlockHeader.RawData.Number;
                        }
                        break;
                    default:
                        break;
                }

                return new ProgramInvoke(address,
                                         origin,
                                         caller,
                                         balance,
                                         call_value,
                                         token_value,
                                         token_id,
                                         data,
                                         last_hash,
                                         coinbase,
                                         timestamp,
                                         number,
                                         deposit,
                                         vm_start,
                                         vm_should_end,
                                         energy_limit);
            }

            throw new ContractValidateException("Unknown contract type");
        }

        public IProgramInvoke CreateProgramInvoke(Program program,
                                                  DataWord to_adderess,
                                                  DataWord caller_address,
                                                  DataWord in_value,
                                                  DataWord token_value,
                                                  DataWord token_id,
                                                  long in_balance,
                                                  byte[] in_data,
                                                  IDeposit deposit,
                                                  bool is_static_call,
                                                  bool is_testing_suite,
                                                  long vm_start,
                                                  long vm_should_end,
                                                  long energy_limit)
        {
            DataWord address = to_adderess;
            DataWord origin = program.OriginAddress;
            DataWord caller = caller_address;
            DataWord balance = new DataWord(in_balance);
            DataWord call_value = in_value;

            byte[] data = null;
            Array.Copy(in_data, 0, data, 0, in_data.Length);

            DataWord last_hash = program.PrevHash;
            DataWord coinbase = program.Coinbase;
            DataWord timestamp = program.Timestamp;
            DataWord number = program.Number;
            DataWord difficulty = program.Difficulty;

            return new ProgramInvoke(address,
                                     origin,
                                     caller,
                                     balance,
                                     call_value,
                                     token_value,
                                     token_id,
                                     data,
                                     last_hash,
                                     coinbase,
                                     timestamp,
                                     number,
                                     difficulty,
                                     deposit,
                                     program.CallDeep + 1,
                                     is_static_call,
                                     is_testing_suite,
                                     vm_start,
                                     vm_should_end,
                                     energy_limit);
        }
        #endregion
    }
}
