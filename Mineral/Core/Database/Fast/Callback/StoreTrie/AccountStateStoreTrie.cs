using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Capsule;
using Mineral.Core.Capsule.Util;
using Mineral.Core.Database2.Common;
using Mineral.Core.Tire;
using Mineral.Utils;

namespace Mineral.Core.Database.Fast.Callback.StoreTrie
{
    public class AccountStateStoreTrie : MineralStoreWithRevoking<BytesCapsule, object>, IBaseDB<byte[], BytesCapsule>
    {
        #region Field
        private TrieService trie_service = new TrieService();
        #endregion


        #region Property
        public long Size => throw new NotImplementedException();
        public bool IsEmpty => base.Size <= 0;
        #endregion


        #region Contructor
        public AccountStateStoreTrie(string db_name = "accountTrie") : base(db_name) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public AccountStateEntity GetAccount(byte[] key)
        {
            return GetAccount(key, this.trie_service.GetFullAccountStateRootHash());
        }

        public AccountStateEntity GetSolidityAccount(byte[] key)
        {
            return GetAccount(key, this.trie_service.GetSolidityAccountStateRootHash());
        }

        public AccountStateEntity GetAccount(byte[] key, byte[] root_hash)
        {
            Trie trie = new Trie(this, root_hash);
            byte[] value = trie.Get(RLP.EncodeElement(key));

            return value.IsNotNullOrEmpty() ? AccountStateEntity.Parse(value) : null;
        }

        public override BytesCapsule Get(byte[] key)
        {
            return base.GetUnchecked(key);
        }

        public void Remove(byte[] key)
        {
            base.Delete(key);
        }
        #endregion
    }
}
