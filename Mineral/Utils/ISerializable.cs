using System.IO;

namespace Mineral
{
    public interface ISerializable
    {
        int Size { get; }
        void Serialize(BinaryWriter writer);
        void Deserialize(BinaryReader reader);
    }
}
