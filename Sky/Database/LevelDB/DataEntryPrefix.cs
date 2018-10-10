namespace Sky.Database.LevelDB
{
    internal static class DataEntryPrefix
    {
        public const byte DATA_Block = 0x01;
        public const byte DATA_Transaction = 0x02;
        public const byte DATA_TxResult = 0x03;

        public const byte ST_Account = 0x40;
        public const byte ST_Delegate = 0x41;
        public const byte ST_Coin = 0x44;
        public const byte ST_SpentCoin = 0x45;
        public const byte ST_BlockTrigger = 0x46;
        public const byte ST_OtherSign = 0x47;

        public const byte IX_HeaderHashList = 0x80;

        public const byte SYS_CurrentBlock = 0xc0;
        public const byte SYS_CurrentHeader = 0xc1;

        public const byte SYS_Version = 0xf0;
    }

    // wallet indexer
    internal static class WIDataEntryPrefix
    {
        public const byte ST_Transaction = 0x40;

        public const byte IX_Group = 0x80;
        public const byte IX_Accounts = 0x81;

        public const byte SYS_Version = 0xf0;
    }
}
