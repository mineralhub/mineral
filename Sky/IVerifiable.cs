using System;
using System.IO;

namespace Sky
{
    public interface IVerifiable : ISerializable
    {
        bool Verify();
        void DeserializeUnsigned(BinaryReader reader);
        void SerializeUnsigned(BinaryWriter writer);
    }
}
