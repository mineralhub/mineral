using System.IO;

namespace Sky.Core
{
    public class TransactionOutput : ISerializable
    {
        public Fixed8 Value { get; private set; }
        public UInt160 AddressHash { get; private set; }

        public int Size => Value.Size + AddressHash.Size;

        public TransactionOutput() { }
        public TransactionOutput(Fixed8 value, UInt160 addressHash)
        {
            Value = value;
            AddressHash = addressHash;
        }

        void ISerializable.Deserialize(BinaryReader reader)
        {
            Value = reader.ReadSerializable<Fixed8>();
            AddressHash = reader.ReadSerializable<UInt160>();
        }

        void ISerializable.Serialize(BinaryWriter writer)
        {
            writer.WriteSerializable(Value);
            writer.WriteSerializable(AddressHash);
        }
    }
}
