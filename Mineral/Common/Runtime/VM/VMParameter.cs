using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Runtime.VM
{
    public static class VMParameter
    {
        public static readonly double TX_CPU_LIMIT_DEFAULT_RATIO = 1.0;

        public static readonly String REASON_ALREADY_TIME_OUT = "Haven Time Out";
        public static readonly int CONTRACT_NAME_LENGTH = 32;
        public static readonly int MIN_TOKEN_ID = 1000_000;
    }
}
