using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using Org.BouncyCastle.Security;

namespace Mineral.Wallets.KeyStore
{
    public class RandomGenerator : RandomNumberGenerator
    {
        #region Field
        private static SecureRandom random = new SecureRandom();
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
        public static byte[] GenerateRandomBytes(int length)
        {
            return SecureRandom.GetNextBytes(random, length);
        }

        public override void GetBytes(byte[] data)
        {
            byte[] result = GenerateRandomBytes(32);
            Array.Copy(result, data, result.Length);
        }
        #endregion
    }
}
