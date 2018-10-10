using System.Collections.Generic;
using System.IO;

namespace Sky.Core
{
    public class DelegateState : StateBase
    {
        public UInt160 AddressHash { get; private set; }
        public byte[] Name { get; private set; }
        public Dictionary<UInt160, Fixed8> Votes { get; private set; }

        public override int Size => base.Size + AddressHash.Size + Name.GetSize() + Votes.GetSize();

        public DelegateState()
        {
            Votes = new Dictionary<UInt160, Fixed8>();
        }

        public DelegateState(UInt160 addressHash, byte[] name)
        {
            AddressHash = addressHash;
            Name = name;
            Votes = new Dictionary<UInt160, Fixed8>();
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            AddressHash = reader.ReadSerializable<UInt160>();
            Name = reader.ReadByteArray();
            Votes = reader.ReadSerializableDictionary<UInt160, Fixed8>();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteSerializable(AddressHash);
            writer.WriteByteArray(Name);
            writer.WriteSerializableDictonary(Votes);
        }

        public void Vote(UInt160 addressHash, Fixed8 value)
        {
            if (Votes.ContainsKey(addressHash))
            {
                if (value == Fixed8.Zero)
                {
                    Votes.Remove(addressHash);
                }
                else
                {
                    Votes[addressHash] = value;
                }
            }
            else if (value > Fixed8.Zero)
            {
                Votes.Add(addressHash, value);
            }
        }
    }
}
