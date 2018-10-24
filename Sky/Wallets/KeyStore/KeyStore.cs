using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Sky.Wallets.KeyStore
{
    public class SCryptParam
    {
        [JsonProperty("dklen")]
        public int Dklen { get; set; }
        [JsonProperty("n")]
        public int N { get; set; }
        [JsonProperty("p")]
        public int P { get; set; }
        [JsonProperty("r")]
        public int R { get; set; }

        public static SCryptParam GetDefaultParam()
        {
            return new SCryptParam() { Dklen = 32, N = 1 << 18, P = 1, R = 8 };
        }
    }

    public class KeyStoreScryptParam : SCryptParam
    {
        [JsonProperty("salt")]
        public string Salt { get; set; }
    }

    public class KeyStoreAesParam
    {
        [JsonProperty("iv")]
        public string Iv { get; set; }
        [JsonProperty("cipher")]
        public string Cipher { get; set; }
        [JsonProperty("mac")]
        public string Mac { get; set; }
    }

    public class KeyStore
    {
        [JsonProperty("version")]
        public int Version { get; set; }
        [JsonProperty("address")]
        public string Address { get; set; }
        [JsonProperty("scrypt")]
        public KeyStoreScryptParam Scrypt { get; set; }
        [JsonProperty("aes")]
        public KeyStoreAesParam Aes { get; set; }
    }
}
