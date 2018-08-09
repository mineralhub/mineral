using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Sky.Cryptography;

namespace Sky.Core
{
    public class Block : IVerifiable
    {
        public BlockHeader Header { get; private set; }
        public List<Transaction> Transactions { get; private set; }
        public int Size => Header.Size + Transactions.GetSize();
        public UInt256 Hash => Header.GetHash();
        public int Height => Header.Height;

        public Block(byte[] data, int index)
        {
            using (MemoryStream ms = new MemoryStream(data, index, data.Length - index, false))
            using (BinaryReader br = new BinaryReader(ms))
            {
                try
                {
                    Deserialize(br);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public Block(BlockHeader header, List<Transaction> transactions)
        {
            Header = header;
            Transactions = transactions;
        }

        public void DeserializeUnsigned(BinaryReader reader)
        {
            Header = new BlockHeader();
            Header.DeserializeUnsigned(reader);
            Transactions = reader.ReadSerializableArray<Transaction>();
        }

        public void SerializeUnsigned(BinaryWriter writer)
        {
            Header.SerializeUnsigned(writer);
            writer.WriteSerializableArray(Transactions);
        }

        public void Deserialize(BinaryReader reader)
        {
            Header = new BlockHeader();
            Header.Deserialize(reader);
            Transactions = reader.ReadSerializableArray<Transaction>();
        }

        public void Serialize(BinaryWriter writer)
        {
            Header.Serialize(writer);
            writer.WriteSerializableArray(Transactions);
        }

        public bool Verify()
        {
            if (Header.Verify() == false)
                return false;
            if (Transactions.Count == 0)
                return false;
            if (Transactions[0].Type != eTransactionType.RewardTransaction)
                return false;
            if (1 < Transactions.Where(p => p.Type == eTransactionType.RewardTransaction).Count())
                return false;
            BlockHeader prev = Blockchain.Instance.GetHeader(Header.PrevHash);
            if (prev == null)
                return false;
            if (prev.Height + 1 != Height)
                return false;
            foreach (Transaction tx in Transactions)
                if (!tx.Verify())
                    return false;
            return true;
        }

        public string ToJson()
        {
            var json = new JObject();
            json.Add("hash", Hash.ToString());
            return json.ToString();
        }
    }
}
