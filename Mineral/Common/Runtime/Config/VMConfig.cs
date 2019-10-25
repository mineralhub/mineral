using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Utils;
using Mineral.Core.Config;
using Mineral.Core.Config.Arguments;

namespace Mineral.Common.Runtime.Config
{
    public class VMConfig
    {
        #region Field
        public static readonly int MAX_CODE_LENGTH = 1024 * 1024;
        public static readonly int MAX_FEE_LIMIT = 1_000_000_000; //1000 trx

        private static bool ENERGY_LIMIT_HARD_FORK = false;
        private static bool ALLOW_TVM_TRANSFER_TRC10 = false;
        private static bool ALLOW_TVM_CONSTANTINOPLE = false;
        private static bool ALLOW_MULTI_SIGN = false;

        private static VMConfig instance = null;
        private bool vm_trace_compressed = false;
        private bool vm_trace = (bool)Args.Instance.VM.VMTrace;
        #endregion


        #region Property
        public static VMConfig Instance
        {
            get
            {
                if (instance == null)
                    instance = new VMConfig();

                return instance;
            }
        }

        public bool IsVmTrace => this.vm_trace;
        public bool IsVmTraceCompressed => this.vm_trace_compressed;

        public static bool EnergyLimitHardFork
        {
            get { return ENERGY_LIMIT_HARD_FORK; }
        }

        public static bool AllowTvmTransferTrc10
        {
            get { return ALLOW_TVM_TRANSFER_TRC10; }
        }

        public static bool AllowTvmConstantinople
        {
            get { return ALLOW_TVM_CONSTANTINOPLE; }
        }

        public static bool AllowMultiSign
        {
            get { return ALLOW_MULTI_SIGN; }
        }
        #endregion


        #region Contructor
        private VMConfig() { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public static void InitVmHardFork()
        {
            ENERGY_LIMIT_HARD_FORK = ForkController.Instance.Pass(Parameter.ForkBlockVersionParameters.ENERGY_LIMIT);
        }

        public static void InitAllowMultiSign(long allow)
        {
            ALLOW_MULTI_SIGN = allow == 1;
        }

        public static void InitAllowTvmTransferTrc10(long allow)
        {
            ALLOW_TVM_TRANSFER_TRC10 = allow == 1;
        }

        public static void InitAllowTvmConstantinople(long allow)
        {
            ALLOW_TVM_CONSTANTINOPLE = allow == 1;
        }
        #endregion
    }
}
