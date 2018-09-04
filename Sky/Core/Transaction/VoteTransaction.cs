using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Sky.Core
{
    public class VoteTransaction : TransactionBase
    {
        public Dictionary<UInt160, Fixed8> Votes { get; private set; }
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

        public override bool Verify(ulong accountNonce)
        {
            if (!base.Verify(accountNonce))
                return false;
            return Votes.Select(p => p.Value).Sum() <= FromAccountState.LockBalance;
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
