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
            Sha3Digest sha3 = new Sha3Digest(256);
            sha3.BlockUpdate(data, 0, data.Length);

            byte[] result = new byte[sha3.GetDigestSize()];
            sha3.DoFinal(result, 0);

            return result;
        }

        public static byte[] SHA3omit12(byte[] input)
        {
            byte[] hash = SHA3(input);
            byte[] address = ByteUtil.CopyRange(hash, 11, hash.Length);
            address[0] = Wallet.ADDRESS_PREFIX_BYTES;

            return address;
        }
    }
}
