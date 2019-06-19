using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Storage;
using Mineral.Core.Capsule;

namespace Mineral.Common.Runtime.VM.Program.Invoke
{
    public interface IProgramInvoke
    {
        bool IsStaticCall { get; }

        DataWord GetContractAddress();
        DataWord GetBalance();
        DataWord GetOriginAddress();
        DataWord GetCallerAddress();
        DataWord GetCallValue();
        DataWord GetTokenValue();
        DataWord GetTokenId();
        DataWord GetDataSize();
        DataWord GetDataValue(DataWord index_data);
        DataWord GetPrevHash();
        DataWord GetCoinbase();
        DataWord GetTimestamp();
        DataWord GetNumber();
        DataWord GetDifficulty();
        BlockCapsule GetBlockByNum(int index);
        IDeposit GetDeposit();

        byte[] GetDataCopy(DataWord offset_data, DataWord length_data);
        bool ByTestingSuite();
        int GetCallDeep();
        long GetVMShouldEndInUs();
        long GetVMStartInUs();
        long GetEnergyLimit();
        void GetStaticCall();
    }
}
