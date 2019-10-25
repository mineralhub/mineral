using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Tire
{
    public interface IScanAction
    {
        void OnNode(byte[] hash, TrieNode node);
        void OnValue(byte[] node_hash, TrieNode node, byte[] key, byte[] value);
    }
}
