using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Capsule.Util;
using Org.BouncyCastle.Crypto.Digests;

namespace Mineral.Cryptography
{
    public static class Hash
    {
        public static readonly byte[] EMPTY_TRIE_HASH;

        static Hash()
        {
            EMPTY_TRIE_HASH = SHA3(RLP.EncodeElement(new byte[0]));
        }

        public static byte[] SHA3(this byte[] data)
        {
            Sha3Digest sha3 = new Sha3Digest(256);
            sha3.BlockUpdate(data, 0, data.Length);

            byte[] result = new byte[sha3.GetDigestSize()];
            sha3.DoFinal(result, 0);

            return result;
        }
    }
}
