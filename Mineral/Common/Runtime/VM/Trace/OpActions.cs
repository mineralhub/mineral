using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Utils;

namespace Mineral.Common.Runtime.VM.Trace
{
    public partial class OpActions
    {
        #region Field
        private List<Action> stack = new List<Action>();
        private List<Action> memory = new List<Action>();
        private List<Action> storage = new List<Action>();
        #endregion


        #region Property
        public List<Action> Stack
        {
            get { return this.stack; }
            set { this.stack = value; }
        }

        public List<Action> Memory
        {
            get { return this.memory; }
            set { this.memory = value; }
        }

        public List<Action> Storage
        {
            get { return this.storage; }
            set { this.storage = value; }
        }
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private static Action AddAction(List<Action> actions, Action.Name action_name)
        {
            Action action = new Action() { ActionName = action_name };
            actions.Add(action);

            return action;
        }
        #endregion


        #region External Method
        public Action AddStackPop()
        {
            return AddAction(stack, Action.Name.Pop);
        }

        public Action AddStackPush(DataWord value)
        {
            return AddAction(stack, Action.Name.Push)
                .AddParameter("value", value);
        }

        public Action AddStackSwap(int from, int to)
        {
            return AddAction(stack, Action.Name.Swap)
                .AddParameter("from", from)
                .AddParameter("to", to);
        }

        public Action AddMemoryExtend(long delta)
        {
            return AddAction(memory, Action.Name.Extend)
                .AddParameter("delta", delta);
        }

        public Action AddMemoryWrite(int address, byte[] data, int size)
        {
            return AddAction(memory, Action.Name.Write)
                .AddParameter("address", address)
                .AddParameter("data", data.ToHexString().Substring(0, size));
        }

        public Action AddStoragePut(DataWord key, DataWord value)
        {
            return AddAction(storage, Action.Name.Put)
                .AddParameter("key", key)
                .AddParameter("value", value);
        }

        public Action AddStorageRemove(DataWord key)
        {
            return AddAction(storage, Action.Name.Remove)
                .AddParameter("key", key);
        }

        public Action AddStorageClear()
        {
            return AddAction(storage, Action.Name.Clear);
        }
        #endregion
    }
}
