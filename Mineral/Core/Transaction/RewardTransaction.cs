﻿using System.IO;
using Newtonsoft.Json.Linq;
using Mineral.Database.LevelDB;

namespace Mineral.Core
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
            if (Reward != Config.Instance.BlockReward)
                return false;
            return true;
        }

        public override bool VerifyBlockchain(Storage storage)
        {
            return base.VerifyBlockchain(storage);
        }

        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["reward"] = Reward.Value;
            return json;
        }
    }
}