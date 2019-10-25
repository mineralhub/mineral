using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Utils;
using Mineral.Core.Capsule;
using Mineral.Core.Exception;
using static Mineral.Core.Capsule.BlockCapsule;

namespace Mineral.Core.Database
{
    public class KhaosDatabase : MineralDatabase<BlockCapsule>
    {
        #region Field
        private KhaosBlock head = null;
        private KhaosStore mini_store = new KhaosStore();
        private KhaosStore mini_unlinked_store = new KhaosStore();
        #endregion


        #region Property
        public KhaosStore MiniStore => this.mini_store;
        public KhaosStore MiniUnlinkedStore => this.mini_unlinked_store;
        public bool IsEmpty => this.mini_store.IsEmpty;
        #endregion


        #region Constructor
        public KhaosDatabase(string dbname = "block_KDB") : base (dbname) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private void CheckNull(KhaosBlock block)
        {
            if (block == null)
                throw new NonCommonBlockException();
        }
        #endregion


        #region External Method
        public void Start(BlockCapsule block)
        {
            SetHead(new KhaosBlock(block));
            this.mini_store.Insert(this.head);
        }

        public void SetHead(KhaosBlock block)
        {
            this.head = block;
            this.mini_store.Head = this.head;
            this.mini_unlinked_store.Head = this.head;
        }

        public KhaosBlock GetHead()
        {
            return this.head;
        }

        public void RemoveBlock(SHA256Hash hash)
        {
            if (!this.mini_store.Remove(hash))
            {
                this.mini_unlinked_store.Remove(hash);
            }

            SetHead(this.mini_store.GetFirst());
        }

        public bool Contain(SHA256Hash hash)
        {
            return this.mini_store.GetBlockByHash(hash) != null || this.mini_unlinked_store.GetBlockByHash(hash) != null;
        }

        public bool ContainInMiniStore(SHA256Hash hash)
        {
            return this.mini_store.GetBlockByHash(hash) != null;
        }

        public BlockCapsule GetBlock(SHA256Hash hash)
        {
            KhaosBlock block = this.mini_store.GetBlockByHash(hash);
            if (block == null)
                block = this.mini_unlinked_store.GetBlockByHash(hash);

            return block?.Block;
        }

        public BlockCapsule Push(BlockCapsule block)
        {
            KhaosBlock kblock = new KhaosBlock(block);
            if (this.head != null && kblock.ParentHash != SHA256Hash.ZERO_HASH)
            {
                KhaosBlock oblock = this.mini_store.GetBlockByHash(kblock.ParentHash);
                if (oblock != null)
                {
                    if (block.Num != oblock.Num + 1)
                    {
                        throw new ArgumentException("parent number : " + oblock.Num + ", block number : " + block.Num);
                    }
                    kblock.Parent = oblock;
                }
                else
                {
                    this.mini_unlinked_store.Insert(kblock);
                    throw new UnLinkedBlockException();
                }
            }

            this.mini_store.Insert(kblock);

            if (this.head == null || block.Num > head.Num)
                SetHead(kblock);

            return this.head.Block;
        }

        public bool Pop()
        {
            KhaosBlock parent = this.head.Parent;
            if (parent != null)
            {
                SetHead(parent);
                return true;
            }

            return false;
        }

        public void SetMaxCapacity(int max_capacity)
        {
            this.mini_store.MaxCapacity = max_capacity;
            this.mini_unlinked_store.MaxCapacity = max_capacity;
        }

        public BlockCapsule GetParent(SHA256Hash hash)
        {
            BlockCapsule result = null;
            KhaosBlock block = this.mini_store.GetBlockByHash(hash);
            if (block == null)
                block = this.mini_unlinked_store.GetBlockByHash(hash);

            if (block != null && block.Block != null && Contain(block.Id))
            {
                result = block.Block;
            }

            return result;
        }

        public KeyValuePair<List<KhaosBlock>, List<KhaosBlock>> GetBranch(SHA256Hash block1, SHA256Hash block2)
        {
            List<KhaosBlock> list1 = new List<KhaosBlock>();
            List<KhaosBlock> list2 = new List<KhaosBlock>();
            KhaosBlock kblock1 = this.mini_store.GetBlockByHash(block1);
            KhaosBlock kblock2 = this.mini_store.GetBlockByHash(block2);
            CheckNull(kblock1);
            CheckNull(kblock2);

            while (kblock1.Num > kblock2.Num)
            {
                list1.Add(kblock1);
                kblock1 = kblock1.Parent;
                CheckNull(kblock1);
                CheckNull(this.mini_store.GetBlockByHash(kblock1.Id));
            }

            while (kblock1.Num < kblock2.Num)
            {
                list2.Add(kblock2);
                kblock2 = kblock2.Parent;
                CheckNull(kblock2);
                CheckNull(this.mini_store.GetBlockByHash(kblock2.Id));
            }

            while (!object.Equals(kblock1, kblock2))
            {
                list1.Add(kblock1);
                list2.Add(kblock2);
                kblock1 = kblock1.Parent;
                CheckNull(kblock1);
                CheckNull(this.mini_store.GetBlockByHash(kblock1.Id));
                kblock2 = kblock2.Parent;
                CheckNull(kblock2);
                CheckNull(this.mini_store.GetBlockByHash(kblock2.Id));
            }

            return new KeyValuePair<List<KhaosBlock>, List<KhaosBlock>>(list1, list2);
        }

        public KeyValuePair<List<KhaosBlock>, List<KhaosBlock>> GetBranch(BlockId block1, BlockId block2)
        {
            List<KhaosBlock> list1 = new List<KhaosBlock>();
            List<KhaosBlock> list2 = new List<KhaosBlock>();
            KhaosBlock kblock1 = this.mini_store.GetBlockByHash(block1);
            KhaosBlock kblock2 = this.mini_store.GetBlockByHash(block2);

            if (kblock1 != null && kblock2 != null)
            {
                while (!object.Equals(kblock1, kblock2))
                {
                    if (kblock1.Num > kblock2.Num)
                    {
                        list1.Add(kblock1);
                        kblock1 = kblock1.Parent;
                    }
                    else if (kblock1.Num < kblock2.Num)
                    {
                        list2.Add(kblock2);
                        kblock2 = kblock2.Parent;
                    }
                    else
                    {
                        list1.Add(kblock1);
                        list2.Add(kblock2);
                        kblock1 = kblock1.Parent;
                        kblock2 = kblock2.Parent;
                    }
                }
            }

            return new KeyValuePair<List<KhaosBlock>, List<KhaosBlock>>(list1, list2);
        }

        #region Override - MineralDatabaseint
        public override bool Contains(byte[] key) { return false; }
        public override void Put(byte[] key, BlockCapsule value) { }
        public override BlockCapsule Get(byte[] key) { return null; }
        public override void Delete(byte[] key) { }
        #endregion
        #endregion
    }
}
