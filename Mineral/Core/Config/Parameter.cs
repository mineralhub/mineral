using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Config
{
    public class Parameter
    {
        public static class ChainParameters
        {
            public static readonly long TRANSFER_FEE = 0;
            public static readonly int WITNESS_STANDBY_LENGTH = 127;
            public static readonly int SOLIDIFIED_THRESHOLD = 70;
            public static readonly int PRIVATE_KEY_LENGTH = 64;
            public static readonly int PRIVATE_KEY_BYTE_LENGTH = 32;
            public static readonly int MAX_ACTIVE_WITNESS_NUM = 27;
            public static readonly int BLOCK_SIZE = 2_000_000;
            public static readonly int BLOCK_PRODUCED_INTERVAL = 3000;
            public static readonly long CLOCK_MAX_DELAY = 3600000;
            public static readonly int BLOCK_PRODUCED_TIME_OUT = 50;
            public static readonly long PRECISION = 1_000_000;
            public static readonly long WINDOW_SIZE_MS = 24 * 3600 * 1000L;
            public static readonly long MS_PER_YEAR = 365 * 24 * 3600 * 1000L;
            public static readonly long MAINTENANCE_SKIP_SLOTS = 2;
            public static readonly int SINGLE_REPEAT = 1;
            public static readonly int BLOCK_FILLED_SLOTS_NUMBER = 128;
            public static readonly int MAX_VOTE_NUMBER = 30;
            public static readonly int MAX_FROZEN_NUMBER = 1;
            public static readonly int BLOCK_VERSION = 1;
        }

        public class NodeParameters
        {
            public static readonly long SYNC_RETURN_BATCH_NUM = 1000;
            public static readonly long SYNC_FETCH_BATCH_NUM = 2000;
            public static readonly long MAX_BLOCKS_IN_PROCESS = 400;
            public static readonly long MAX_BLOCKS_ALREADY_FETCHED = 800;
            public static readonly long MAX_BLOCKS_SYNC_FROM_ONE_PEER = 1000;
            public static readonly long SYNC_CHAIN_LIMIT_NUM = 500;
            public static readonly int MAX_TRANSACTION_PENDING = 2000;
        }

        public class NetParameters
        {
            public static readonly long GRPC_IDLE_TIME_OUT = 60000L;
            public static readonly long ADV_TIME_OUT = 20000L;
            public static readonly long SYNC_TIME_OUT = 5000L;
            public static readonly long HEAD_NUM_MAX_DELTA = 1000L;
            public static readonly long HEAD_NUM_CHECK_TIME = 60000L;
            public static readonly int MAX_INVENTORY_SIZE_IN_MINUTES = 2;
            public static readonly long NET_MAX_TRX_PER_SECOND = 700L;
            public static readonly long MAX_TRX_PER_PEER = 200L;
            public static readonly int NET_MAX_INV_SIZE_IN_MINUTES = 2;
            public static readonly int MSG_CACHE_DURATION_IN_BLOCKS = 5;
            public static readonly int MAX_BLOCK_FETCH_PER_PEER = 100;
            public static readonly int MAX_TRX_FETCH_PER_PEER = 1000;
        }

        public class DatabaseParameters
        {
            public static readonly int TRANSACTIONS_COUNT_LIMIT_MAX = 1000;
            public static readonly int ASSET_ISSUE_COUNT_LIMIT_MAX = 1000;
            public static readonly int PROPOSAL_COUNT_LIMIT_MAX = 1000;
            public static readonly int EXCHANGE_COUNT_LIMIT_MAX = 1000;
        }

        public class AdaptiveResourceLimitParameters
        {
            public static readonly int CONTRACT_RATE_NUMERATOR = 99;
            public static readonly int CONTRACT_RATE_DENOMINATOR = 100;
            public static readonly int EXPAND_RATE_NUMERATOR = 1000;
            public static readonly int EXPAND_RATE_DENOMINATOR = 999;
            public static readonly int PERIODS_MS = 60_000;
            public static readonly int LIMIT_MULTIPLIER = 1000;
        }

        public enum ForkBlockVersion
        {
            ENERGY_LIMIT = 5,
            VERSION_3_2_2 = 6,
            VERSION_3_5 = 7,
            VERSION_3_6 = 8
        }

        public class ForkBlockVersionParameters
        {
            public static readonly int START_NEW_TRANSACTION = 4;
            public static readonly int ENERGY_LIMIT = 5;
        }
    }
}
