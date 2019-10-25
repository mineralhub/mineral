using Org.BouncyCastle.Math;
using System;
using System.Text;

namespace Mineral.Common.Runtime.VM.Trace
{
    public class Op
    {
        #region Field
        private OpCode code;
        private int deep = 0;
        private int pc = 0;
        private BigInteger energy = BigInteger.Zero;
        private OpActions actions = null;
        #endregion


        #region Property
        public OpCode Code
        {
            get { return this.code; }
            set { this.code = value; }
        }

        public int Deep
        {
            get { return this.deep; }
            set { this.deep = value; }
        }

        public int PC
        {
            get { return this.pc; }
            set { this.pc = value; }
        }

        public BigInteger Energy
        {
            get { return this.energy; }
            set { this.energy = value; }
        }

        public OpActions Actions
        {
            get { return this.actions; }
            set { this.actions = value; }
        }
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        #endregion
    }
}
