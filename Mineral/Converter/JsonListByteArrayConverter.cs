using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mineral.Converter
{
    public class JsonListByteArrayConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(byte[]).Equals(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            List<byte[]> result = new List<byte[]>();
            JToken token = JToken.Load(reader);

            foreach (string item in token)
            {
                result.Add(item.HexToBytes());
            }

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            List<byte[]> items = value as List<byte[]>;
            List<string> result = items.Select(bytes =>
            {
                return bytes.ToHexString();
            }).ToList();

            serializer.Serialize(writer, result);
        }
    }
}
