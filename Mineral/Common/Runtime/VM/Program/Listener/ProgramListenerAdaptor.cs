using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Runtime.VM.Program.Listener
{
    public class ProgramListenerAdaptor : IProgramListener
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
        public virtual void OnMemoryExtend(int delta) { }
        public virtual void OnMemoryWrite(int address, byte[] data, int size) { }
        public virtual void OnStackPop() { }
        public virtual void OnStackPush(DataWord value) { }
        public virtual void OnStackSwap(int from, int to) { }
        public virtual void OnStorageClear() { }
        public virtual void OnStoragePut(DataWord key, DataWord value) { }
        #endregion
    }
}
