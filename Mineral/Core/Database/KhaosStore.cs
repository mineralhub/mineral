using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mineral.Common.Utils;
using Mineral.Utils;
using Org.BouncyCastle.Utilities.Collections;
using static Mineral.Core.Capsule.BlockCapsule;

namespace Mineral.Core.Database
{
    public class KhaosStore
    {
        #region Field
        private Dictionary<BlockId, KhaosBlock> khaosblock_hashes = new Dictionary<BlockId, KhaosBlock>();
        private Dictionary<long, List<KhaosBlock>> khaosblock_numbers = new Dictionary<long, List<KhaosBlock>>();

        private KhaosBlock head = null;
        private int max_capacity = 1024;
        #endregion


        #region Property
        public int MaxCapacity
        {
            get { return this.max_capacity; }
            set { this.max_capacity = value; }
        }

        public KhaosBlock Head
        {
            get { return this.head; }
            set { this.head = value; }
        }

        public bool IsEmpty => this.khaosblock_hashes.IsNullOrEmpty();
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Insert(KhaosBlock block)
        {
            khaosblock_hashes.Add(block.Id, block);

            long min = Math.Max(0, head.Num - max_capacity);
            foreach (KeyValuePair<long, List<KhaosBlock>> pair in this.khaosblock_numbers.Where(x => x.Key < min))
            {
                this.khaosblock_numbers.Remove(pair.Key);
                foreach (KhaosBlock b in pair.Value)
                {
                    this.khaosblock_hashes.Remove(b.Id);
                }
            }

            if (this.khaosblock_numbers.ContainsKey(block.Num))
            {
                List<KhaosBlock> blocks = this.khaosblock_numbers[block.Num];
                if (blocks == null)
                    blocks = new List<KhaosBlock>();

                blocks.Add(block);
                this.khaosblock_numbers[block.Num] = blocks;
            }
            else
            {
                this.khaosblock_numbers.Add(block.Num, new List<KhaosBlock>() { block });
            }
        }

        public bool Remove(SHA256Hash hash)
        {
            if (this.khaosblock_hashes.TryGetValue(new BlockId(hash), out KhaosBlock block))
            {
                if (this.khaosblock_numbers.TryGetValue(block.Num, out List<KhaosBlock> blocks))
                {
                    blocks.RemoveAll(x => x.Id.Equals(hash));
                }

                if (!blocks.IsNotNullOrEmpty())
                    this.khaosblock_numbers.Remove(block.Num);

                this.khaosblock_hashes.Remove(new BlockId(hash));
            }

            return block != null ? true : false;
        }

        public List<KhaosBlock> GetBlockByNum(long num)
        {
            this.khaosblock_numbers.TryGetValue(num, out List<KhaosBlock> blocks);

            return blocks;
        }

        public KhaosBlock GetBlockByHash(SHA256Hash hash)
        {
            this.khaosblock_hashes.TryGetValue(new BlockId(hash), out KhaosBlock block);

            return block;
        }

        public KhaosBlock GetFirst()
        {
            long num = this.khaosblock_numbers.Keys.Max();

            return this.khaosblock_numbers[num].First();
        }
        #endregion
    }
}
