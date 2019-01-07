using Mineral.Utils;
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

        public int CalcBlockTime(int height)
        {
            return Config.Instance.GenesisBlock.Timestamp + height * Config.Instance.Block.NextBlockTimeSec;
        }

        public override int CalcBlockHeight(int time)
        {
            return (time - Config.Instance.GenesisBlock.Timestamp) / Config.Instance.Block.NextBlockTimeSec;
        }

        public override int GetCreateBlockCount(UInt160 addr, int height)
        {
            int targetHeight = CalcBlockHeight(DateTime.UtcNow.ToTimestamp());
            if (TurnTable.GetTurn(targetHeight) == addr)
            {
                int remain = TurnTable.RemainUpdate(height);
                if (remain < targetHeight - height)
                    return remain;
                return targetHeight - height;
            }
            return 0;
        }

        public override int RemainUpdate(int height)
        {
            return TurnTable.RemainUpdate(height);
        }

        public override void Update(BlockChain chain)
        {
            int currentHeight = chain.CurrentBlockHeight;
            UpdateTurnTable(chain, chain.GetBlock(currentHeight - currentHeight % Config.Instance.RoundBlock));
        }

        private void UpdateTurnTable(BlockChain chain, Block block)
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
