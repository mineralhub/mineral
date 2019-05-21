using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Utils;
using Newtonsoft.Json;

namespace Mineral.Converter
{
    public class JsonUInt160Converter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(UInt160).Equals(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return Wallets.WalletAccount.ToAddressHash(reader.Value.ToString());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(Wallets.WalletAccount.ToAddress((UInt160)value));
        }
    }
}
