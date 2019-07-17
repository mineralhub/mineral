using Mineral.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mineral.Cryptography
{
    public class MerkleTree
    {
        #region Field
        private MerkleNode root = null;
        #endregion


        #region Property
        public byte[] RootHash
        {
            get { return root != null ? root.Hash : new byte[0]; }
        }
        #endregion


        #region Contructor
        public MerkleTree(byte[][] hashes)
        {
            if (hashes.Length == 0)
                return;

            root = Build(hashes.Select(p => new MerkleNode { Hash = p }).ToArray());
        }

        public MerkleTree(List<byte[]> hashes)
        {
            if (hashes.Count == 0)
                return;

            root = Build(hashes.Select(p => new MerkleNode { Hash = p }).ToArray());
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private static MerkleNode Build(MerkleNode[] leaves)
        {
            if (leaves.Length == 0)
                throw new ArgumentException();
            if (leaves.Length == 1)
                return leaves[0];

            MerkleNode[] parents = new MerkleNode[(leaves.Length + 1) / 2];
            for (int i = 0; i < parents.Length; ++i)
            {
                parents[i] = new MerkleNode();
                parents[i].LeftChild = leaves[i * 2];
                leaves[i * 2].Parent = parents[i];
                if (i * 2 + 1 == leaves.Length)
                {
                    parents[i].RightChild = parents[i].LeftChild;
                }
                else
                {
                    parents[i].RightChild = leaves[i * 2 + 1];
                    leaves[i * 2 + 1].Parent = parents[i];
                }
                parents[i].Hash = parents[i].LeftChild.Hash.Concat(parents[i].RightChild.Hash).ToArray().DoubleSHA256();
            }

            return Build(parents);
        }
        #endregion


        #region External Method
        #endregion
    }
}
