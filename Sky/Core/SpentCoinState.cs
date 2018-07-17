using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sky.Core
{
    public class SpentCoinState : StateBase
    {
        public UInt256 TransactionHash { get; private set; }
        public int TransactionHeight { get; private set; }
        public Dictionary<ushort, int> Items { get; private set; }
        public override int Size => base.Size + TransactionHash.Size + sizeof(int) + sizeof(int) + Items.Count * (sizeof(ushort) + sizeof(int));

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            TransactionHash = reader.ReadSerializable<UInt256>();
            TransactionHeight = reader.ReadInt32();
            int count = reader.ReadInt32();
            Items = new Dictionary<ushort, int>();
            for (int i = 0; i < count; ++i)
            {
                ushort index = reader.ReadUInt16();
                int height = reader.ReadInt32();
                Items.Add(index, height);
            }
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteSerializable(TransactionHash);
            writer.Write(TransactionHeight);
            writer.Write(Items.Count);
            foreach (var i in Items)
            {
                writer.Write(i.Key);
                writer.Write(i.Value);
            }
        }
    }
}
