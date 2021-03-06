﻿using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core;
using Newtonsoft.Json;

namespace Mineral.Converter
{
    public class JsonAddressToByteArray : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(byte[]).Equals(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return Wallet.Base58ToAddress(reader.Value.ToString());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(Wallet.AddressToBase58((byte[])value));
        }
    }
}
