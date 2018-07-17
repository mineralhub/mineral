namespace Sky.Sql
{
    public static class Block
    {
        public static string SelectByHash = @"SELECT ""height"", ""version"", ""timestamp"", ""merkleRoot"", ""previousBlockHash"" FROM ""blocks"" WHERE hash=@hash";
        public static string SelectByHeight = @"SELECT ""height"", ""version"", ""timestamp"", ""merkleRoot"", ""previousBlockHash"" FROM ""blocks"" WHERE height=@height";
        public static string InsertFormat = @"INSERT INTO blocks (""hash"", ""height"", ""previousBlockHash"", ""merkleRoot"", ""version"", ""timestamp"") VALUES" +
            "(@hash, @height, @previousBlockHash, @merkleRoot, @version, @timestamp)";
    }
}