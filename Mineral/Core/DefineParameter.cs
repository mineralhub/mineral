using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core
{
    public class DefineParameter
    {
        //public static readonly byte[] LAST_HASH = "lastHash".HexToBytes();
        public static readonly string DIFFICULTY = "2001";

        public static readonly string BLOCK_DB_NAME = "block_data";
        public static readonly string TRANSACTION_DB_NAME = "transaction_data";

        public static readonly string CONF_FILE = "config.conf";

        public static readonly string TEST_CONF = "config-test.conf";

        public static readonly string DATABASE_DIR = "storage.directory";

        public static readonly byte ADD_PRE_FIX_BYTE_MAINNET = (byte)0x41;   //41 + address
        public static readonly string ADD_PRE_FIX_STRING_MAINNET = "41";
        public static readonly byte ADD_PRE_FIX_BYTE_TESTNET = (byte)0xa0;   //a0 + address
        public static readonly string ADD_PRE_FIX_STRING_TESTNET = "a0";
        public static readonly int ADDRESS_SIZE = 42;

        public static readonly long TRANSACTION_MAX_BYTE_SIZE = 500 * 1_024L;
        public static readonly long MAXIMUM_TIME_UNTIL_EXPIRATION = 24 * 60 * 60 * 1_000L; //one day
        public static readonly long TRANSACTION_DEFAULT_EXPIRATION_TIME = 60 * 1_000L; //60 seconds
                                                                                       // config for smart contract
        public static readonly long SUN_PER_ENERGY = 100; // 1 us = 100 DROP = 100 * 10^-6 TRX
        public static readonly long ENERGY_LIMIT_IN_CONSTANT_TX = 3_000_000L; // ref: 1 us = 1 energy
        public static readonly long MAX_RESULT_SIZE_IN_TX = 64; // max 8 * 8 items in result
        public static readonly long PB_DEFAULT_ENERGY_LIMIT = 0L;
        public static readonly long CREATOR_DEFAULT_ENERGY_LIMIT = 1000 * 10_000L;

        public static readonly int ONE_HUNDRED = 100;
        public static readonly int ONE_THOUSAND = 1000;

        public static readonly int NORMALTRANSACTION = 0;
        public static readonly int UNEXECUTEDDEFERREDTRANSACTION = 1;
        public static readonly int EXECUTINGDEFERREDTRANSACTION = 2;
    }
}
