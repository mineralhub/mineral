using System.Collections.Generic;
using System.IO;

namespace Sky.Core
{
    public class BlockTriggerState : StateBase
    {
        public List<UInt256> TransactionHashes { get; private set; }

        public override int Size => base.Size + TransactionHashes.GetSize();

        public BlockTriggerState()
        {
            TransactionHashes = new List<UInt256>();
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            TransactionHashes = reader.ReadSerializableArray<UInt256>();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteSerializableArray(TransactionHashes);
        }
    }
}
