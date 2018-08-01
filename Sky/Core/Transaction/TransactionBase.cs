using System.Collections.Generic;
using System.IO;
using Sky.Cryptography;

namespace Sky.Core
{
    public class TransactionBase : IVerifiable
    {
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
                        _referense.Add(tx.Data.Outputs[group.PrevIndex]);
                    }
                }
                return _referense;
            }
        }

        public virtual int Size => Fee.Size + Outputs.GetSize();

        public TransactionBase(List<TransactionInput> inputs, List<TransactionOutput> outputs, List<MakerSignature> signatures)
        {
            Inputs = inputs;
            Outputs = outputs;
            Signatures = signatures;
        }

        public TransactionBase() { }

        public virtual bool Verify()
        {
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
            return true;
        }

        public virtual void DeserializeUnsigned(BinaryReader reader)
        {
            Fee = reader.ReadSerializable<Fixed8>();
            Inputs = reader.ReadSerializableArray<TransactionInput>();
            Outputs = reader.ReadSerializableArray<TransactionOutput>();
        }

        public virtual void SerializeUnsigned(BinaryWriter writer)
        {
            writer.WriteSerializable(Fee);
            writer.WriteSerializableArray(Inputs);
            writer.WriteSerializableArray(Outputs);
        }

        public virtual void Serialize(BinaryWriter writer)
        {
            SerializeUnsigned(writer);
            writer.WriteSerializableArray(Signatures);
        }

        public virtual void Deserialize(BinaryReader reader)
        {
            DeserializeUnsigned(reader);
            Signatures = reader.ReadSerializableArray<MakerSignature>();
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
    }
}
