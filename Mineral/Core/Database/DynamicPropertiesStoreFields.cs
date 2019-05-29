using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Utils;

namespace Mineral.Core.Database
{
    public partial class DynamicPropertiesStore
    {
        private static class DynamicResourceProperties
        {
            public static readonly byte[] ONE_DAY_NET_LIMIT = "ONE_DAY_NET_LIMIT".GetBytes();
            public static readonly byte[] PUBLIC_NET_USAGE = "PUBLIC_NET_USAGE".GetBytes();
            public static readonly byte[] PUBLIC_NET_LIMIT = "PUBLIC_NET_LIMIT".GetBytes();
            public static readonly byte[] PUBLIC_NET_TIME = "PUBLIC_NET_TIME".GetBytes();
            public static readonly byte[] FREE_NET_LIMIT = "FREE_NET_LIMIT".GetBytes();
            public static readonly byte[] TOTAL_NET_WEIGHT = "TOTAL_NET_WEIGHT".GetBytes();
            public static readonly byte[] TOTAL_NET_LIMIT = "TOTAL_NET_LIMIT".GetBytes();
            public static readonly byte[] TOTAL_ENERGY_TARGET_LIMIT = "TOTAL_ENERGY_TARGET_LIMIT".GetBytes();
            public static readonly byte[] TOTAL_ENERGY_CURRENT_LIMIT = "TOTAL_ENERGY_CURRENT_LIMIT".GetBytes();
            public static readonly byte[] TOTAL_ENERGY_AVERAGE_USAGE = "TOTAL_ENERGY_AVERAGE_USAGE".GetBytes();
            public static readonly byte[] TOTAL_ENERGY_AVERAGE_TIME = "TOTAL_ENERGY_AVERAGE_TIME".GetBytes();
            public static readonly byte[] TOTAL_ENERGY_WEIGHT = "TOTAL_ENERGY_WEIGHT".GetBytes();
            public static readonly byte[] TOTAL_ENERGY_LIMIT = "TOTAL_ENERGY_LIMIT".GetBytes();
            public static readonly byte[] BLOCK_ENERGY_USAGE = "BLOCK_ENERGY_USAGE".GetBytes();
        }

        private static readonly byte[] LATEST_BLOCK_HEADER_TIMESTAMP = "latest_block_header_timestamp".GetBytes();
        private static readonly byte[] LATEST_BLOCK_HEADER_NUMBER = "latest_block_header_number".GetBytes();
        private static readonly byte[] LATEST_BLOCK_HEADER_HASH = "latest_block_header_hash".GetBytes();
        private static readonly byte[] STATE_FLAG = "state_flag".GetBytes();
        private static readonly byte[] LATEST_SOLIDIFIED_BLOCK_NUM = "LATEST_SOLIDIFIED_BLOCK_NUM".GetBytes();
        private static readonly byte[] LATEST_PROPOSAL_NUM = "LATEST_PROPOSAL_NUM".GetBytes();
        private static readonly byte[] LATEST_EXCHANGE_NUM = "LATEST_EXCHANGE_NUM".GetBytes();
        private static readonly byte[] BLOCK_FILLED_SLOTS = "BLOCK_FILLED_SLOTS".GetBytes();
        private static readonly byte[] BLOCK_FILLED_SLOTS_INDEX = "BLOCK_FILLED_SLOTS_INDEX".GetBytes();
        private static readonly byte[] NEXT_MAINTENANCE_TIME = "NEXT_MAINTENANCE_TIME".GetBytes();
        private static readonly byte[] MAX_FROZEN_TIME = "MAX_FROZEN_TIME".GetBytes();
        private static readonly byte[] MIN_FROZEN_TIME = "MIN_FROZEN_TIME".GetBytes();
        private static readonly byte[] MAX_FROZEN_SUPPLY_NUMBER = "MAX_FROZEN_SUPPLY_NUMBER".GetBytes();
        private static readonly byte[] MAX_FROZEN_SUPPLY_TIME = "MAX_FROZEN_SUPPLY_TIME".GetBytes();
        private static readonly byte[] MIN_FROZEN_SUPPLY_TIME = "MIN_FROZEN_SUPPLY_TIME".GetBytes();
        private static readonly byte[] WITNESS_ALLOWANCE_FROZEN_TIME = "WITNESS_ALLOWANCE_FROZEN_TIME".GetBytes();
        private static readonly byte[] MAINTENANCE_TIME_INTERVAL = "MAINTENANCE_TIME_INTERVAL".GetBytes();
        private static readonly byte[] ACCOUNT_UPGRADE_COST = "ACCOUNT_UPGRADE_COST".GetBytes();
        private static readonly byte[] WITNESS_PAY_PER_BLOCK = "WITNESS_PAY_PER_BLOCK".GetBytes();
        private static readonly byte[] WITNESS_STANDBY_ALLOWANCE = "WITNESS_STANDBY_ALLOWANCE".GetBytes();

