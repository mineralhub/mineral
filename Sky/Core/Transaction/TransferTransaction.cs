using System.IO;

namespace Sky.Core
{
    public class TransferTransaction : TransactionBase
    {
        public UInt160 To;
        public Fixed8 Amount;

        public override int Size => base.Size + To.Size + Amount.Size;

        public override void CalcFee()
        {
            Fee = Config.DefaultFee;
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            To = reader.ReadSerializable<UInt160>();
            Amount = reader.ReadSerializable<Fixed8>();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteSerializable(To);
            writer.WriteSerializable(Amount);
        }

        public override bool Verify()
        {
            if (base.Verify() == false)
                return false;
            FromAccountState.AddBalance(-Amount);
            if (FromAccountState.Balance < Fixed8.Zero)
                return false;
            return true;
        }
    }
}
