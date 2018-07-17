namespace Sky.Sql
{
    public static class Transaction
    {
        public static string SelectByBlockHash = @"SELECT ""hash"", ""version"", ""type"", ""from"", ""to"", ""timestamp"", ""amount"", ""fee"", ""data"" FROM transactions WHERE blockHash=@blockHash";
        public static string InsertFormat = @"INSERT INTO transactions (""hash"", ""version"", ""from"", ""to"", ""timestamp"", ""amount"", ""fee"", ""blockHash"") VALUES " +
            "(@hash, @version, @from, @to, @timestamp, @amount, @fee, @blockHash)";
    }
}