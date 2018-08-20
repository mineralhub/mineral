using System.IO;
using System.Collections.Generic;
using Sky.Cryptography;
using System.Text;

namespace Sky.Core
{
    public class Transaction : IVerifiable
    {
        public short Version { get; protected set; }
        public eTransactionType Type { get; protected set; }
        public int Timestamp { get; protected set; }
        public TransactionBase Data { get; protected set; }
        public Fixed8 Fee => Data.Fee;
        public List<TransactionInput> Inputs => Data.Inputs;
        public List<TransactionOutput> Outputs => Data.Outputs;
        public List<MakerSignature> Signature => Data.Signatures;
        public List<TransactionOutput> References => Data.References;

        public virtual int Size => sizeof(eTransactionType) + sizeof(short) + sizeof(int) + Data.Size;
        public UInt256 Hash => this.GetHash();

        public Transaction(short version, eTransactionType type, int timestamp, List<TransactionInput> inputs, List<TransactionOutput> outputs, List<MakerSignature> signatures)
        {
            Version = version;
            Type = type;
            Timestamp = timestamp;
            MallocTrasnactionData(inputs, outputs, signatures);
        }

        public Transaction(short version, eTransactionType type, int timestamp, TransactionBase txData)
        {
            Version = version;
            Type = type;
            Timestamp = timestamp;
            Data = txData;
        }

        public Transaction()
        {
        }

        static public Transaction DeserializeFrom(byte[] value, int offset = 0)
        {
            using (MemoryStream ms = new MemoryStream(value, offset, value.Length - offset, false))
            using (BinaryReader reader = new BinaryReader(ms, Encoding.UTF8))
            {
                Transaction tx = new Transaction();
                tx.Deserialize(reader);
                return tx;
            }
        }

        private void MallocTrasnactionData(List<TransactionInput> inputs = null, List<TransactionOutput> outputs = null, List<MakerSignature> signatures = null)
        {
            switch (Type)
            {
                case eTransactionType.DataTransaction:
                    Data = new DataTransaction(this, inputs, outputs, signatures);
                    break;
                case eTransactionType.VoteTransaction:
                    Data = new VoteTransaction(this, inputs, outputs, signatures);
                    break;
                case eTransactionType.RegisterDelegateTransaction:
                    Data = new RegisterDelegateTransaction(this, inputs, outputs, signatures);
                    break;
                case eTransactionType.RewardTransaction:
                    Data = new RewardTransaction(this, inputs, outputs, signatures);
                    break;
                default:
                    Data = new TransactionBase(this, inputs, outputs, signatures);
                    break;
            }
        }

        public void DeserializeUnsigned(BinaryReader reader)
        {
            Version = reader.ReadInt16();
            Type = (eTransactionType)reader.ReadInt16();
            Timestamp = reader.ReadInt32();
            MallocTrasnactionData();
            Data.DeserializeUnsigned(reader);
        }

        public void SerializeUnsigned(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write((short)Type);
            writer.Write(Timestamp);
            Data.SerializeUnsigned(writer);
        }

        public virtual void Deserialize(BinaryReader reader)
        {
            Version = reader.ReadInt16();
            Type = (eTransactionType)reader.ReadInt16();
            Timestamp = reader.ReadInt32();
            MallocTrasnactionData();
            Data.Deserialize(reader);
        }

        public virtual void Serialize(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write((short)Type);
            writer.Write(Timestamp);
            Data.Serialize(writer);
        }

        public bool Verify()
        {
            if (Data.Verify() == false)
                return false;
            // blockchain database spent
            if (Blockchain.Instance.IsDoubleSpend(this))
                return false;
            return true;
        }
    }
}