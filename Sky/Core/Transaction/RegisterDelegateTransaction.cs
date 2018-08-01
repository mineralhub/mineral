using System.Collections.Generic;
using System.IO;

namespace Sky.Core
{
    public class RegisterDelegateTransaction : TransactionBase
    {
        public UInt160 Sender { get; private set; }
        public byte[] NameBytes { get; private set; }

        public override int Size => base.Size + Sender.Size + NameBytes.GetSize();

        public RegisterDelegateTransaction()
        {
        }

        public RegisterDelegateTransaction(List<TransactionInput> inputs, List<TransactionOutput> outputs, List<MakerSignature> signatures)
            : base(inputs, outputs, signatures)
        {
        }

        public RegisterDelegateTransaction(List<TransactionInput> inputs, List<TransactionOutput> outputs, List<MakerSignature> signatures, UInt160 sender, byte[] name)
            : base(inputs, outputs, signatures)
        {
            Sender = sender;
            NameBytes = name;
        }

        public override void CalcFee()
        {
            Fee = Config.RegisterDelegateFee;
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Sender = reader.ReadSerializable<UInt160>();
            NameBytes = reader.ReadByteArray();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteSerializable(Sender);
            writer.WriteByteArray(NameBytes);
        }

        public override bool Verify()
        {
            if (!base.Verify())
                return false;
            if (Sender != References[0].AddressHash)
                return false;
            if (NameBytes == null || NameBytes.Length == 0)
                return false;
            return true;
        }
    }
}
