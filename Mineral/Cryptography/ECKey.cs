using System;
using System.Linq;
using System.Text;
using Mineral.Core;
using Mineral.Cryptography;
using Mineral.Utils;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;

namespace Mineral.Cryptography
{
    public class ECKey
    {
        #region Field
        public static readonly BigInteger HALF_CURVE_ORDER;
        public static readonly BigInteger CURVE_ORDER;
        public static readonly ECDomainParameters CURVE;
        public static readonly X9ECParameters secp256k1;
        private static readonly ECKeyPairGenerator generator;

        private readonly ECKeyParameters key;
        private static ECDomainParameters domain_parameter;
        #endregion


        #region Property
        public static X9ECParameters Secp256k1
        {
            get { return secp256k1; }
        }

        public static ECDomainParameters DomainParameter
        {
            get
            {
                if (domain_parameter == null)
                    domain_parameter = new ECDomainParameters(Secp256k1.Curve, Secp256k1.G, Secp256k1.N, Secp256k1.H);

                return domain_parameter;
            }
        }

        public ECPrivateKeyParameters PrivateKeyParameter
        {
            get
            {
                if (this.key is ECPrivateKeyParameters)
                {
                    return this.key as ECPrivateKeyParameters;
                }

                return null;
            }
        }

        public ECPublicKeyParameters PublicKeyParameter
        {
            get
            {
                if (this.key is ECPublicKeyParameters)
                {
                    return this.key as ECPublicKeyParameters;
                }

                return new ECPublicKeyParameters("EC", Secp256k1.G.Multiply(PrivateKeyParameter.D), DomainParameter);
            }
        }

        public byte[] PrivateKey
        {
            get
            {
                byte[] result = null;
                if (this.key is ECPrivateKeyParameters)
                {
                    result = ((ECPrivateKeyParameters)this.key).D.ToByteArrayUnsigned();
                }

                return result;
            }
        }

        public byte[] PublicKey
        {
            get
            {
                var q = PublicKeyParameter.Q.Normalize();

                return Secp256k1.Curve.CreatePoint(q.XCoord.ToBigInteger(), q.YCoord.ToBigInteger()).GetEncoded(false);
            }
        }

        public byte[] PublicKeyCompressed
        {
            get
            {
                var q = PublicKeyParameter.Q.Normalize();

                return Secp256k1.Curve.CreatePoint(q.XCoord.ToBigInteger(), q.YCoord.ToBigInteger()).GetEncoded(true);
            }
        }
        #endregion


        #region Contructor
        static ECKey()
        {
            secp256k1 = SecNamedCurves.GetByName("secp256k1");
            generator = new ECKeyPairGenerator("EC");
            CURVE = new ECDomainParameters(secp256k1.Curve, secp256k1.G, secp256k1.N, secp256k1.H);
            HALF_CURVE_ORDER = secp256k1.N.ShiftRight(1);
            CURVE_ORDER = secp256k1.N;
            domain_parameter = new ECDomainParameters(Secp256k1.Curve, Secp256k1.G, Secp256k1.N, Secp256k1.H);
        }

        public ECKey()
            : this(Generate())
        {
        }

        public ECKey(AsymmetricCipherKeyPair key_pair)
            : this((key_pair.Private as ECPrivateKeyParameters).D.ToByteArrayUnsigned(), true)
        {
        }

