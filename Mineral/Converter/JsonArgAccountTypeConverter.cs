using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Exception;
using Newtonsoft.Json;
using static Mineral.Core.Config.Arguments.Account;

namespace Mineral.Converter
{
    public class JsonArgAccountTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(AccountType).Equals(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (Enum.TryParse<AccountType>(reader.Value.ToString(), true, out AccountType type))
                return type;
            throw new InvalidTypeException(existingValue.ToString() + "is Inavlid AccountType");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((LogLevel)value).ToString());
        }
    }
}
