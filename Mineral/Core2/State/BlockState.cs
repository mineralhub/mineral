using Mineral.Core2.Transactions;
using Mineral.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Mineral.Core2.State
{
    public class BlockState : StateBase
    {
        public Fixed8 Fee { get; private set; }
        public BlockHeader Header { get; private set; }
        public UInt256[] TransactionHashs { get; private set; }

        public override int Size => base.Size + sizeof(uint) + Fee.Size + Header.Size;

        public BlockState()
        {
        }

        public BlockState(Block block, Fixed8 fee = default(Fixed8))
        {
            Fee = fee;
            Header = block.Header;
            TransactionHashs = block.Transactions.Select(p => p.Hash).ToArray();
        }

        public Block GetBlock(Func<UInt256, TransactionState> txSelector)
        {
            List<Transaction> transactions = new List<Transaction>();
            foreach (UInt256 hash in TransactionHashs)
            {
                TransactionState txState = txSelector(hash);
                if (txState != null)
                {
                    transactions.Add(txState.Transaction);
                }
            }
            return new Block(Header, transactions);
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Fee = reader.ReadSerializable<Fixed8>();
            Header = BlockHeader.FromArray(reader.ReadByteArray(), 0);
            TransactionHashs = reader.ReadSerializableArray<UInt256>().ToArray();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteSerializable(Fee);
            writer.WriteByteArray(Header.ToArray());
            writer.WriteSerializableArray(TransactionHashs);
        }

        public static BlockState DeserializeFrom(byte[] data, int offset = 0)
        {
            BlockState blockState = new BlockState();
            using (MemoryStream ms = new MemoryStream(data, offset, data.Length - offset, false))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                blockState.Deserialize(reader);
            }
            return blockState;
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["fee"] = Fee.ToString();
            json["header"] = Header.ToJson();
            json["transaction_hashs"] = TransactionHashs.ToString();
            return json;
        }
    }
}
