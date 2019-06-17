using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Runtime.VM
{
    public class OpCodeAttribute : Attribute
    {
        public enum Tier
        {
            InvalidTier = 0,
            ZeroTier = 0,
            SpecialTier = 1,
            BaseTier = 2,
            VeryLowTier = 3,
            LowTier = 5,
            MidTier = 8,
            HighTier = 10,
            ExtTier = 20,
        }
        public enum CallFlags
        {
            /**
             * Indicates that opcode is a call
             */
            Call,

            /**
             * Indicates that the code is executed in the context of the caller
             */
            Stateless,

            /**
             * Indicates that the opcode has value parameter (3rd on stack)
             */
            HasValue,

            /**
             * Indicates that any state modifications are disallowed during the call
             */
            Static,

            /**
             * Indicates that value and message sender are propagated from parent to child scope
             */
            Delegate
        }

        #region Field
        #endregion


        #region Property
        public byte OpCode { get; set; }
        public int Require { get; set; }
        public Tier OpCodeTier { get; set; }
        public int Result { get; set; }
        public List<CallFlags> Flags { get; set; } = new List<CallFlags>();
        #endregion


        #region Contructor
        //public OpCodeAttribute(byte op_code, int require, int result, Tier tier, CallFlags[] flags = null)
        //{
        //    this.OpCode = op_code;
        //    this.Require = require;
        //    this.Result = result;
        //    this.OpCodeTier = tier;
        //    this.Flags = new List<CallFlags>(flags);
        //}

        public OpCodeAttribute(byte op_code, int require, int result, Tier tier, params CallFlags[] flags)
        {
            this.OpCode = op_code;
            this.Require = require;
            this.Result = result;
            this.OpCodeTier = tier;
            this.Flags = new List<CallFlags>(flags);
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        #endregion
    }
}
