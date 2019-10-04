using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Mineral.Converter
{
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
}
