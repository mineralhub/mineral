using Sky.Cryptography;
using System.Collections.Generic;

namespace Sky.Core.DPos
{
    public class DelegateTurnTable
    {
        private Queue<ECKey> _table = new Queue<ECKey>();

        public ECKey Front => _table.Peek();
        public int Count => _table.Count;
        public ECKey Dequeue => _table.Dequeue();

        public void Enqueue(ECKey key)
        {
            _table.Enqueue(key);
        }
    }

    public class DPos
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
