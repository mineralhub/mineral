using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Runtime.VM.Program.Invoke;

namespace Mineral.Common.Runtime.VM.Trace
{
    using VMConfig = Runtime.Config.VMConfig;

    public class ProgramTrace
    {
        #region Field
        private List<Op> ops = new List<Op>();
        private string result = "";
        private string error = "";
        private string contract_address = "";
        #endregion


        #region Property
        public List<Op> Ops
        {
            get { return this.ops; }
            set { this.ops = value; }
        }

        public string Result
        {
            get { return this.result; }
            set { this.result = value; }
        }

        public string Error
        {
            get { return this.error; }
            set { this.error = value; }
        }

        public string ContractAddress
        {
            get { return this.contract_address; }
            set { this.contract_address = value; }
        }
        #endregion


        #region Contructor
        public ProgramTrace() : this(null) { }
        public ProgramTrace(IProgramInvoke invoke)
        {
            if (invoke != null && VMConfig.Instance.IsVmTrace)
            {
                this.contract_address = Core.Wallet.ToMineralAddress(invoke.GetContractAddress().GetLast20Bytes()).ToHexString();
            }
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public ProgramTrace SetResult(byte[] result)
        {
            this.result = result.ToHexString();
            return this;
        }

        public ProgramTrace SetError(System.Exception exception)
        {
            Error = (exception != null ? string.Format("{0}: {1}", exception.StackTrace, exception.Message));
            return this;
        }

        public Op AddOp(byte code, int pc, int deep, DataWord energy, OpActions actions)
        {
            Op op = new Op();
            op.Code = (OpCode)code;
            op.PC = pc;
            op.Deep = deep;
            op.Energy = energy.ToBigInteger();
            op.Actions = actions;
            this.ops.Add(op);

            return op;
        }

        public void Merge(ProgramTrace program_trace)
        {
            this.ops.AddRange(program_trace.ops);
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
