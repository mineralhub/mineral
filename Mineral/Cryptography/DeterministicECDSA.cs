using System;
using System.Linq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;

namespace Mineral.Cryptography
{
    internal class DeterministicECDSA : ECDsaSigner
    {
        #region Field
        private readonly IDigest digest = null;
        private byte[] buffer = new byte[0];
        #endregion


        #region Property
        #endregion


        #region Contructor
        public DeterministicECDSA()
            : base(new HMacDsaKCalculator(new Sha256Digest()))

        {
            digest = new Sha256Digest();
        }

        public DeterministicECDSA(Func<IDigest> digest)
            : base(new HMacDsaKCalculator(digest()))
        {
            this.digest = digest();
        }

        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public byte[] Sign()
        {
            byte[] hash = new byte[digest.GetDigestSize()];

            digest.BlockUpdate(buffer, 0, buffer.Length);
            digest.DoFinal(hash, 0);
            digest.Reset();

            return SignHash(hash);
        }

        public byte[] SignHash(byte[] hash)
        {
            return new ECDSASignature(GenerateSignature(hash)).ToDER();
        }

        public void Update(byte[] buf)
        {
            buffer = buffer.Concat(buf).ToArray();
        }
        #endregion
    }
}
