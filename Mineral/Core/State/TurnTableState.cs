using Mineral.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mineral.Core
{
    public class TurnTableState : StateBase
    {
        public int turnTableHeight { get; private set; }
        public List<UInt160> addrs { get; private set; }

        public override int Size => base.Size + addrs.GetSize() + sizeof(int);

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            turnTableHeight = reader.ReadInt32();
            addrs = reader.ReadSerializableArray<UInt160>();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(turnTableHeight);
            writer.WriteSerializableArray<UInt160>(addrs);
        }

        public void SetTurnTable(List<UInt160> addr, int height)
        {
            addrs = addr;
            turnTableHeight = height;
        }
    }
}
