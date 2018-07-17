using System.Collections.Generic;
using System.IO;
using System.Text;
using Sky.Cryptography;

namespace Sky.Core
{
    public class Transaction : IVerifiable
    {
        public short Version { get; protected set; }
        public eTransactionType Type { get; protected set; }
        public int Timestamp { get; protected set; }
        public Fixed8 Fee { get; protected set; }
        public List<TransactionInput> Inputs { get; protected set; }
        public List<TransactionOutput> Outputs { get; protected set; }
        public List<MakerSignature> Signatures { get; protected set; }
        public UInt256 Hash => this.GetHash();

        // cache refs
        private List<TransactionOutput> _referense;
        public List<TransactionOutput> References
        {
            get
            {
                if (_referense == null)
                {
                    _referense = new List<TransactionOutput>();
                    foreach (var group in Inputs)
                    {
                        Transaction tx = Blockchain.Instance.GetTransaction(group.PrevHash);
                        if (tx == null)
                            return null;
                        _referense.Add(tx.Outputs[group.PrevIndex]);
                    }
                }
                return _referense;
            }
        }

        public virtual int Size => sizeof(eTransactionType) + sizeof(short) + sizeof(int) +
            Fee.Size + Outputs.GetSize();

        public Transaction(short version, eTransactionType type, int timestamp, List<TransactionInput> inputs, List<TransactionOutput> outputs, List<MakerSignature> signatures)
        {
            Version = version;
            Type = type;
            Timestamp = timestamp;
            Inputs = inputs;
            Outputs = outputs;
            Signatures = signatures;
        }

        public Transaction()
        {
        }

        static public Transaction DeserializeFrom(byte[] value, int offset = 0)
        {
            using (MemoryStream ms = new MemoryStream(value, offset, value.Length - offset, false))
            using (BinaryReader reader = new BinaryReader(ms, Encoding.UTF8))
            {
                Transaction tx = null;
                short version = reader.ReadInt16();
                eTransactionType type = (eTransactionType)reader.ReadInt16();
                switch (type)
                {
                    case eTransactionType.DataTransaction:
                        tx = new DataTransaction();
                        break;
                    case eTransactionType.VoteTransaction:
                        tx = new VoteTransaction();
                        break;
                    case eTransactionType.RegisterDelegateTransaction:
                        tx = new RegisterDelegateTransaction();
                        break;
                    default:
                        tx = new Transaction();
                        break;
                }
                reader.BaseStream.Position = 0;
                tx.Deserialize(reader);
                tx.CalcFee();
                return tx;
            }
        }

        public void DeserializeUnsigned(BinaryReader reader)
        {
            Version = reader.ReadInt16();
            Type = (eTransactionType)reader.ReadInt16();
            Timestamp = reader.ReadInt32();
            Fee = reader.ReadSerializable<Fixed8>();
            Inputs = reader.ReadSerializableArray<TransactionInput>();
            Outputs = reader.ReadSerializableArray<TransactionOutput>();
        }

        public void SerializeUnsigned(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write((short)Type);
            writer.Write(Timestamp);
            writer.WriteSerializable(Fee);
            writer.WriteSerializableArray(Inputs);
            writer.WriteSerializableArray(Outputs);
        }

        public virtual void Deserialize(BinaryReader reader)
        {
            DeserializeUnsigned(reader);
            Signatures = reader.ReadSerializableArray<MakerSignature>();
        }

        public virtual void Serialize(BinaryWriter writer)
        {
            SerializeUnsigned(writer);
            writer.WriteSerializableArray(Signatures);
        }

        public virtual void CalcFee()
        {
            Fee = Config.DefaultFee;
        }

        public TransactionResult GetTransactionResult()
        {
            if (References == null)
                return null;

            return new TransactionResult(References.Sum(p => p.Value) - Outputs.Sum(p => p.Value));
        }

        public virtual bool Verify()
        {
            if (Type == eTransactionType.RewardTransaction)
            {
                // zero input
                if (0 < Inputs.Count)
                    return false;
                // single output
                if (1 < Outputs.Count)
                    return false;
                // block reward
                if (Outputs[0].Value != Config.BlockReward)
                    return false;
                return true;
            }
            // required input
            if (Inputs.Count == 0)
                return false;
            // signature count
            if (Signatures.Count != Inputs.Count)
                return false;
            // multiple input
            for (int i = 1; i < Inputs.Count; i++)
                for (int j = 0; j < i; j++)
                    if (Inputs[i].PrevHash == Inputs[j].PrevHash && Inputs[i].PrevIndex == Inputs[j].PrevIndex)
                        return false;
            // has value
            TransactionResult result = GetTransactionResult();
            if (result == null || result.Amount < Fee)
                return false;
            // sign
            for (int i = 0; i < Inputs.Count; ++i)
            {
                if (!Cryptography.Helper.VerifySignature(Signatures[i], Inputs[i].Hash.Data))
                    return false;
            }
            // blockchain database spent
            if (!Blockchain.Instance.IsDoubleSpend(this))
                return false;
            return true;
        }
    }
}