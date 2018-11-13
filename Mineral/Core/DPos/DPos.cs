using System;
using System.Collections.Generic;

namespace Mineral.Core.DPos
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
            return UpdateHeight + Config.Instance.RoundBlock - height;
        }

        public UInt160 GetTurn(int height)
        {
            return _table[(height - UpdateHeight) % Count];
        }
    }

    public class DPos : Proof
    {
        public DelegateTurnTable TurnTable { get; protected set; }

        public DPos()
        {
            TurnTable = new DelegateTurnTable();
        }

        public int CalcBlockTime(int genesisBlockTime, long height)
        {
            return genesisBlockTime + (int)height * Config.Instance.Block.NextBlockTimeSec;
        }

        public override int GetCreateCount(UInt160 addr, int height)
        {
            for (int i = 1; i < Config.Instance.MaxDelegate + 1; i++)
            {
                var time = CalcBlockTime(Config.Instance.GenesisBlock.Timestamp, height + i);
                if (DateTime.UtcNow.ToTimestamp() < time)
                    return 0;
                UInt160 hash = TurnTable.GetTurn(height + i);
                if (addr == hash)
                {
                    int remain = TurnTable.RemainUpdate(height + i);
                    if (remain < 0)
                        return i + remain;
                    return i;
                }
            }
            return 0;
        }

        public override int RemainUpdate(int height)
        {
            return TurnTable.RemainUpdate(height);
        }

        public override void Update(Blockchain chain)
        {
            int currentHeight = chain.CurrentBlockHeight;
            UpdateTurnTable(chain, chain.GetBlock(currentHeight - currentHeight % Config.Instance.RoundBlock));
        }

        private void UpdateTurnTable(Blockchain chain, Block block)
        {
            // calculate turn table
            List<DelegateState> delegates = chain.GetDelegateStateAll();
            delegates.Sort((x, y) =>
            {
                var valueX = x.Votes.Sum(p => p.Value).Value;
                var valueY = y.Votes.Sum(p => p.Value).Value;
                if (valueX == valueY)
                {
                    if (x.AddressHash < y.AddressHash)
                        return -1;
                    else
                        return 1;
                }
                else if (valueX < valueY)
                    return -1;
                return 1;
            });

            int delegateRange = Config.Instance.MaxDelegate < delegates.Count ? Config.Instance.MaxDelegate : delegates.Count;
            List<UInt160> addrs = new List<UInt160>();
            for (int i = 0; i < delegateRange; ++i)
                addrs.Add(delegates[i].AddressHash);

            TurnTable.SetTable(addrs);
            TurnTable.SetUpdateHeight(block.Height);
            chain.PersistTurnTable(addrs, block.Height);
        }

        public override void SetTurnTable(TurnTableState state)
        {
            TurnTable.SetTable(state.addrs);
            TurnTable.SetUpdateHeight(state.turnTableHeight);
        }
    }
}
