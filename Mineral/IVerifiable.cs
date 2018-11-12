using System.IO;

namespace Mineral
{
    public interface IVerifiable : ISerializable
    {
        bool Verify();
        void DeserializeUnsigned(BinaryReader reader);
        void SerializeUnsigned(BinaryWriter writer);
    }
}
