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


        #region Override - MineralDatabase
        public override bool Contains(byte[] key) { return false; }
        public override void Put(byte[] key, BlockCapsule value) { }
        public override BlockCapsule Get(byte[] key) { return null; }
        public override void Delete(byte[] key) { }
        #endregion
        #endregion
    }
}
