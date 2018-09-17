using System.Collections.Generic;

namespace Sky.Core.DPos
{
    public class DelegateTurnTable
    {
        private List<UInt160> _table = new List<UInt160>();
        public int UpdateHeight { get; private set; }
        public int Count => _table.Count;

        public void SetTable(List<UInt160> addressHashes)
        {
            _table = addressHashes;
        }

        public void SetUpdateHeight(int height)
        {
            UpdateHeight = height;
        }

        public int RemainUpdate(int height)
        {
            return UpdateHeight + Config.RoundBlock - height;
        }

        public UInt160 GetTurn(int height)
        {
            int remain = RemainUpdate(height);
            if (remain < 0)
                return null;

            return _table[(height - UpdateHeight) % Count];
        }
    }

    public class DPos: Proof
    {
        public DelegateTurnTable TurnTable { get; protected set; }

        public DPos()
        {
            TurnTable = new DelegateTurnTable();
        }

        public int CalcBlockTime(int genesisBlockTime, long height)
        {
            return genesisBlockTime + (int)height * Config.Block.NextBlockTimeSec;
        }
    }
}
