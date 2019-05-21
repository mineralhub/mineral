using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Utils;
using Newtonsoft.Json;

namespace Mineral.Converter
{
    public class JsonUInt256Converter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(UInt160).Equals(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return UInt256.FromHexString(reader.Value.ToString());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((UInt256)value).ToArray().ToHexString());
        }
    }
}
