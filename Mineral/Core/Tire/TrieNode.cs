using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Mineral.Core.Capsule;
using Mineral.Core.Capsule.Util;
using Mineral.Cryptography;

namespace Mineral.Core.Tire
{
    public class TrieNode
    {
        public enum TrieNodeType
        {
            BranchNode,
            KVNodeValue,
            KVNodeNode,
        }

        #region Field
        private static readonly object NULL_NODE = new object();
        private static readonly int MIN_BRANCHES_CONCURRENTLY = 3;

        private object[] children = null;
        private byte[] hash = null;
        private byte[] rlp = null;
        private bool dirty = false;
        private LList parsed_rlp = null;
        private TrieNodeType node_type = TrieNodeType.BranchNode;
        private Trie reference = null;
        #endregion


        #region Property
        public byte[] Hash => this.hash;
        public bool Dirty => this.dirty;

        public TrieNodeType NodeType
        {
            get { return this.node_type; }
            set { this.node_type = value; }
        }
        #endregion


        #region Contructor
        private TrieNode(Trie reference, LList parsed_rlp)
        {
            this.reference = reference;
            this.parsed_rlp = parsed_rlp;
            this.rlp = parsed_rlp.GetEncoded();
        }

        public TrieNode(Trie reference)
        {
            this.reference = reference;
            this.children = new object[17];
            this.dirty = true;
        }

        public TrieNode(Trie reference, TrieKey key, object value_or_node)
            : this (reference, new object[] { key, value_or_node })
        {
            this.reference = reference;
            dirty = true;
        }

        private TrieNode(Trie reference, object[] children)
        {
            this.reference = reference;
            this.children = children;
        }

        public TrieNode(Trie reference, int length)
        {
            this.reference = reference;
            this.children = new object[length];
        }

        public TrieNode(Trie reference, byte[] hash_or_rlp)
        {
            this.reference = reference;
            if (hash_or_rlp.Length == 32)
                this.hash = hash_or_rlp;
            else
                this.rlp = hash_or_rlp;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private void Resolve()
        {
            if (!ResolveCheck())
            {
                Logger.Error("Invalid Trie state, can't resolve hash " + hash.ToHexString());
                throw new System.Exception("Invalid Trie state, can't resolve hash " + hash.ToHexString());
            }
        }

        private byte[] Encode(int depth, bool forceHash)
        {
            if (!dirty)
            {
                return hash != null ? RLP.EncodeElement(hash) : rlp;
            }
            else
            {
                TrieNodeType type = this.node_type;
                byte[] ret;
                if (type == TrieNodeType.BranchNode)
                {
                    if (depth == 1 && this.reference.Async)
                    {
                        // parallelize encode() on the first trie level only and if there are at least
                        // MIN_BRANCHES_CONCURRENTLY branches are modified
                        object[] encoded = new object[17];
                        int encode_count = 0;
                        for (int i = 0; i < 16; i++)
                        {
                            TrieNode child = BranchNodeGetChild(i);
                            if (child == null)
                            {
                                encoded[i] = RLP.EMPTY_ELEMENT_RLP;
                            }
                            else if (!child.dirty)
                            {
                                encoded[i] = child.Encode(depth + 1, false);
                            }
                            else
                            {
                                encode_count++;
                            }
                        }
                        for (int i = 0; i < 16; i++)
                        {
                            if (encoded[i] == null)
                            {
                                TrieNode child = BranchNodeGetChild(i);
                                if (child == null)
                                {
                                    continue;
                                }
                                if (encode_count >= MIN_BRANCHES_CONCURRENTLY)
                                {
                                    encoded[i] = Task.Run<byte[]>(() =>
                                    {
                                        return child.Encode(depth + 1, false);
                                    });
                                }
                                else
                                {
                                    encoded[i] = child.Encode(depth + 1, false);
                                }
                            }
                        }
                        byte[] value = BranchNodeGetValue();
                        encoded[16] = RLP.EncodeElement(value);
                        try
                        {
                            ret = EncodeRlpListTaskResult(encoded);
                        }
                        catch (System.Exception e)
                        {
                            throw new System.Exception(e.Message);
                        }
                    }
                    else
                    {
                        byte[][] encoded = new byte[17][];
                        for (int i = 0; i < 16; i++)
                        {
                            TrieNode child = BranchNodeGetChild(i);
                            encoded[i] = child == null ? RLP.EMPTY_ELEMENT_RLP : child.Encode(depth + 1, false);
                        }
                        byte[] value = BranchNodeGetValue();
                        encoded[16] = RLP.EncodeElement(value);
                        ret = RLP.EncodeList(encoded);
                    }
                }
                else if (type == TrieNodeType.KVNodeNode)
                {
                    ret = RLP.EncodeList(RLP.EncodeElement(KVNodeGetKey().ToPacked()),
                        KVNodeGetChildNode().Encode(depth + 1, false));
                }
                else
                {
                    byte[] value = KVNodeGetValue();
                    ret = RLP.EncodeList(RLP.EncodeElement(KVNodeGetKey().ToPacked()),
                        RLP.EncodeElement(value == null ? new byte[0] : value));
                }
                if (this.hash != null)
                {
                    this.reference.DeleteHash(this.hash);
                }

                this.dirty = false;
                if (ret.Length < 32 && !forceHash)
                {
                    this.rlp = ret;
                    return ret;
                }
                else
                {
                    this.hash = ret.SHA3();
                    this.reference.AddHash(this.hash, ret);
                    return RLP.EncodeElement(hash);
                }
            }
        }

        private byte[] EncodeRlpListTaskResult(params object[] list)
        {
            byte[][] vals = new byte[list.Length][];
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i] is Task<byte[]>)
                {
                    vals[i] = ((Task<byte[]>)list[i]).Result;
                }
                else
                {
                    vals[i] = (byte[])list[i];
                }
            }

