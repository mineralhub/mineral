using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Sky.Cryptography;

namespace Sky.Core
{
    public class Block : IVerifiable
    {
        private BlockHeader _header = null;
        private List<Transaction> _transactions = null;
        public BlockHeader Header => _header;
        public List<Transaction> Transactions => _transactions;
        public int Size => _header.Size + _transactions.GetSize();
        public UInt256 Hash => _header.GetHash();
        public int Height => _header.Height;

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
            _header = header;
            _transactions = transactions;
        }

        public void DeserializeUnsigned(BinaryReader reader)
        {
            _header = new BlockHeader();
            _header.DeserializeUnsigned(reader);
            _transactions = reader.ReadSerializableArray<Transaction>();
        }

        public void SerializeUnsigned(BinaryWriter writer)
        {
            _header.SerializeUnsigned(writer);
            writer.WriteSerializableArray(_transactions);
        }

        public void Deserialize(BinaryReader reader)
        {
            _header = new BlockHeader();
            _header.Deserialize(reader);
            _transactions = reader.ReadSerializableArray<Transaction>();
        }

        public void Serialize(BinaryWriter writer)
        {
            _header.Serialize(writer);
            writer.WriteSerializableArray(_transactions);
        }

        public bool Verify()
        {
            if (Header.Verify() == false)
                return false;
            if (_transactions.Count == 0)
                return false;
            if (_transactions[0].Type != eTransactionType.RewardTransaction)
                return false;
            if (1 < _transactions.Where(p => p.Type == eTransactionType.RewardTransaction).Count())
                return false;
            BlockHeader prev = Blockchain.Instance.GetHeader(_header.PrevHash);
            if (prev == null)
                return false;
            if (prev.Height + 1 != Height)
                return false;
            if (_header.Timestamp <= prev.Timestamp)
                return false;
            foreach (Transaction tx in _transactions)
                if (!tx.Verify())
                    return false;
            return true;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
