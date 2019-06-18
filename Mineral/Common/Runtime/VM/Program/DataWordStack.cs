using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Mineral.Common.Runtime.VM.Program.Listener;

namespace Mineral.Common.Runtime.VM.Program
{
    public class DataWordStack : IProgramListenerAware
    {
        #region Field
        private Stack<DataWord> stack = new Stack<DataWord>();

        private static readonly long serial_version_uid = 1;
        [NonSerialized]
        private IProgramListener program_listener = null;
        #endregion


        #region Property
        public int Size
        {
            get { return this.stack.Count; }
        }
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private bool IsAccessible(int from)
        {
            return from >= 0 && from < this.stack.Count;
        }

        private void StackSwap(int from, int to)
        {
            DataWord val_from = this.stack.ElementAt(from);
            DataWord val_to = this.stack.ElementAt(to);

            List<DataWord> temp = new List<DataWord>(this.stack.Count);

            for (int i = 0; i < temp.Count; i++)
            {
                if (from == i)
                    temp[i] = val_to;
                if (to == i)
                    temp[i] = val_from;
            }

            temp.Reverse();
            this.stack = new Stack<DataWord>(temp);
        }
        #endregion


        #region External Method
        public void SetProgramListener(IProgramListener listener)
        {
            this.program_listener = listener;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public DataWord Pop()
        {
            this.program_listener?.OnStackPop();

            return this.stack.Pop();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Push(DataWord obj)
        {
            this.program_listener?.OnStackPush(obj);
            this.stack.Push(obj);
        }

        public void Swap(int from, int to)
        {
            if (IsAccessible(from) && IsAccessible(to) && from != to)
            {
                if (this.program_listener != null)
                {
                    this.program_listener.OnStackSwap(from, to);
                }
                StackSwap(from, to);
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;

            if (obj == null || GetType() != obj.GetType())
                return false;

            DataWordStack datawords = (DataWordStack)obj;
            return object.Equals(program_listener, datawords.program_listener);
        }
        #endregion
    }
}
