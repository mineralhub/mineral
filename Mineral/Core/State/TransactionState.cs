using Mineral.Core.Transactions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mineral.Core.State
{
    public class TransactionState : StateBase
    {
        public uint Height { get; private set; }
        public Transaction Transaction { get; private set; }

        public override int Size => base.Size + sizeof(uint) + Transaction.Size;

        public TransactionState()
        {
        }

        public TransactionState(uint height, Transaction tx)
        {
            Height = height;
            Transaction = tx;
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Height = reader.ReadUInt32();
            Transaction = Transaction.DeserializeFrom(reader.ReadBytes((int)reader.BaseStream.Length), sizeof(uint));
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(Height);
            writer.WriteSerializable(Transaction);
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["height"] = Height.ToString();
            json["transaction"] = Transaction.ToJson();
            return json;
        }
    }
}
