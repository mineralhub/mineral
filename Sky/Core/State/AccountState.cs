using Sky.Cryptography;
using System.IO;

namespace Sky.Core
{
    public class AccountState : StateBase
    {
        public UInt160 AddressHash { get; private set; }
        public bool IsFrozen { get; private set; }
        public Fixed8 Balance { get; private set; }
        public byte[] Vote { get; private set; }

        public override int Size => base.Size + AddressHash.Size + sizeof(bool) + Balance.Size;

        public AccountState() { }
        public AccountState(UInt160 hash)
        {
            AddressHash = hash;
            IsFrozen = false;
            Balance = Fixed8.Zero;
            Vote = null;
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            AddressHash = reader.ReadSerializable<UInt160>();
            IsFrozen = reader.ReadBoolean();
            Balance = reader.ReadSerializable<Fixed8>();
            Vote = reader.ReadByteArray();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteSerializable(AddressHash);
            writer.Write(IsFrozen);
            writer.WriteSerializable(Balance);
            writer.WriteByteArray(Vote);
        }

        public void AddBalance(Fixed8 value)
        {
            Balance += value;
        }

        public void SetVote(byte[] vote)
        {
            Vote = vote;
        }
    }
}
