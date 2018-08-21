using System.IO;

namespace Sky.Core
{
    public class RewardTransaction : TransactionBase
    {
        public Fixed8 Reward;

        public override int Size => base.Size + Reward.Size;

        public override void CalcFee()
        {
            Fee = Fixed8.Zero;
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Reward = reader.ReadSerializable<Fixed8>();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteSerializable(Reward);
        }

        public override bool Verify()
        {
            if (!base.Verify())
                return false;
            if (Reward != Config.BlockReward)
                return false;
            return true;
        }
    }
}
