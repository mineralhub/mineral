using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sky.Cryptography;

namespace Sky.Wallets.KeyStore
{
    public class KeyStoreService
    {
        public static bool GenerateKeyStore(string path, string password, byte[] privatekey, string address)
        {
            SCryptParam param = KeyStoreScryptParam.GetDefaultParam();
            return GenerateKeyStore(path, password, privatekey, address, param.N, param.R, param.P, param.Dklen);
        }

        public static bool GenerateKeyStore(string path, string password, byte[] privatekey, string address, int n, int r, int p, int dklen)
        {
            KeyStoreScryptParam scrypt_param = new KeyStoreScryptParam() { Dklen = dklen, N = n, R = r, P = p };

            byte[] salt;
            byte[] derivedkey;
            if (!KeyStoreCrypto.GenerateScrypt(password, scrypt_param.N, scrypt_param.R, scrypt_param.P, scrypt_param.Dklen, out salt, out derivedkey))
            {
                Console.WriteLine("fail to generate scrypt.");
                return false;
            }
            scrypt_param.Salt = salt.ToHexString();

            byte[] cipherkey = KeyStoreCrypto.GenerateCipherKey(derivedkey);
            byte[] iv = RandomGenerator.GenerateRandomBytes(16);

            byte[] ciphertext = new byte[32];
            using (var am = new Aes128CounterMode(iv))
            using (var ict = am.CreateEncryptor(cipherkey, null))
            {
                ict.TransformBlock(privatekey, 0, privatekey.Length, ciphertext, 0);
            }

            byte[] mac = KeyStoreCrypto.GenerateMac(derivedkey, ciphertext);

            KeyStore keystore = new KeyStore()
            {
                Version = 1,
                Address = address,
                Scrypt = scrypt_param,
                Aes = new KeyStoreAesParam()
                {
                    Iv = iv.ToHexString(),
                    Cipher = ciphertext.ToHexString(),
                    Mac = mac.ToHexString()
                }
            };

            string json = JsonConvert.SerializeObject(keystore);
            using (var file = File.CreateText(path))
            {
                file.Write(json);
                file.Flush();
            }

            return true;
        }

        public static bool DecryptKeyStore(string password, KeyStore keystore, out byte[] privatekey)
        {
            return DecryptKeyStore(password,
                                keystore.Scrypt.N,
                                keystore.Scrypt.P,
                                keystore.Scrypt.R,
                                keystore.Scrypt.Dklen,
                                keystore.Scrypt.Salt.HexToBytes(),
                                keystore.Aes.Iv.HexToBytes(),
                                keystore.Aes.Cipher.HexToBytes(),
                                keystore.Aes.Mac.HexToBytes(),
                                out privatekey
                );
        }

        public static bool DecryptKeyStore(string password
                                        , int n, int p, int r, int dklen, byte[] salt
                                        , byte[] iv, byte[] ciphertext, byte[] mac, out byte[] privatekey)
        {
            byte[] derivedkey = new byte[32];

            privatekey = null;
            if (!KeyStoreCrypto.EncryptScrypt(password, n, r, p, dklen, salt, out derivedkey))
            {
                return false;
            }

            if (!KeyStoreCrypto.VerifyMac(derivedkey, ciphertext, mac))
            {
                return false;
            }

            byte[] cipherkey = KeyStoreCrypto.GenerateCipherKey(derivedkey);

            privatekey = new byte[32];
            using (var am = new Aes128CounterMode(iv))
            using (var ict = am.CreateEncryptor(cipherkey, null))
            {
                ict.TransformBlock(ciphertext, 0, ciphertext.Length, privatekey, 0);
            }

            return true;
        }
    }
}
