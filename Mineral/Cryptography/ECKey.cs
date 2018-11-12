using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using System;

namespace Mineral.Cryptography
{
    public class ECKey : IEquatable<ECKey>
    {
        public static readonly X9ECParameters Secp256k1;
        private static readonly ECDomainParameters DomainParameter;
        private static readonly ECKeyPairGenerator Generator;

        private readonly ECKeyParameters _key;
        public ECPrivateKeyParameters PrivateKey => _key as ECPrivateKeyParameters;
        public ECPublicKeyParameters PublicKey => _key is ECPublicKeyParameters ? (ECPublicKeyParameters)_key : new ECPublicKeyParameters("EC", Secp256k1.G.Multiply(PrivateKey.D), DomainParameter);
        public byte[] PrivateKeyBytes
        {
            get
            {
                byte[] keyBytes = PrivateKey.D.ToByteArray();
                if (keyBytes.Length == 32)
                    return keyBytes;
                byte[] bytes = new byte[32];
                Array.Copy(keyBytes, keyBytes.Length - bytes.Length, bytes, 0, bytes.Length);
                return bytes;
            }
        }

        static ECKey()
        {
            Secp256k1 = SecNamedCurves.GetByName("secp256k1");
            DomainParameter = new ECDomainParameters(Secp256k1.Curve, Secp256k1.G, Secp256k1.N, Secp256k1.H);
            Generator = new ECKeyPairGenerator();
        }

        public ECKey(byte[] key, bool prikey)
        {
            if (prikey)
            {
                _key = new ECPrivateKeyParameters(new BigInteger(1, key), DomainParameter);
            }
            else
            {
                _key = new ECPublicKeyParameters("EC", Secp256k1.Curve.DecodePoint(key), DomainParameter);
            }
        }

        public ECKey(AsymmetricCipherKeyPair keypair) : this((keypair.Private as ECPrivateKeyParameters).D.ToByteArray(), true)
        {
        }

        static public AsymmetricCipherKeyPair Generate()
        {
            Generator.Init(new ECKeyGenerationParameters(DomainParameter, new SecureRandom()));
            return Generator.GenerateKeyPair();
        }

        public bool Equals(ECKey other)
        {
            return _key == other._key;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ECKey);
        }

        public override int GetHashCode()
        {
            return _key.GetHashCode();
        }
    }
}
