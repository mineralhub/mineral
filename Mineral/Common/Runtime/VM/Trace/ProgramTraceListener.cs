using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Runtime.VM.Program.Listener;

namespace Mineral.Common.Runtime.VM.Trace
{
    public class ProgramTraceListener : ProgramListenerAdaptor
    {
        #region Field
        private readonly bool enabled = false;
        private OpActions actions = new OpActions();
        #endregion


        #region Property
        #endregion


        #region Contructor
        public ProgramTraceListener(bool enabled)
        {
            this.enabled = enabled;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override void OnMemoryExtend(int delta)
        {
            if (this.enabled)
            {
                actions.AddMemoryExtend(delta);
            }
        }

        public override void OnMemoryWrite(int address, byte[] data, int size)
        {
            if (this.enabled)
            {
                actions.AddMemoryWrite(address, data, size);
            }
        }

        public override void OnStackPop()
        {
            if (this.enabled)
            {
                actions.AddStackPop();
            }
        }

        public override void OnStackPush(DataWord value)
        {
            if (this.enabled)
            {
                actions.AddStackPush(value);
            }
        }

        public override void OnStackSwap(int from, int to)
        {
            if (this.enabled)
            {
                actions.AddStackSwap(from, to);
            }
        }

        public override void OnStorageClear()
        {
            if (this.enabled)
            {
                actions.AddStorageClear();
            }
        }

        public override void OnStoragePut(DataWord key, DataWord value)
        {
            if (this.enabled)
            {
                if (value == DataWord.ZERO)
                    actions.AddStorageRemove(key);
                else
                    actions.AddStoragePut(key, value);
            }
        }

        public OpActions ResetActions()
        {
            OpActions result = this.actions;
            this.actions = new OpActions();

            return result;
        }
        #endregion
    }
}
