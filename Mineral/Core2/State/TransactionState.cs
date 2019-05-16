using Mineral.Core2.Transactions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mineral.Core2.State
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
            Transaction = Transaction.DeserializeFrom(reader.ReadBytes((int)reader.BaseStream.Length), 0);
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.Write(Height);
            writer.WriteSerializable(Transaction);
        }

        public static TransactionState DeserializeFrom(byte[] data, int offset = 0)
        {
            TransactionState txState = new TransactionState();
            using (MemoryStream ms = new MemoryStream(data, offset, data.Length - offset, false))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                txState.Deserialize(reader);
            }
            return txState;
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
