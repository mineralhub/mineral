using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sky.Core
{
    public class UnspentCoinState : StateBase
    {
        public List<CoinState> Items { get; protected set; }
        public override int Size => base.Size + Items.GetSize();

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Items = reader.ReadByteArray().Select(p => (CoinState)p).ToList();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteByteArray(Items.Cast<byte>().ToArray());
        }
    }
}
