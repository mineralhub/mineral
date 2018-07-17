using System;
using System.IO;
using System.Text;

namespace Sky.Core
{
    public class VoteTransaction : Transaction
    {
        public UInt160 Sender { get; private set; }
        public byte[] Delegate { get; private set; }
        public override int Size => base.Size + Sender.Size + Delegate.GetSize();

        public VoteTransaction()
        {
        }
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Sender = reader.ReadSerializable<UInt160>();
            Delegate = reader.ReadByteArray();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteSerializable(Sender);
            writer.WriteByteArray(Delegate);
        }

        public override bool Verify()
        {
            if (!base.Verify())
                return false;
            if (Sender != References[0].AddressHash)
                return false;
            if (Delegate == null || Delegate.Length == 0)
                return false;
            return true;
        }
    }
}
