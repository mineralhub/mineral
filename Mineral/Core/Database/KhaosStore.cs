using System;
using System.Collections.Concurrent;
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
        private ConcurrentDictionary<BlockId, KhaosBlock> khaosblock_hashes = new ConcurrentDictionary<BlockId, KhaosBlock>(Environment.ProcessorCount * 2, 50000);
        private ConcurrentDictionary<long, List<KhaosBlock>> khaosblock_numbers = new ConcurrentDictionary<long, List<KhaosBlock>>(Environment.ProcessorCount * 2, 50000);

        private KhaosBlock head = null;
        private int max_capacity = 1024;
        #endregion


        #region Property
        public int Size
        {
            get { return this.khaosblock_hashes.Count; }
        }

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

        public bool IsEmpty
        {
            get { return this.khaosblock_hashes.IsNullOrEmpty(); }
        }
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
            // TODO 중복으로 들어올떄는 어떻게 해야할지
            if (!this.khaosblock_hashes.ContainsKey(block.Id))
            {
                this.khaosblock_hashes.TryAdd(block.Id, block);
            }

            long min = Math.Max(0, head.Num - max_capacity);
            foreach (KeyValuePair<long, List<KhaosBlock>> pair in this.khaosblock_numbers.Where(x => x.Key < min))
            {
                this.khaosblock_numbers.TryRemove(pair.Key, out _);
                foreach (KhaosBlock b in pair.Value)
                {
                    this.khaosblock_hashes.TryRemove(b.Id, out _);
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
                this.khaosblock_numbers.TryAdd(block.Num, new List<KhaosBlock>() { block });
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
                    this.khaosblock_numbers.TryRemove(block.Num, out _);

                this.khaosblock_hashes.TryRemove(new BlockId(hash), out _);
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
