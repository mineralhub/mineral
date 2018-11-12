using System;
using System.Linq;

namespace Mineral.Cryptography
{
    public class MerkleTree
    {
        private MerkleTreeNode _root;
        public UInt256 RootHash => _root.Hash;

        public MerkleTree(UInt256[] hashes)
        {
            if (hashes.Length == 0)
                throw new ArgumentException();

            _root = Build(hashes.Select(p => new MerkleTreeNode { Hash = p }).ToArray());
        }

        private static MerkleTreeNode Build(MerkleTreeNode[] leaves)
        {
            if (leaves.Length == 0)
                throw new ArgumentException();
            if (leaves.Length == 1)
                return leaves[0];

            MerkleTreeNode[] parents = new MerkleTreeNode[(leaves.Length + 1) / 2];
            for (int i = 0; i < parents.Length; ++i)
            {
                parents[i] = new MerkleTreeNode();
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
                parents[i].Hash = new UInt256(parents[i].LeftChild.Hash.Data.Concat(parents[i].RightChild.Hash.Data).ToArray().DoubleSHA256());
            }
            return Build(parents);
        }
    }
}
