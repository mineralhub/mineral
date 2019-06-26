using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Storage;
using Mineral.Core.Capsule;

namespace Mineral.Common.Runtime.VM.Program.Invoke
{
    public interface IProgramInvoke
    {
        bool IsStaticCall { get; set; }

        DataWord ContractAddress { get; }
        DataWord Balance { get; }
        DataWord OriginAddress { get; }
        DataWord CallerAddress { get; }
        DataWord CallValue { get; }
        DataWord TokenValue { get; }
        DataWord TokenId { get; }
        DataWord DataSize { get; }
        DataWord PrevHash { get; }
        DataWord Coinbase { get; }
        DataWord Timestamp { get; }
        DataWord Number { get; }
        DataWord Difficulty { get; }
        IDeposit Deposit { get; }
        bool IsTestingSuite { get; }
        int CallDeep { get; }
        long VMShouldEndInUs { get; }
        long VMStartInUs { get; }
        long EnergyLimit { get; }

        BlockCapsule GetBlockByNum(int index);
        DataWord GetDataValue(DataWord index_data);
        byte[] GetDataCopy(DataWord offset_data, DataWord length_data);
    }
}
