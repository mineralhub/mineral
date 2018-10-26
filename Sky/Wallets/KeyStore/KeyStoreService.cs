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
        public static readonly string KDF_SCRYPT = "scrypt";
        public static readonly string AES128CTR = "aes-128-ctr";

        public static bool GenerateKeyStore(string path, string password, byte[] privatekey, string address)
        {
            KdfParam param = KdfParam.GetDefaultParam();
            return GenerateKeyStore(path, password, privatekey, address, param.N, param.R, param.P, param.Dklen);
        }

        public static bool GenerateKeyStore(string path, string password, byte[] privatekey, string address, int n, int r, int p, int dklen)
        {
            KdfParam kdf_param = new KdfParam() { Dklen = dklen, N = n, R = r, P = p };

            byte[] salt;
            byte[] derivedkey;
            if (!KeyStoreCrypto.GenerateScrypt(password, kdf_param.N, kdf_param.R, kdf_param.P, kdf_param.Dklen, out salt, out derivedkey))
            {
                Console.WriteLine("fail to generate scrypt.");
                return false;
            }
            kdf_param.Salt = salt.ToHexString();

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
                Crypto = new KeyStoreCryptoInfo()
                {
                    Kdf = new KeyStoreKdfInfo()
                    {
                        Name = KDF_SCRYPT,
                        Params = kdf_param
                    },
                    Aes = new KeyStoreAesInfo()
                    {
                        Name = AES128CTR,
                        Text = ciphertext.ToHexString(),
                        Params = new AesParam()
                        {
                            Iv = iv.ToHexString(),
                        }
                    },
                    Mac = mac.ToHexString()
                },
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
            byte[] derivedkey = new byte[32];

            privatekey = null;

            password = "aAbBcCdDeE";

            KeyStoreKdfInfo kdf = keystore.Crypto.Kdf;
            KeyStoreAesInfo aes = keystore.Crypto.Aes;

            if (!KeyStoreCrypto.EncryptScrypt(password
                                            , kdf.Params.N
                                            , kdf.Params.R
                                            , kdf.Params.P
                                            , kdf.Params.Dklen
                                            , kdf.Params.Salt.HexToBytes()
                                            , out derivedkey))
            {
                Console.WriteLine("fail to generate scrypt.");
                return false;
            }

            byte[] iv = aes.Params.Iv.HexToBytes();
            byte[] ciphertext = aes.Text.HexToBytes();
            byte[] mac = keystore.Crypto.Mac.HexToBytes();

            if (!KeyStoreCrypto.VerifyMac(derivedkey, ciphertext, mac))
            {
                Console.WriteLine("Password do not match.");
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
