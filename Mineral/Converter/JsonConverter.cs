using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Converter
{
    public class JsonByteArrayConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(byte[]).Equals(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return System.Text.Encoding.UTF8.GetBytes(reader.Value.ToString());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(System.Text.Encoding.UTF8.GetString((byte[])value));
        }
    }

    public class JsonByteArrayToHexConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(byte[]).Equals(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return ((string)reader.Value).HexToBytes();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            byte[] val = value as byte[];
            writer.WriteValue(val.ToHexString());
        }
    }

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

    public class JsonFixed8Converter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(UInt160).Equals(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return Fixed8.Parse(reader.Value.ToString());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((Fixed8)value).ToString());
        }
    }
}
