using Mineral.Utils;
using Newtonsoft.Json;
using System;
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
            return reader.Value.ToString().HexToBytes();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((byte[])value).ToHexString());
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
            return typeof(Fixed8).Equals(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return Fixed8.FromLongValue(long.Parse(reader.Value.ToString()));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((Fixed8)value).Value);
        }
    }

    public class JsonLogLevelConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(LogLevel).Equals(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (Enum.TryParse<LogLevel>(reader.Value.ToString(), true, out LogLevel logLevel))
                return logLevel;
            return LogLevel.INFO;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((LogLevel)value).ToString());
        }
    }
}
