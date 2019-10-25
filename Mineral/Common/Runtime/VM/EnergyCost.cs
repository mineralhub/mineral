using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Common.Runtime.VM
{
    public static class EnergyCost
    {
        public static readonly int STEP = 1;
        public static readonly int SSTORE = 300;

        public static readonly int ZEROSTEP = 0;
        public static readonly int QUICKSTEP = 2;
        public static readonly int FASTESTSTEP = 3;
        public static readonly int FASTSTEP = 5;
        public static readonly int MIDSTEP = 8;
        public static readonly int SLOWSTEP = 10;
        public static readonly int EXTSTEP = 20;

        public static readonly int GENESISENERGYLIMIT = 1000000;
        public static readonly int MINENERGYLIMIT = 125000;

        public static readonly int BALANCE = 20;
        public static readonly int SHA3 = 30;
        public static readonly int SHA3_WORD = 6;
        public static readonly int SLOAD = 50;
        public static readonly int STOP = 0;
        public static readonly int SUICIDE = 0;
        public static readonly int CLEAR_SSTORE = 5000;
        public static readonly int SET_SSTORE = 20000;
        public static readonly int RESET_SSTORE = 5000;
        public static readonly int REFUND_SSTORE = 15000;
        public static readonly int CREATE = 32000;

        public static readonly int JUMPDEST = 1;
        public static readonly int CREATE_DATA_BYTE = 5;
        public static readonly int CALL = 40;
        public static readonly int STIPEND_CALL = 2300;
        public static readonly int VT_CALL = 9000;
        public static readonly int NEW_ACCT_CALL = 25000;
        public static readonly int MEMORY = 3;
        public static readonly int SUICIDE_REFUND = 24000;
        public static readonly int QUAD_COEFF_DIV = 512;
        public static readonly int CREATE_DATA = 200;
        public static readonly int TX_NO_ZERO_DATA = 68;
        public static readonly int TX_ZERO_DATA = 4;
        public static readonly int TRANSACTION = 21000;
        public static readonly int TRANSACTION_CREATE_CONTRACT = 53000;
        public static readonly int LOG_ENERGY = 375;
        public static readonly int LOG_DATA_ENERGY = 8;
        public static readonly int LOG_TOPIC_ENERGY = 375;
        public static readonly int COPY_ENERGY = 3;
        public static readonly int EXP_ENERGY = 10;
        public static readonly int EXP_BYTE_ENERGY = 10;
        public static readonly int IDENTITY = 15;
        public static readonly int IDENTITY_WORD = 3;
        public static readonly int RIPEMD160 = 600;
        public static readonly int RIPEMD160_WORD = 120;
        public static readonly int SHA256 = 60;
        public static readonly int SHA256_WORD = 12;
        public static readonly int EC_RECOVER = 3000;
        public static readonly int EXT_CODE_SIZE = 20;
        public static readonly int EXT_CODE_COPY = 20;
        public static readonly int EXT_CODE_HASH = 400;
        public static readonly int NEW_ACCT_SUICIDE = 0;
    }
}
