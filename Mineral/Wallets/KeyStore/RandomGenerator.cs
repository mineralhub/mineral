using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using Org.BouncyCastle.Security;

namespace Mineral.Wallets.KeyStore
{
    public class RandomGenerator : RandomNumberGenerator
    {
        private static SecureRandom random = new SecureRandom();

        public static byte[] GenerateRandomBytes(int length)
        {
            return SecureRandom.GetNextBytes(random, length);
        }

        public override void GetBytes(byte[] data)
        {
            byte[] result = GenerateRandomBytes(32);
            Array.Copy(result, data, result.Length);
        }
    }
}
