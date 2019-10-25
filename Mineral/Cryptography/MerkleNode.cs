using Mineral.Utils;

namespace Mineral.Cryptography
{
    internal class MerkleNode
    {
        public byte[] Hash;
        public MerkleNode Parent;
        public MerkleNode LeftChild;
        public MerkleNode RightChild;

        public bool IsLeaf => LeftChild == null && RightChild == null;
        public bool IsRoot => Parent == null;
    }
}