        private static readonly byte[] ENERGY_FEE = "ENERGY_FEE".GetBytes();
        private static readonly byte[] MAX_CPU_TIME_OF_ONE_TX = "MAX_CPU_TIME_OF_ONE_TX".GetBytes();
        private static readonly byte[] CREATE_ACCOUNT_FEE = "CREATE_ACCOUNT_FEE".GetBytes();
        private static readonly byte[] CREATE_NEW_ACCOUNT_FEE_IN_SYSTEM_CONTRACT = "CREATE_NEW_ACCOUNT_FEE_IN_SYSTEM_CONTRACT".GetBytes();
        private static readonly byte[] CREATE_NEW_ACCOUNT_BANDWIDTH_RATE = "CREATE_NEW_ACCOUNT_BANDWIDTH_RATE".GetBytes();
        private static readonly byte[] TRANSACTION_FEE = "TRANSACTION_FEE".GetBytes();
        private static readonly byte[] ASSET_ISSUE_FEE = "ASSET_ISSUE_FEE".GetBytes();
        private static readonly byte[] UPDATE_ACCOUNT_PERMISSION_FEE = "UPDATE_ACCOUNT_PERMISSION_FEE".GetBytes();
        private static readonly byte[] MULTI_SIGN_FEE = "MULTI_SIGN_FEE".GetBytes();
        private static readonly byte[] EXCHANGE_CREATE_FEE = "EXCHANGE_CREATE_FEE".GetBytes();
        private static readonly byte[] EXCHANGE_BALANCE_LIMIT = "EXCHANGE_BALANCE_LIMIT".GetBytes();
        private static readonly byte[] TOTAL_TRANSACTION_COST = "TOTAL_TRANSACTION_COST".GetBytes();
        private static readonly byte[] TOTAL_CREATE_ACCOUNT_COST = "TOTAL_CREATE_ACCOUNT_COST".GetBytes();
        private static readonly byte[] TOTAL_CREATE_WITNESS_COST = "TOTAL_CREATE_WITNESS_FEE".GetBytes();
        private static readonly byte[] TOTAL_STORAGE_POOL = "TOTAL_STORAGE_POOL".GetBytes();
        private static readonly byte[] TOTAL_STORAGE_TAX = "TOTAL_STORAGE_TAX".GetBytes();
        private static readonly byte[] TOTAL_STORAGE_RESERVED = "TOTAL_STORAGE_RESERVED".GetBytes();
        private static readonly byte[] STORAGE_EXCHANGE_TAX_RATE = "STORAGE_EXCHANGE_TAX_RATE".GetBytes();
        private static readonly byte[] FORK_CONTROLLER = "FORK_CONTROLLER".GetBytes();
        private static readonly String FORK_PREFIX = "FORK_VERSION_";

        private static readonly byte[] REMOVE_THE_POWER_OF_THE_GR = "REMOVE_THE_POWER_OF_THE_GR".GetBytes();
        private static readonly byte[] ALLOW_DELEGATE_RESOURCE = "ALLOW_DELEGATE_RESOURCE".GetBytes();
        private static readonly byte[] ALLOW_ADAPTIVE_ENERGY = "ALLOW_ADAPTIVE_ENERGY".GetBytes();
        private static readonly byte[] ALLOW_UPDATE_ACCOUNT_NAME = "ALLOW_UPDATE_ACCOUNT_NAME".GetBytes();
        private static readonly byte[] ALLOW_SAME_TOKEN_NAME = " ALLOW_SAME_TOKEN_NAME".GetBytes();
        private static readonly byte[] ALLOW_CREATION_OF_CONTRACTS = "ALLOW_CREATION_OF_CONTRACTS".GetBytes();
        private static readonly byte[] TOTAL_SIGN_NUM = "TOTAL_SIGN_NUM".GetBytes();
        private static readonly byte[] ALLOW_MULTI_SIGN = "ALLOW_MULTI_SIGN".GetBytes();
        private static readonly byte[] TOKEN_ID_NUM = "TOKEN_ID_NUM".GetBytes();
        private static readonly byte[] TOKEN_UPDATE_DONE = "TOKEN_UPDATE_DONE".GetBytes();
        private static readonly byte[] ALLOW_TVM_TRANSFER_TRC10 = "ALLOW_TVM_TRANSFER_TRC10".GetBytes();
        private static readonly byte[] ALLOW_TVM_CONSTANTINOPLE = "ALLOW_TVM_CONSTANTINOPLE".GetBytes();
        private static readonly byte[] ALLOW_PROTO_FILTER_NUM = "ALLOW_PROTO_FILTER_NUM".GetBytes();
        private static readonly byte[] AVAILABLE_CONTRACT_TYPE = "AVAILABLE_CONTRACT_TYPE".GetBytes();
        private static readonly byte[] ACTIVE_DEFAULT_OPERATIONS = "ACTIVE_DEFAULT_OPERATIONS".GetBytes();
        private static readonly byte[] ALLOW_ACCOUNT_STATE_ROOT = "ALLOW_ACCOUNT_STATE_ROOT".GetBytes();
    }
}
