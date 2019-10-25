using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Exception;
using Newtonsoft.Json;

namespace Mineral.Converter
{
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
            throw new InvalidTypeException(existingValue.ToString() + "is Inavlid LogLevel Type");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((LogLevel)value).ToString());
        }
    }
}
