using System;
using System.Linq;
using Mineral.Core.Capsule;
using Mineral.Core.Database2.Common;
using Mineral.Cryptography;

namespace Mineral.Core.Tire
{
    public class Trie : ITrie<byte[]>
    {
        #region Field
        private IBaseDB<byte[], BytesCapsule> cache = null;
        private TrieNode root = null;
        private bool async = true;
        #endregion


        #region Property
        private bool HasRoot
        {
            get { return this.root != null && this.root.ResolveCheck(); }
        }

        public IBaseDB<byte[], BytesCapsule> Cache
        {
            get { return this.cache; }
        }

        public bool Async
        {
            get { return this.async; }
            set { this.async = value; }
        }
        #endregion


        #region Contructor
        public Trie() : this((byte[])null) { }
        public Trie(byte[] root) : this(new ConcurrentHashDB(), root) { }
        public Trie(IBaseDB<byte[], BytesCapsule> cache) : this(cache, null) { }
        public Trie(IBaseDB<byte[], BytesCapsule> cache, byte[] root)
        {
            this.cache = cache;
            SetRoot(root);
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private void Encode()
        {
            if (this.root != null)
            {
                root.Encode();
            }
        }

        private TrieNode Insert(TrieNode node, TrieKey key, object node_or_value)
        {
            if (node.NodeType == TrieNode.TrieNodeType.BranchNode)
            {
                if (key.IsEmpty)
                {
                    return node.BranchNodeSetValue((byte[])node_or_value);
                }
                TrieNode child = node.BranchNodeGetChild(key.GetHex(0));
                if (child != null)
                {
                    return node.BranchNodeSetChild(key.GetHex(0), Insert(child, key.Shift(1), node_or_value));
                }
                else
                {
                    TrieKey child_key = key.Shift(1);
                    TrieNode new_child;
                    if (!child_key.IsEmpty)
                    {
                        new_child = new TrieNode(this, child_key, node_or_value);
                    }
                    else
                    {
                        new_child = node_or_value is TrieNode ? (TrieNode)node_or_value : new TrieNode(this, child_key, node_or_value);
                    }
                    return node.BranchNodeSetChild(key.GetHex(0), new_child);
                }
            }
            else
            {
                TrieKey current_node_key = node.KVNodeGetKey();
                TrieKey common_prefix = key.GetCommonPrefix(current_node_key);
                if (common_prefix.IsEmpty)
                {
                    TrieNode new_branch = new TrieNode(this);
                    Insert(new_branch, current_node_key, node.KVNodeGetValueOrNode());
                    Insert(new_branch, key, node_or_value);
                    node.Dispose();
                    return new_branch;
                }
                else if (common_prefix.Equals(key))
                {
                    return node.KVNodeSetValueOrNode(node_or_value);
                }
                else if (common_prefix.Equals(current_node_key))
                {
                    Insert(node.KVNodeGetChildNode(), key.Shift(common_prefix.GetLength()), node_or_value);
                    return node.Invalidate();
                }
                else
                {
                    TrieNode new_branch = new TrieNode(this);
                    TrieNode newKvNode = new TrieNode(this, common_prefix, new_branch);

                    Insert(newKvNode, current_node_key, node.KVNodeGetValueOrNode());
                    Insert(newKvNode, key, node_or_value);
                    node.Dispose();
                    return newKvNode;
                }
            }
        }
        #endregion


        #region External Method
        public void AddHash(byte[] hash, byte[] result)
        {
            this.cache.Put(hash, new BytesCapsule(result));
        }

        public void DeleteHash(byte[] hash)
        {
            this.cache.Remove(hash);
        }

        public byte[] GetHash(byte[] hash)
        {
            BytesCapsule bytes = this.cache.Get(hash);
            return bytes == null ? null : bytes.Data;
        }

        public byte[] GetRootHash()
        {
            Encode();
            return this.root != null ? this.root.Hash : Hash.EMPTY_TRIE_HASH;
        }

        public void SetRoot(byte[] root)
        {
            if (this.root != null && root.SequenceEqual(Hash.EMPTY_TRIE_HASH))
            {
                this.root = new TrieNode(this, root);
            }
            else
            {
                this.root = null;
            }
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void Put(byte[] key, byte[] value)
        {
            TrieKey k = TrieKey.FromNormal(key);
            if (this.root == null)
            {
                if (value != null && value.Length > 0)
                    this.root = new TrieNode(this, k, value);
            }
            else
            {
                if (value == null || value.Length == 0)
                    this.root = Delete(this.root, k);
                else
                    this.root = Insert(root, k, value);
            }
        }

        public byte[] Get(byte[] key)
        {
            if (!HasRoot)
                return null;

            return Get(root, TrieKey.FromNormal(key));
        }

        public byte[] Get(TrieNode node, TrieKey key)
        {
            if (node == null)
                return null;

            if (node.NodeType == TrieNode.TrieNodeType.BranchNode)
            {
                if (key.IsEmpty)
                {
                    return node.BranchNodeGetValue();
                }
                TrieNode child = node.BranchNodeGetChild(key.GetHex(0));
                return Get(child, key.Shift(1));
            }
            else
            {
                TrieKey k = key.MatchAndShift(node.KVNodeGetKey());
                if (k == null)
                    return null;

                if (node.NodeType == TrieNode.TrieNodeType.KVNodeValue)
                    return k.IsEmpty ? node.KVNodeGetValue() : null;
                else
                    return Get(node.KVNodeGetChildNode(), k);
            }
        }

        public void Delete(byte[] key)
        {
            TrieKey k = TrieKey.FromNormal(key);
            if (this.root != null)
            {
                this.root = Delete(this.root, k);
            }
        }

        public TrieNode Delete(TrieNode node, TrieKey key)
        {
            TrieNode kv_node = null;

            if (node.NodeType == TrieNode.TrieNodeType.BranchNode)
            {
                if (key.IsEmpty)
                {
                    node.BranchNodeSetValue(null);
                }
                else
                {
                    int index = key.GetHex(0);
                    TrieNode child = node.BranchNodeGetChild(index);
                    if (child == null)
                        return node;

                    TrieNode new_node = Delete(child, key.Shift(1));
                    node.BranchNodeSetChild(index, new_node);
                    if (new_node != null)
                        return node;
                }

                int compact_index = node.BranchNodeCompactIndex();
                if (compact_index < 0)
                    return node;

                node.Dispose();
                if (compact_index == 16)
                    return new TrieNode(this, TrieKey.Empty(true), node.BranchNodeGetValue());
                else
                    kv_node = new TrieNode(this, TrieKey.SingleHex(compact_index), node.BranchNodeGetChild(compact_index));
            }
            else
            {
                TrieKey k = key.MatchAndShift(node.KVNodeGetKey());
                if (k == null)
                {
                    return node;
                }
                else if (node.NodeType == TrieNode.TrieNodeType.KVNodeValue)
                {
                    if (k.IsEmpty)
                    {
                        node.Dispose();
                        return null;
                    }
                    else
                    {
                        return node;
                    }
                }
                else
                {
                    TrieNode child = Delete(node.KVNodeGetChildNode(), k);
                    if (child == null)
                        throw new System.Exception("Shouldn't happen");

                    kv_node = node.KVNodeSetValueOrNode(child);
                }
            }

            TrieNode new_child = kv_node.KVNodeGetChildNode();
            if (new_child.NodeType != TrieNode.TrieNodeType.BranchNode)
            {
                TrieKey new_key = kv_node.KVNodeGetKey().Concat(new_child.KVNodeGetKey());
                TrieNode new_node = new TrieNode(this, new_key, new_child.KVNodeGetValueOrNode());
                new_child.Dispose();
                kv_node.Dispose();

                return new_node;
            }
            else
            {
                return kv_node;
            }
        }

        public bool Flush()
        {
            bool result = this.root != null && this.root.Dirty;
            if (result)
            {
                Encode();
                this.root = new TrieNode(this, this.root.Hash);
            }

            return result;
        }

        public void ScanTree(IScanAction scan_action)
        {
            ScanTree(this.root, TrieKey.Empty(false), scan_action);
        }

        public void ScanTree(TrieNode node, TrieKey key, IScanAction scan_action)
        {
            if (node != null)
            {
                if (node.Hash != null)
                {
                    scan_action.OnNode(node.Hash, node);
                }

                if (node.NodeType == TrieNode.TrieNodeType.BranchNode)
                {
                    if (node.BranchNodeGetValue() != null)
                    {
                        scan_action.OnValue(node.Hash, node, key.ToNormal(), node.BranchNodeGetValue());
                    }

                    for (int i = 0; i < 16; i++)
                    {
                        ScanTree(node.BranchNodeGetChild(i), key.Concat(TrieKey.SingleHex(i)), scan_action);
                    }
                }
                else if (node.NodeType == TrieNode.TrieNodeType.KVNodeNode)
                {
                    ScanTree(node.KVNodeGetChildNode(), key.Concat(node.KVNodeGetKey()), scan_action);
                }
                else
                {
                    scan_action.OnValue(node.Hash, node, key.Concat(node.KVNodeGetKey()).ToNormal(), node.KVNodeGetValue());
                }
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;

            if (obj == null || this.GetType() != obj.GetType())
                return false;

            Trie trie = (Trie)obj;

            return GetRootHash().SequenceEqual(trie.GetRootHash());
        }
        #endregion
    }
}