            return RLP.EncodeList(vals);
        }

        private void Parse()
        {
            if (this.children != null)
                return;

            Resolve();

            LList list = this.parsed_rlp == null ? RLP.DecodeLazyList(this.rlp) : this.parsed_rlp;

            if (list != null && list.Count == 2)
            {
                this.children = new object[2];
                TrieKey key = TrieKey.FromPacked(list.GetBytes(0));
                this.children[0] = key;
                if (key.IsTerminal)
                {
                    this.children[1] = list.GetBytes(1);
                }
                else
                {
                    this.children[1] = list.IsList(1) ? new TrieNode(this.reference, list.GetList(1)) : new TrieNode(this.reference, list.GetBytes(1));
                }
            }
            else
            {
                this.children = new object[17];
                this.parsed_rlp = list;
            }
        }
        #endregion


        #region External Method
        public bool ResolveCheck()
        {
            if (this.rlp != null || this.parsed_rlp != null || this.hash == null)
            {
                return true;
            }
            this.rlp = this.reference.GetHash(this.hash);

            return this.rlp != null;
        }

        public byte[] Encode()
        {
            return Encode(1, true);
        }

        public TrieNode BranchNodeGetChild(int hex)
        {
            Parse();

            object child = this.children[hex];
            if (child == null && this.parsed_rlp != null)
            {
                if (this.parsed_rlp.IsList(hex))
                {
                    child = new TrieNode(this.reference, this.parsed_rlp.GetList(hex));
                }
                else
                {
                    byte[] bytes = this.parsed_rlp.GetBytes(hex);
                    if (bytes.Length == 0)
                    {
                        child = NULL_NODE;
                    }
                    else
                    {
                        child = new TrieNode(this.reference, bytes);
                    }
                }
                this.children[hex] = child;
            }

            return child == NULL_NODE ? null : (TrieNode)child;
        }

        public TrieNode BranchNodeSetChild(int hex, TrieNode node)
        {
            Parse();
            this.children[hex] = node == null ? NULL_NODE : node;
            this.dirty = true;

            return this;
        }

        public byte[] BranchNodeGetValue()
        {
            Parse();
            object child = children[16];
            if (child == null && this.parsed_rlp != null)
            {
                byte[] bytes = this.parsed_rlp.GetBytes(16);
                if (bytes.Length == 0)
                {
                    child = NULL_NODE;
                }
                else
                {
                    child = bytes;
                }
                this.children[16] = child;
            }

            return child == NULL_NODE ? null : (byte[])child;
        }

        public TrieNode BranchNodeSetValue(byte[] val)
        {
            Parse();
            this.children[16] = val == null ? NULL_NODE : val;
            this.dirty = true;

            return this;
        }

        public int BranchNodeCompactIndex()
        {
            Parse();
            int count = 0;
            int index = -1;
            for (int i = 0; i < 16; i++)
            {
                if (BranchNodeGetChild(i) != null)
                {
                    count++;
                    index = i;
                    if (count > 1)
                    {
                        return -1;
                    }
                }
            }

            return count > 0 ? index : (BranchNodeGetValue() == null ? -1 : 16);
        }

        public bool BranchNodeCanCompact()
        {
            Parse();
            int count = 0;
            for (int i = 0; i < 16; i++)
            {
                count += BranchNodeGetChild(i) == null ? 0 : 1;
                if (count > 1)
                {
                    return false;
                }
            }

            return count == 0 || BranchNodeGetValue() == null;
        }

        public TrieKey KVNodeGetKey()
        {
            Parse();

            return (TrieKey)this.children[0];
        }

        public TrieNode KVNodeGetChildNode()
        {
            Parse();

            return (TrieNode)this.children[1];
        }

        public byte[] KVNodeGetValue()
        {
            Parse();

            return (byte[])this.children[1];
        }

        public TrieNode KVNodeSetValue(byte[] value)
        {
            Parse();
            this.children[1] = value;
            this.dirty = true;

            return this;
        }

        public object KVNodeGetValueOrNode()
        {
            Parse();

            return this.children[1];
        }

        public TrieNode KVNodeSetValueOrNode(object value_or_node)
        {
            Parse();
            this.children[1] = value_or_node;
            this.dirty = true;

            return this;
        }

        public void Dispose()
        {
            if (this.hash != null)
            {
                this.reference.DeleteHash(this.hash);
            }
        }

        public TrieNode Invalidate()
        {
            this.dirty = true;

            return this;
        }
        #endregion
    }
}
