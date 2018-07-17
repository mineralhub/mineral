using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sky.Core
{
    public class DelegatorState : StateBase
    {
        public byte[] Name { get; private set; }
        public UInt160 AddressHash { get; private set; }
        public List<UInt160> VoteAddressHashes { get; private set; }

        public override int Size => base.Size + Name.GetSize() + AddressHash.Size + VoteAddressHashes.GetSize();

        public DelegatorState()
        {
            VoteAddressHashes = new List<UInt160>();
        }

        public DelegatorState(byte[] name)
            : this()
        {
            Name = name;
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Name = reader.ReadByteArray();
            AddressHash = reader.ReadSerializable<UInt160>();
            VoteAddressHashes = reader.ReadSerializableArray<UInt160>();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteByteArray(Name);
            writer.WriteSerializable(AddressHash);
            writer.WriteSerializableArray(VoteAddressHashes);
        }

        public void SetAddressHash(UInt160 addrHash)
        {
            AddressHash = addrHash;
        }
    }
}
