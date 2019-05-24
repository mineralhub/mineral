using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Parameters;
using Mineral.Utils;

namespace Mineral.Cryptography
{
    public static class Helper
    {
        private static ThreadLocal<SHA256> _sha256 = new ThreadLocal<SHA256>(() => System.Security.Cryptography.SHA256.Create());
        private static ThreadLocal<RIPEMD160Managed> _ripemd160 = new ThreadLocal<RIPEMD160Managed>(() => new RIPEMD160Managed());

        public static byte[] SHA256(this byte[] data)
        {
            return _sha256.Value.ComputeHash(data);
        }

        public static byte[] SHA256(this byte[] data, int offset, int count)
        {
            return _sha256.Value.ComputeHash(data, offset, count);
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
            return _ripemd160.Value.ComputeHash(data.ToArray());
        }

        public static UInt256 GetHash(this ISerializable value)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                try
                {
                    value.Serialize(bw);
                    bw.Flush();
                    return new UInt256(ms.ToArray().DoubleSHA256());
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public static UInt256 GetHash(this IVerifiable value)
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                try
                {
                    value.Serialize(bw);
                    bw.Flush();
                    return new UInt256(ms.ToArray().DoubleSHA256());
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public static string Base58CheckEncode(this byte[] data)
        {
            byte[] checksum = data.DoubleSHA256();
            byte[] buffer = new byte[data.Length + 4];
            Buffer.BlockCopy(data, 0, buffer, 0, data.Length);
            Buffer.BlockCopy(checksum, 0, buffer, data.Length, 4);
            return Base58.Encode(buffer);
        }

        public static byte[] Base58CheckDecode(this string input)
        {
            byte[] buffer = Base58.Decode(input);
            if (buffer.Length < 4)
                throw new FormatException();
            byte[] checksum = buffer.SHA256(0, buffer.Length - 4).SHA256();
            if (!buffer.Skip(buffer.Length - 4).SequenceEqual(checksum.Take(4)))
                throw new FormatException();
            return buffer.Take(buffer.Length - 4).ToArray();
        }

        public static uint Murmur32(this IEnumerable<byte> value, uint seed)
        {
            using (Murmur3 murmur = new Murmur3(seed))
            {
                return murmur.ComputeHash(value.ToArray()).ToUInt32(0);
            }
        }

        public static byte[] Sign(byte[] message, byte[] prikey)
        {
            return Sign(message, new ECKey(prikey, true));
        }

        public static byte[] Sign(byte[] message, ECKey key)
        {
            ISigner signer = SignerUtilities.GetSigner("NONEwithECDSA");
            signer.Init(true, key.PrivateKey);
            signer.BlockUpdate(message, 0, message.Length);
            return signer.GenerateSignature();
        }

        public static bool VerifySignature(byte[] signature, byte[] message, byte[] pubkey)
        {
            return VerifySignature(signature, message, new ECKey(pubkey, false));
        }

        //public static bool VerifySignature(MakerSignature makerSign, byte[] message)
        //{
        //    return VerifySignature(makerSign.Signature, message, new ECKey(makerSign.Pubkey, false));
        //}

        public static bool VerifySignature(byte[] signature, byte[] message, ECKey key)
        {
            ISigner signer = SignerUtilities.GetSigner("NONEwithECDSA");
            signer.Init(false, key.PublicKey);
            signer.BlockUpdate(message, 0, message.Length);
            return signer.VerifySignature(signature);
        }

        public static byte[] ToByteArray(this ECPublicKeyParameters pubkey, bool isCompressed = false)
        {
            var q = pubkey.Q.Normalize();
            return ECKey.Secp256k1.Curve.CreatePoint(q.XCoord.ToBigInteger(), q.YCoord.ToBigInteger()).GetEncoded(isCompressed);
        }
    }
}
