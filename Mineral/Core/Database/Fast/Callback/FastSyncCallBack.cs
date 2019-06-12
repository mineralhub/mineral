using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf;
using Mineral.Core.Capsule;
using Mineral.Core.Capsule.Util;
using Mineral.Core.Database.Fast.Callback.StoreTrie;
using Mineral.Core.Exception;
using Mineral.Core.Tire;
using Mineral.Cryptography;
using Mineral.Utils;

namespace Mineral.Core.Database.Fast.Callback
{
    public class FastSyncCallBack
    {
        private class TrieEntry
        {
            public byte[] Key { get; set; } = null;
            public byte[] Data { get; set; } = null;

            public TrieEntry(byte[] key, byte[] data)
            {
                this.Key = key;
                this.Data = data;
            }
        }

        #region Field
        BlockCapsule block = null;
        private volatile bool execute = false;
        private volatile bool allow_generate_root = false;
        private Trie trie = null;

        private Manager db_manager;
        private AccountStateStoreTrie db = new AccountStateStoreTrie();
        private List<TrieEntry> entry_list = new List<TrieEntry>();
        #endregion


        #region Property
        #endregion


        #region Constructor
        public FastSyncCallBack(Manager db_manager)
        {
            this.db_manager = db_manager;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private bool Execute()
        {
            if (!execute || !allow_generate_root)
            {
                this.execute = false;
                return false;
            }

            return true;
        }

        private void PrintErrorLog(Trie trie)
        {
            trie.ScanTree(new ScanAction());
        }
        #endregion


        #region External Method
        public void AccountCallBack(byte[] key, AccountCapsule account)
        {
            if (Execute())
                return;

            if (account == null)
                return;

            this.entry_list.Add(new TrieEntry(key, account.Data));
        }

        public void PreExecute(BlockCapsule block)
        {
            this.block = block;
            this.execute = true;
            this.allow_generate_root = this.db_manager.DynamicProperties.AllowAccountStateRoot();

            if (!Execute())
                return;

            byte[] root_hash = null;
            try
            {
                BlockCapsule parent_block = this.db_manager.GetBlockById(this.block.ParentId);
                root_hash = parent_block.Instance.BlockHeader.RawData.AccountStateRoot.ToByteArray();
            }
            catch (System.Exception e)
            {
                Logger.Error(e.Message);
            }

            if (root_hash.SequenceEqual(new byte[0]))
            {
                root_hash = Hash.EMPTY_TRIE_HASH;
            }

            trie = new Trie(this.db, root_hash);
        }

        public void PreExecuteTrans()
        {
            this.entry_list.Clear();
        }

        public void ExecuteTransFinish()
        {
            foreach (TrieEntry entry in entry_list)
            {
                this.trie.Put(RLP.EncodeElement(entry.Key), entry.Data);
            }
            entry_list.Clear();
        }

        public void ExecutePushFinish()
        {
            if (!Execute())
                return;

            ByteString old_root = this.block.Instance.BlockHeader.RawData.AccountStateRoot;
            this.execute = false;

            byte[] new_root = this.trie.GetRootHash();
            if (new_root.IsNullOrEmpty())
            {
                new_root = Hash.EMPTY_TRIE_HASH;
            }

            if (old_root.IsNotNullOrEmpty() && !old_root.SequenceEqual(new_root))
            {
                Logger.Error(string.Format(
                    "The AccountStateRoot hash is error. {0}, oldRoot: {1}, newRoot: {2}",
                    this.block.Id.ToString(),
                    old_root.ToByteArray().ToHexString(),
                    new_root.ToHexString()));
                PrintErrorLog(trie);
                throw new BadBlockException("The AccountStateRoot hash is error");
            }
        }

        public void ExecuteGenerateFinish()
        {
            if (!Execute())
                return;

            byte[] new_root = this.trie.GetRootHash();
            if (new_root.IsNullOrEmpty())
            {
                new_root = Hash.EMPTY_TRIE_HASH;
            }

            this.block.SetAccountStateRoot(new_root);
            this.execute = false;
        }

        public void DeleteAccount(byte[] key)
        {
            if (!Execute())
                return;

            this.trie.Delete(RLP.EncodeElement(key));
        }
        #endregion
    }
}
