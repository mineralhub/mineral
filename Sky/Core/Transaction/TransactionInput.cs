using System.IO;
using Sky.Cryptography;

namespace Sky.Core
{
    public class TransactionInput : ISerializable
    {
        public UInt256 PrevHash { get; private set; }
        public ushort PrevIndex { get; private set; }
        public UInt256 Hash => this.GetHash();
        public int Size => PrevHash.Size + sizeof(ushort);

        public TransactionInput()
        {
        }

        public TransactionInput(UInt256 prevHash, ushort prevIndex)
        {
            PrevHash = prevHash;
            PrevIndex = prevIndex;
        }

        public void Deserialize(BinaryReader reader)
        {
            PrevHash = reader.ReadSerializable<UInt256>();
            PrevIndex = reader.ReadUInt16();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.WriteSerializable(PrevHash);
            writer.Write(PrevIndex);
        }
    }
}
