using Mineral.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mineral.Core.State
{
    public class BlockState : StateBase
    {
        public uint Height { get; private set; }
        public Fixed8 Fee { get; private set; }
        public BlockHeader Header { get; private set; }

        public override int Size => base.Size + sizeof(uint) + Fee.Size + Header.Size;

        public BlockState()
        {
        }

        public BlockState(Block block, Fixed8 fee = default(Fixed8))
        {
            Height = block.Header.Height;
            Fee = fee;
            Header = block.Header;
        }
        
        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Height = reader.ReadUInt32();
            Fee = reader.ReadSerializable<Fixed8>();
            Header = BlockHeader.FromArray(reader.ReadByteArray(), 0);
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(Height);
            writer.WriteSerializable(Fee);
            writer.WriteByteArray(Header.ToArray());
        }
    }
}