        protected ECKey(byte[] bytes, bool isPrivate)
        {
            if (isPrivate)
            {
                key = new ECPrivateKeyParameters(new BigInteger(1, bytes), DomainParameter);
            }
            else
            {
                var q = Secp256k1.Curve.DecodePoint(bytes);
                key = new ECPublicKeyParameters("EC", q, DomainParameter);
            }
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        protected static AsymmetricCipherKeyPair Generate()
        {
            generator.Init(new ECKeyGenerationParameters(domain_parameter, new SecureRandom()));
            AsymmetricCipherKeyPair keys = generator.GenerateKeyPair();

            if ((keys.Private as ECPrivateKeyParameters).D.ToByteArrayUnsigned().Length != 32)
            {
                keys = Generate();
            }

            return keys;
        }

        private static ECPoint DecompressKey(BigInteger xBN, bool yBit)
        {
            var curve = Secp256k1.Curve;
            var compEnc = X9IntegerConverter.IntegerToBytes(xBN, 1 + X9IntegerConverter.GetByteLength(curve));
            compEnc[0] = (byte)(yBit ? 0x03 : 0x02);

            return curve.DecodePoint(compEnc);
        }
        #endregion


        #region External Method
        public static ECKey FromPrivateKey(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException("private key must be not null.");

            if (bytes.Length != 32)
                throw new ArgumentException("prviate key length must be 32 bytes.");

            return new ECKey(bytes, true);
        }

        public static ECKey FromPublicKey(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException("private key must be not null.");

            if (bytes.Length != 65)
                throw new ArgumentException("prviate key length must be 32 bytes.");

            return new ECKey(bytes, false);
        }

        public static int GetRecIdFromV(byte v)
        {
            var header = v;

            if (header < 27 || header > 34)
                throw new Exception("Header byte out of range : " + header);
            if (header >= 31)
                header -= 4;

            return header - 27;
        }

        public virtual ECDSASignature Sign(byte[] hash)
        {
            if (PrivateKey == null)
                throw new ArgumentNullException("private key must be not null.");

            DeterministicECDSA signer = new DeterministicECDSA();
            signer.Init(true, PrivateKeyParameter);

            ECDSASignature signature = ECDSASignature.FromDER(signer.SignHash(hash)).MakeCanonicalised();

            int rec_id = -1;
            for (int i = 0; i < 4; i++)
            {
                var recovery = RecoverFromSignature(i, signature, hash, false);
                if (recovery != null && recovery.PublicKey.SequenceEqual(PublicKey))
                {
                    rec_id = i;
                    break;
                }
            }

            if (rec_id == -1)
            {
                throw new Exception("Could not constrcut a recoverable key. This should never happen.");
            }
            signature.V = (byte)(rec_id + 27);

            return signature;
        }

        public bool Verify(byte[] hash, ECDSASignature signatrue)
        {
            var signer = new ECDsaSigner();
            signer.Init(false, PublicKeyParameter);

            return signer.VerifySignature(hash, signatrue.R, signatrue.S);
        }

        public static ECKey RecoverFromSignature(ECDSASignature signature, byte[] message, bool compressed)
        {
            return RecoverFromSignature(GetRecIdFromV(signature.V), signature, message, compressed);
        }

        public static ECKey RecoverFromSignature(int rec_id, ECDSASignature signautre, byte[] message, bool compressed)
        {
            if (rec_id < 0)
                throw new ArgumentException("rec_id must be positive");
            if (signautre.R.SignValue < 0)
                throw new ArgumentException("ECDSASignature R must be positive");
            if (signautre.S.SignValue < 0)
                throw new ArgumentException("ECDSASignature S must be positive");
            if (message == null)
                throw new ArgumentNullException("Recovery signature message is null");

            var curve = Secp256k1;
            var n = curve.N;
            var i = BigInteger.ValueOf((long)rec_id / 2);
            var x = signautre.R.Add(i.Multiply(n));
            var prime = new BigInteger(1,
                Org.BouncyCastle.Utilities.Encoders.Hex.Decode(
                    "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEFFFFFC2F"));
            if (x.CompareTo(prime) >= 0)
                return null;

            var R = DecompressKey(x, (rec_id & 1) == 1);

            if (!R.Multiply(n).IsInfinity)
                return null;

            var e = new BigInteger(1, message);
            var e_inv = BigInteger.Zero.Subtract(e).Mod(n);
            var r_inv = signautre.R.ModInverse(n);
            var sr_inv = r_inv.Multiply(signautre.S).Mod(n);
            var e_invr_inv = r_inv.Multiply(e_inv).Mod(n);
            var q = ECAlgorithms.SumOfTwoMultiplies(curve.G, e_invr_inv, R, sr_inv);
            q = q.Normalize();

            if (compressed)
            {
                q = Secp256k1.Curve.CreatePoint(q.XCoord.ToBigInteger(), q.YCoord.ToBigInteger());
                return new ECKey(q.GetEncoded(true), false);
            }

            return new ECKey(q.GetEncoded(), false);
        }

        public static byte[] ComputeAddress(byte[] publickey)
        {
            return Hash.ToAddress(ArrayUtil.CopyRange(publickey, 1, publickey.Length));
        }

        public static byte[] SignatureToAddress(byte[] hash, ECDSASignature signature)
        {
            return ComputeAddress(SignatureToKeyBytes(hash, signature));
        }

        public static byte[] SignatureToKeyBytes(byte[] hash, ECDSASignature signature)
        {
            if (hash.Length != 32)
                throw new ArgumentException("messageHash argument has length " + hash.Length);

            int header = new byte[] { signature.V }.ToInt32(0);
            if (header < 27 || header > 34)
            {
                throw new SignatureException("Header byte out of range: " + header);
            }
            if (header >= 31)
            {
                header -= 4;
            }
            int rec_id = header - 27;
            byte[] key = RecoverFromSignature(rec_id, signature, hash, false).PublicKey;
            if (key == null)
            {
                throw new SignatureException("Could not recover public key from " +
                    "signature");
            }
            return key;
        }
        #endregion
    }
}
