namespace Mineral.Core
{
    public enum ERROR_BLOCK
    {
        NO_ERROR = 0,
        ERROR,
        ERROR_HEIGHT,
        ERROR_EXIST_HEIGHT,
        ERROR_HASH,
        ERORR_EXIST_HASH,
    };

    public enum MINERAL_ERROR_CODES
    {
        // System
        NO_ERROR = 0,
        SYS_ERROR = 1,
        SYS_BLOCK_HEIGHT    = 2,

        // Transaction
        SYS_EXIST_TRANSACTION = 100,

        TX_FROM_ADDRESS_INVALID = 1000,
        TX_FROM_ACCOUNT_INVALID,
        TX_SIGNATURE_INVALID,
        TX_NOT_ENOUGH_BALANCE,
        TX_SELF_TRANSFER_NOT_ALLOWED,
        TX_TOO_SMALL_TRANSFER_BALANCE,
        TX_FEE_VALUE_MISMATCH,

        TX_NOT_ENOUGH_LOCKBALANCE = 2000,
        TX_NO_LOCK_BALANCE,
        TX_LOCK_VALUE_CANNOT_NEGATIVE,
        TX_LOCK_TTL_NOT_ARRIVED,

        TX_VOTE_TTL_NOT_ARRIVED = 2100,
        TX_VOTE_OVERCOUNT,
        TX_ZERO_VOTE_VALUE_NOT_ALLOWED,
        TX_DELEGATE_NOT_REGISTERED,

        // delegate
        TX_DELEGATE_NAME_INVALID = 2200,
        TX_DELEGATE_ALREADY_REGISTER,
    };
}
