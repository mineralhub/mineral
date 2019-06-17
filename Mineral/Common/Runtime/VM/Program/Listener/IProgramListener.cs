using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Runtime.VM.Program.Listener
{
    public interface IProgramListener
    {
        void OnMemoryExtend(int delta);
        void OnMemoryWrite(int address, byte[] data, int size);
        void OnStackPop();
        void OnStackPush(DataWord value);
        void OnStackSwap(int from, int to);
        void OnStoragePut(DataWord key, DataWord value);
        void OnStorageClear();
    }
}
