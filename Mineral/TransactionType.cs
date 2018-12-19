namespace Mineral
{
    public enum TransactionType : short
    {
        SupplyTransaction = 1,
        TransferTransaction = 2,
        VoteTransaction = 3,
        RegisterDelegateTransaction = 4,
        OtherSignTransaction = 5,
        SignTransaction = 6,
        LockTransaction = 7,
        UnlockTransaction = 8,
    }
}
