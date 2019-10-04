using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Mineral.Core.Capsule;

namespace Mineral.Core.Net.Service
{
    public class CheatWitnessInfo
    {
        #region Field
        private int times = 0;
        private long latest_block_num = 0;
        private HashSet<BlockCapsule> blocks = new HashSet<BlockCapsule>();
        private long time = 0;
        #endregion


        #region Property
        public int Times
        {
            get { return this.times; }
            set { Interlocked.Exchange(ref this.times, value); }
        }

        public long Time
        {
            get { return this.time; }
            set { this.time = value; }
        }

        public long LatestBlockNum
        {
            get { return this.latest_block_num; }
            set { this.latest_block_num = value; }
        }

        public HashSet<BlockCapsule> Blocks
        {
            get { return this.blocks; }
            set { this.blocks = value; }
        }
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Add(BlockCapsule block)
        {
            this.blocks.Add(block);
        }

        public void Increment()
        {
            Interlocked.Increment(ref this.times);
        }

        public void Clear()
        {
            this.blocks.Clear();
        }

        public override string ToString()
        {
            return "{" +
                "times=" + this.times +
                ", time=" + this.time +
                ", latestBlockNum=" + this.latest_block_num +
                ", blockCapsuleSet=" + this.blocks.ToString() +
                '}';
        }

        #endregion
    }
}
