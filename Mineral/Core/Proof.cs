using Mineral.Utils;

namespace Mineral.Core
{
    public abstract class Proof
    {
        public abstract int CalcBlockHeight(int time);
        public abstract int GetCreateBlockCount(UInt160 addr, int height);
        public abstract int RemainUpdate(int height);
        public abstract void Update(BlockChain chain);
        public abstract void SetTurnTable(TurnTableState state);
    }
}
