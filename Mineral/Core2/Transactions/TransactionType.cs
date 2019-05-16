namespace Mineral.Core2.Transactions
{
    public enum TransactionType : short
    {
        Supply = 1,
        Transfer,
        Vote,
        RegisterDelegate,
        OtherSign,
        Sign,
        Lock,
        Unlock,
        BlockSign,
    }
}
