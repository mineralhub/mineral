using Mineral.Old;
using Mineral.Utils;
using System;
using System.Collections.Generic;

namespace Mineral.Core2.DPos
{
    public class DelegateTurnTable
    {
        private List<UInt160> _table = new List<UInt160>();
        public uint UpdateHeight { get; private set; }
        public uint Count => (uint)_table.Count;

        public void SetTable(List<UInt160> addressHashes)
        {
            _table = addressHashes;
        }

        public void SetUpdateHeight(uint height)
        {
            UpdateHeight = height;
        }

        public uint RemainUpdate(uint height)
        {
            return UpdateHeight + Config.Instance.RoundBlock - height;
        }

        public UInt160 GetTurn(uint height)
        {
            return _table[(int)((height - UpdateHeight) % Count)];
        }
    }

    public class DPos : Proof
    {
        public DelegateTurnTable TurnTable { get; protected set; }

        public DPos()
        {
            TurnTable = new DelegateTurnTable();
        }

        public uint CalcBlockTime(uint height)
        {
            return PrevConfig.Instance.GenesisBlock.Timestamp + height * PrevConfig.Instance.Block.NextBlockTimeSec;
        }

        public override uint CalcBlockHeight(uint time)
        {
            return (time - PrevConfig.Instance.GenesisBlock.Timestamp) / PrevConfig.Instance.Block.NextBlockTimeSec;
        }

        public override uint GetCreateBlockCount(UInt160 addr, uint height)
        {
            uint targetHeight = CalcBlockHeight((uint)DateTime.UtcNow.ToTimestamp());
            if (TurnTable.GetTurn(targetHeight) == addr)
            {
                uint remain = TurnTable.RemainUpdate(height);
                if (remain < targetHeight - height)
                    return remain;
                return targetHeight - height;
            }
            return 0;
        }

        public override uint RemainUpdate(uint height)
        {
            return TurnTable.RemainUpdate(height);
        }

        public override void Update(BlockChain chain)
        {
            uint currentHeight = chain.CurrentBlockHeight;
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
