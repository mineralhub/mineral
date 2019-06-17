using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Runtime.VM.Program.Listener
{
    public class CompositeProgramListener : ProgramListenerAdaptor
    {
        #region Field
        private List<IProgramListener> listeners = new List<IProgramListener>();
        #endregion


        #region Property
        public bool IsEmpty
        {
            get { return this.IsEmpty; }
        }
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override void OnMemoryExtend(int delta)
        {
            this.listeners.ForEach(listener => listener.OnMemoryExtend(delta));
        }

        public override void OnMemoryWrite(int address, byte[] data, int size)
        {
            this.listeners.ForEach(listener => listener.OnMemoryWrite(address, data, size));
        }

        public override void OnStackPop()
        {
            this.listeners.ForEach(listener => listener.OnStackPop());
        }

        public override void OnStackPush(DataWord value)
        {
            this.listeners.ForEach(listener => listener.OnStackPush(value));
        }

        public override void OnStackSwap(int from, int to)
        {
            this.listeners.ForEach(listener => listener.OnStackSwap(from, to));
        }

        public override void OnStorageClear()
        {
            this.listeners.ForEach(listener => listener.OnStorageClear());
        }

        public override void OnStoragePut(DataWord key, DataWord value)
        {
            this.listeners.ForEach(listener => listener.OnStoragePut(key, value));
        }

        public void AddListener(IProgramListener listener)
        {
            this.listeners.Add(listener);
        }
        #endregion
    }
}
