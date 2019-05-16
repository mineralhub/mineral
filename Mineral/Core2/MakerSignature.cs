using Newtonsoft.Json.Linq;
using System.IO;

namespace Mineral.Core2
{
    public class MakerSignature : ISerializable
    {
        public byte[] Signature { get; private set; }
        public byte[] Pubkey { get; private set; }

        public int Size => Signature.GetSize() + Pubkey.GetSize();

        public MakerSignature()
        {
        }

        public MakerSignature(byte[] signature, byte[] pubkey)
        {
            Signature = signature;
            Pubkey = pubkey;
        }

        public void Deserialize(BinaryReader reader)
        {
            Signature = reader.ReadByteArray();
            Pubkey = reader.ReadByteArray();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.WriteByteArray(Signature);
            writer.WriteByteArray(Pubkey);
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["signature"] = Signature == null ? null : Signature.ToHexString();
            json["pubkey"] = Pubkey == null ? null : Pubkey.ToHexString();
            return json;
        }
    }
}
