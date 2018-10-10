using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Sky.Core
{
    public class VoteTransaction : TransactionBase
    {
        public Dictionary<UInt160, Fixed8> Votes;
        public override int Size => base.Size + Votes.GetSize();

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Votes = reader.ReadSerializableDictionary<UInt160, Fixed8>();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteSerializableDictonary(Votes);
        }

        public override bool Verify()
        {
            if (!base.Verify())
                return false;

            if (Config.VoteMaxLength < Votes.Count)
            {
                TxResult = ERROR_CODES.E_TX_VOTE_OVERCOUNT;
                return false;
            }

            return true;
        }

        public override bool VerifyBlockchain()
        {
            if (!base.VerifyBlockchain())
                return false;

            Fixed8 LockValue = FromAccountState.LockBalance;

            foreach (var v in Votes)
            {
                LockValue -= v.Value;
                if (LockValue < Fixed8.Zero)
                {
                    TxResult = ERROR_CODES.E_TX_NOT_ENOUGH_LOCKBALANCE;
                    return false;
                }
            }
            return true;
        }

        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            JArray votes = new JArray();
            foreach (var v in Votes)
                votes[v.Key] = v.Value.Value;
            json["votes"] = votes;
            return json;
        }
    }
}
