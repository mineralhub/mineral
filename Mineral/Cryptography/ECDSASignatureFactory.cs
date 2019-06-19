using System;
using System.Collections.Generic;
using System.Text;
using Org.BouncyCastle.Math;

namespace Mineral.Cryptography
{
    public class ECDSASignatureFactory
    {
        public static ECDSASignature FromComponents(byte[] r, byte[] s)
        {
            return new ECDSASignature(new BigInteger(1, r), new BigInteger(1, s));
        }

        public static ECDSASignature FromComponents(byte[] r, byte[] s, byte v)
        {
            var signature = FromComponents(r, s);
            signature.V = new[] { v };
            return signature;
        }

        public static ECDSASignature FromComponents(byte[] r, byte[] s, byte[] v)
        {
            var signature = FromComponents(r, s);
            signature.V = v;
            return signature;
        }

        public static ECDSASignature FromComponents(byte[] rs)
        {
            var r = new byte[32];
            var s = new byte[32];
            Array.Copy(rs, 0, r, 0, 32);
            Array.Copy(rs, 32, s, 0, 32);
            var signature = FromComponents(r, s);
            return signature;
        }

        public static ECDSASignature ExtractECDSASignature(string signature)
        {
            var signatureArray = signature.HexToBytes();
            return ExtractECDSASignature(signatureArray);
        }

        public static ECDSASignature ExtractECDSASignature(byte[] signatureArray)
        {
            var v = signatureArray[64];

            if (v == 0 || v == 1)
                v = (byte)(v + 27);

            var r = new byte[32];
            Array.Copy(signatureArray, r, 32);
            var s = new byte[32];
            Array.Copy(signatureArray, 32, s, 0, 32);

            return ECDSASignatureFactory.FromComponents(r, s, v);
        }

        public static bool IsLessThan(BigInteger value1, BigInteger value2)
        {
            return value1.CompareTo(value2) < 0;
        }

        public static bool ValidateComponents(BigInteger r, BigInteger s, byte v)
        {
            if (v != 27 && v != 28)
            {
                return false;
            }

            if (IsLessThan(r, BigInteger.One))
            {
                return false;
            }
            if (IsLessThan(s, BigInteger.One))
            {
                return false;
            }

            if (!IsLessThan(r, ECDSASignature.SECP256K1N))
            {
                return false;
            }
            return IsLessThan(s, ECDSASignature.SECP256K1N);
        }
    }
}
