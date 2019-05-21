using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Utils;
using Newtonsoft.Json;

namespace Mineral.Converter
{
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
}
