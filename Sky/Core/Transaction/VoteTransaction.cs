using System.Collections.Generic;
using System.IO;

namespace Sky.Core
{
    public class VoteTransaction : TransactionBase
    {
        public Dictionary<UInt160, Fixed8> Votes { get; private set; }
        public override int Size => base.Size + Votes.GetSize();

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Votes = reader.ReadSerializableDictionary<UInt160, Fixed8>();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteSerializableDictonary(Votes);
        }

        public override bool Verify()
        {
            if (!base.Verify())
                return false;
            return true;
        }
    }
}
