using System.IO;

namespace Sky
{
    public interface ISerializable
    {
        int Size { get; }
        void Serialize(BinaryWriter writer);
        void Deserialize(BinaryReader reader);
    }
}
