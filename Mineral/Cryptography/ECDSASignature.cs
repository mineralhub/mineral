using System;
using System.IO;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Math;

namespace Mineral.Cryptography
{
    public class ECDSASignature
    {
        #region Field
        private const string INVALID_MESSAGE = "Invalid DER signature";
        public static readonly BigInteger SECP256K1N = new BigInteger("fffffffffffffffffffffffffffffffebaaedce6af48a03bbfd25e8cd0364141", 16);

        #endregion


        #region Property
        public BigInteger R { get; }
        public BigInteger S { get; }
        public byte V { get; set; }

        public bool IsLowS
        {
            get { return S.CompareTo(ECKey.HALF_CURVE_ORDER) <= 0; }
        }

        public bool IsValidComponents
        {
            get { return IsValidateComponents(R, S, V); }
        }
        #endregion


        #region Contructor
        public ECDSASignature(BigInteger r, BigInteger s)
        {
            R = r;
            S = s;
        }

        public ECDSASignature(BigInteger[] rs)
        {
            R = rs[0];
            S = rs[1];
        }

        public ECDSASignature(byte[] der_signature)
        {
            try
            {
                var decoder = new Asn1InputStream(der_signature);
                var seq = decoder.ReadObject() as DerSequence;
                if (seq == null || seq.Count != 2)
                    throw new FormatException(INVALID_MESSAGE);
                R = ((DerInteger)seq[0]).Value;
                S = ((DerInteger)seq[1]).Value;
            }
            catch (Exception ex)
            {
                throw new FormatException(INVALID_MESSAGE, ex);
            }
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public static bool IsLessThan(BigInteger value1, BigInteger value2)
        {
            return value1.CompareTo(value2) < 0;
        }

        public static bool IsValidateComponents(BigInteger r, BigInteger s, byte v)
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

        public static bool IsValidDER(byte[] bytes)
        {
            try
            {
                FromDER(bytes);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static ECDSASignature FromComponents(byte[] r, byte[] s)
        {
            return new ECDSASignature(new BigInteger(1, r), new BigInteger(1, s));
        }

        public static ECDSASignature FromComponents(byte[] r, byte[] s, byte v)
        {
            ECDSASignature signature = FromComponents(r, s);
            signature.V = v;

            return signature;
        }

        public static ECDSASignature FromComponents(byte[] rs)
        {
            byte[] r = new byte[32];
            byte[] s = new byte[32];

            Array.Copy(rs, 0, r, 0, 32);
            Array.Copy(rs, 32, s, 0, 32);

            return FromComponents(r, s);
        }

        public static ECDSASignature ExtractECDSASignature(string signature)
        {
            var signatureArray = signature.HexToBytes();

            return ExtractECDSASignature(signatureArray);
        }

        public static ECDSASignature ExtractECDSASignature(byte[] signature)
        {
            var v = signature[64];

            if (v == 0 || v == 1)
                v = (byte)(v + 27);

            byte[] r = new byte[32];
            Array.Copy(signature, r, 32);

            byte[] s = new byte[32];
            Array.Copy(signature, 32, s, 0, 32);

            return FromComponents(r, s, v);
        }

        public static ECDSASignature FromDER(byte[] signatrue)
        {
            return new ECDSASignature(signatrue);
        }

        public ECDSASignature MakeCanonicalised()
        {
            if (!IsLowS)
                return new ECDSASignature(R, ECKey.CURVE_ORDER.Subtract(S));

            return this;
        }

        public byte[] ToDER()
        {
            var bos = new MemoryStream(72);
            var seq = new DerSequenceGenerator(bos);

            seq.AddObject(new DerInteger(R));
            seq.AddObject(new DerInteger(S));
            seq.Close();

            return bos.ToArray();
        }

        public byte[] ToByteArray()
        {
            byte fixed_v = V >= 27 ? (byte)(V - 27) : V;

            byte[] result = new byte[65];
            Array.Copy(R.ToByteArray(), 0, result, 0, 32);
            Array.Copy(S.ToByteArray(), 0, result, 32, 32);
            result[64] = fixed_v;

            return result;
        }
        #endregion
    }
}
