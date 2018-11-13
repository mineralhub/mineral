namespace Mineral.Core
{
    public abstract class Proof
    {
        public abstract int GetCreateCount(UInt160 addr, int height);
        public abstract int RemainUpdate(int height);
        public abstract void Update(Blockchain chain);
        public abstract void SetTurnTable(TurnTableState state);
    }
}
