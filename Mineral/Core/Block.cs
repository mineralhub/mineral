using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Mineral.Cryptography;

namespace Mineral.Core
{
    public class Block : IVerifiable
    {
        public BlockHeader Header { get; private set; }
        public List<Transaction> Transactions { get; private set; }
        public int Size => Header.Size + Transactions.GetSize();
        public UInt256 Hash => Header.GetHash();
        public int Height => Header.Height;

        public Block()
        {
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

        public byte[] Trim()
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms))
            {
                Header.Serialize(writer);
                writer.WriteSerializableArray(Transactions.Select(p => p.Hash).ToArray());
                writer.Flush();
                return ms.ToArray();
            }
        }

        public static Block FromTrimmedData(byte[] data, int index, Func<UInt256, Transaction> txSelector)
        {
            Block block = new Block();
            using (MemoryStream ms = new MemoryStream(data, index, data.Length - index, false))
            using (BinaryReader reader = new BinaryReader(ms))
            {
                try
                {
                    block.Header = new BlockHeader();
                    block.Header.Deserialize(reader);
                    int count = reader.ReadInt32();
                    block.Transactions = new List<Transaction>(count);
                    for (int i = 0; i < count; ++i)
                    {
                        block.Transactions.Add(txSelector(reader.ReadSerializable<UInt256>()));
                    }
                    return block;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public bool Verify()
        {
            if (Header.Verify() == false)
                return false;
            if (Transactions.Count == 0)
                return false;
            if (Header.MerkleRoot != new MerkleTree(Transactions.Select(p => p.Hash).ToArray()).RootHash)
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

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["header"] = Header.ToJson();
            JArray transactions = new JArray();
            foreach (var v in Transactions)
                transactions.Add(v.ToJson());
            json["transactions"] = transactions;
            json["hash"] = Hash.ToString();
            return json;
        }
    }
}
