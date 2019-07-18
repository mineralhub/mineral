using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Mineral.Converter;

namespace Mineral.Wallets.KeyStore
{
    public class KdfParam
    {
        [JsonProperty("dklen")]
        public int Dklen { get; set; }
        [JsonProperty("n")]
        public int N { get; set; }
        [JsonProperty("p")]
        public int P { get; set; }
        [JsonProperty("r")]
        public int R { get; set; }
        [JsonProperty("salt")]
        [JsonConverter(typeof(JsonByteArrayToHexConverter))]
        public byte[] Salt { get; set; }

        public static KdfParam GetDefaultParam()
        {
            return new KdfParam() { Dklen = 32, N = 1 << 18, P = 1, R = 8 };
        }
    }

    public class AesParam
    {
        [JsonProperty("iv")]
        [JsonConverter(typeof(JsonByteArrayToHexConverter))]
        public byte[] Iv { get; set; }
    }

    public class KeyStoreKdfInfo
    {
        [JsonProperty("alg")]
        public string Name { get; set; }
        [JsonProperty("params")]
        public KdfParam Params { get; set; }
    }

    public class KeyStoreAesInfo
    {
        [JsonProperty("alg")]
        public string Name { get; set; }
        [JsonProperty("text")]
        [JsonConverter(typeof(JsonByteArrayToHexConverter))]
        public byte[] Text { get; set; }
        [JsonProperty("params")]
        public AesParam Params { get; set; }
    }

    public class KeyStoreCryptoInfo
    {
        [JsonProperty("kdf")]
        public KeyStoreKdfInfo Kdf { get; set; }
        [JsonProperty("cipher")]
        public KeyStoreAesInfo Aes { get; set; }
        [JsonProperty("mac")]
        [JsonConverter(typeof(JsonByteArrayToHexConverter))]
        public byte[] Mac { get; set; }
    }

    public class KeyStore
    {
        [JsonProperty("version")]
        public int Version { get; set; }
        [JsonProperty("address")]
        public string Address { get; set; }
        [JsonProperty("crypto")]
        public KeyStoreCryptoInfo Crypto { get; set; }

        public static KeyStore FromJson(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<KeyStore>(json);
            }
            catch (System.Exception e)
            {
                Logger.Error("Invalid keystore file format.");
                return null;
            }
        }
    }
}
