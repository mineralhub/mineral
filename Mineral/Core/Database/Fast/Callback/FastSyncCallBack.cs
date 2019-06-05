using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Capsule;
using Trie.Net.Standard;

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
        private Trie<TrieEntry> tire = 

        private Manager db_manager;
        private AccountStateStoreTrie db = new AccountStateStoreTrie();
        private List<TrieEntry> trie_entry = new List<TrieEntry>();
        #endregion


        #region Property
        #endregion


        #region Constructor
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
        #endregion


        #region External Method
        public void AccountCallBack(byte[] key, AccountCapsule account)
        {
            if (Execute())
                return;

            if (account == null)
                return;

            this.trie_entry.Add(new TrieEntry(key, account.Data));
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
                BlockCapsule parent_block = this.db_manager.blockby
            }
            catch (System.Exception e)
            {
                Logger.Error(e.Message);
            }
        }

        public void PreExecuteTrans()
        {
            this.trie_entry.Clear();
        }

        public void ExecuteTransFinish()
        {
            foreach (TrieEntry entry in trie_entry)
            {
            }
            trie_entry.Clear();
        }
        #endregion
    }
}
