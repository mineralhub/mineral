using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Sky.Core
{
    public class TransferTransaction : TransactionBase
    {
        public Dictionary<UInt160, Fixed8> To;

        public override int Size => base.Size + To.GetSize();

        public override void CalcFee()
        {
            Fee = Config.DefaultFee;
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            To = reader.ReadSerializableDictionary<UInt160, Fixed8>(Config.TransferToMaxLength);
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteSerializableDictonary(To);
        }

        public override bool Verify()
        {
            if (!base.Verify())
                return false;
            Fixed8 totalAmount = Fixed8.Zero;
            foreach (Fixed8 v in To.Values)
            {
                if (v < Fixed8.Satoshi)
                    return false;
                totalAmount += v;
            }
            if (FromAccountState.Balance - totalAmount < Fixed8.Zero)
                return false;
            return true;
        }

        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            JArray to = new JArray();
            foreach (var v in To)
            {
                var j = new JObject();
                j["addr"] = v.Key.ToString();
                j["amount"] = v.Value.ToString();
                to.Add(j);
            }
            json["to"] = to;
            return json;
        }
    }
}
