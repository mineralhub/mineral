using System.IO;
using Newtonsoft.Json.Linq;

namespace Sky.Core
{
    public class RegisterDelegateTransaction : TransactionBase
    {
        public byte[] NameBytes;

        public override int Size => base.Size + NameBytes.GetSize();

        public override void CalcFee()
        {
            Fee = Config.RegisterDelegateFee;
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            NameBytes = reader.ReadByteArray();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteByteArray(NameBytes);
        }

        public override bool Verify()
        {
            if (!base.Verify())
                return false;
            if (NameBytes == null || NameBytes.Length == 0)
                return false;
            return true;
        }

        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["namebytes"] = NameBytes;
            return json;
        }
    }
}
