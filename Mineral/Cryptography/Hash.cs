using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Mineral.Core;
using Mineral.Core.Capsule.Util;
using Mineral.Utils;
using Org.BouncyCastle.Crypto.Digests;

namespace Mineral.Cryptography
{
    public static class Hash
    {
        private static ThreadLocal<SHA256> sha256 = new ThreadLocal<SHA256>(() => System.Security.Cryptography.SHA256.Create());
        private static ThreadLocal<RIPEMD160Managed> ripemd160 = new ThreadLocal<RIPEMD160Managed>(() => new RIPEMD160Managed());
        public static readonly byte[] EMPTY_TRIE_HASH;

        static Hash()
        {
            EMPTY_TRIE_HASH = SHA3(RLP.EncodeElement(new byte[0]));
        }

        public static byte[] SHA256(this byte[] data)
        {
            return sha256.Value.ComputeHash(data);
        }

        public static byte[] SHA256(this byte[] data, int offset, int count)
        {
            return sha256.Value.ComputeHash(data, offset, count);
        }

        public static byte[] DoubleSHA256(this byte[] data)
        {
            return data.SHA256().SHA256();
        }

        public static byte[] DoubleSHA256(this byte[] data, int offset, int count)
        {
            return SHA256(data, offset, count).SHA256();
        }

        public static byte[] RIPEMD160(this byte[] data)
        {
            return ripemd160.Value.ComputeHash(data);
        }

        public static byte[] SHA3(this byte[] data)
        {
            KeccakDigest digest = new KeccakDigest(256);
            byte[] output = new byte[digest.GetDigestSize()];
            digest.BlockUpdate(data, 0, data.Length);
            digest.DoFinal(output, 0);

            return output;
        }

        public static byte[] ToAddress(byte[] input)
        {
            byte[] hash = Hash.SHA3(input);
            byte[] address = new byte[hash.Length - 11];


            address[0] = Wallet.ADDRESS_PREFIX_BYTES;
            Array.Copy(hash, 12, address, 1, address.Length - 1);

            return address;
        }
    }
}
