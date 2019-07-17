using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core;
using Mineral.Core.Capsule.Util;
using Mineral.Utils;
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
            KeccakDigest digest = new KeccakDigest(256);
            byte[] output = new byte[digest.GetDigestSize()];
            digest.BlockUpdate(data, 0, data.Length);
            digest.DoFinal(output, 0);

            return output;
        }

        public static byte[] ToAddressSHA3(byte[] input)
        {
            byte[] hash = SHA3(input);
            byte[] address = new byte[hash.Length - 12];
            Array.Copy(hash, 12, address, 0, address.Length);

            return address;
        }
    }
}
