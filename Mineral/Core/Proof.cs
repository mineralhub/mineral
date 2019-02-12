using Mineral.Utils;

namespace Mineral.Core
{
    public abstract class Proof
    {
        public abstract uint CalcBlockHeight(uint time);
        public abstract uint GetCreateBlockCount(UInt160 addr, uint height);
        public abstract uint RemainUpdate(uint height);
        public abstract void Update(BlockChain chain);
        public abstract void SetTurnTable(TurnTableState state);
    }
}
