using System;
using System.Collections.Generic;
using System.IO;

namespace Sky.Core
{
    public class VoteTransaction : TransactionBase
    {
        public UInt160 Sender { get; private set; }
        public Dictionary<UInt160, Fixed8> Votes { get; private set; }
        public override int Size => base.Size + Sender.Size + Votes.GetSize();

        public VoteTransaction()
        {
        }

        public VoteTransaction(Transaction owner, List<TransactionInput> inputs, List<TransactionOutput> outputs, List<MakerSignature> signatures)
            : base(owner, inputs, outputs, signatures)
        {
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Sender = reader.ReadSerializable<UInt160>();
            Votes = reader.ReadSerializableDictionary<UInt160, Fixed8>();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteSerializable(Sender);
            writer.WriteSerializableDictonary(Votes);
        }

        public override bool Verify()
        {
            if (!base.Verify())
                return false;
            if (Sender != References[0].AddressHash)
                return false;
            return true;
        }
    }
}
