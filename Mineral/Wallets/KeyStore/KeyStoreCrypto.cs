using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Digests;

namespace Mineral.Wallets.KeyStore
{
    public class KeyStoreCrypto
    {
        public class KeyStoreSalt : RandomNumberGenerator
        {
            public byte[] _salt;
            public KeyStoreSalt(byte[] salt)
            {
                _salt = salt;
            }

            public override void GetBytes(byte[] data)
            {
                Array.Copy(_salt, data, _salt.Length);
            }
        }

        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public static bool GenerateScrypt(string password, int n, int r, int p, int dklen, out byte[] salt, out byte[] derivedkey)
        {
            bool result = false;

            salt = new byte[32];
            derivedkey = new byte[32];
            string[] encrypt = new Scrypt.ScryptEncoder(
                                            n,
                                            r,
                                            p,
                                            new RandomGenerator()).Encode(password).Split('$');

            if (encrypt.Length == 7)
            {
                var result_salt = Convert.FromBase64String(encrypt[encrypt.Length - 2]);
                Array.Copy(result_salt, salt, result_salt.Length);

                var result_derivedkey = Convert.FromBase64String(encrypt[encrypt.Length - 1]);
                Array.Copy(result_derivedkey, derivedkey, result_derivedkey.Length);

                result = true;
            }

            return result;
        }

        public static bool EncryptScrypt(string password, int n, int r, int p, int dklen, byte[] salt, out byte[] derivedkey)
        {
            bool result = false;

            derivedkey = new byte[32];
            string[] encrypt = new Scrypt.ScryptEncoder(
                                n,
                                r,
                                p,
                                new KeyStoreSalt(salt)).Encode(password).Split('$');

            if (encrypt.Length == 7)
            {
                var result_derivedkey = Convert.FromBase64String(encrypt[encrypt.Length - 1]);
                Array.Copy(result_derivedkey, derivedkey, result_derivedkey.Length);

                result = true;
            }

            return result;
        }

        public static byte[] GenerateCipherKey(byte[] derivekey)
        {
            byte[] cipherkey = new byte[16];
            Array.Copy(derivekey, cipherkey, 16);
            return cipherkey;
        }

        public static byte[] GenerateMac(byte[] derivekey, byte[] ciphertext)
        {
            int size = (int)(derivekey.Length * 0.5) + ciphertext.Length;

            byte[] input = new byte[size];
            Array.Copy(derivekey, 16, input, 0, 16);
            Array.Copy(ciphertext, 0, input, 16, ciphertext.Length);

            var digest = new KeccakDigest(256);
            var mac = new byte[digest.GetDigestSize()];
            digest.BlockUpdate(input, 0, input.Length);
            digest.DoFinal(mac, 0);

            return mac;
        }

        public static bool VerifyMac(byte[] derivedkey, byte[] ciphertext, byte[] mac)
        {
            byte[] generateMac = GenerateMac(derivedkey, ciphertext);
            return generateMac.ToHexString().Equals(mac.ToHexString());
        }
        #endregion
    }
}
