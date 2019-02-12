namespace Mineral.Network
{
    public enum DisconnectType
    {
        None,
        InvalidMessageFlow,
        InvalidBlock,
        InvalidTransaction,
        InvalidData,
        MultiConnection,
        Exception,
    }
}
