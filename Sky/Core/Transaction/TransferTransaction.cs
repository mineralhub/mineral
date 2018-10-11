using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Sky.Database.LevelDB;

namespace Sky.Core
{
    public class TransferTransaction : TransactionBase
    {
        public Dictionary<UInt160, Fixed8> To;

        public override int Size => base.Size + To.GetSize();

        public override void CalcFee()
        {
            Fee = Config.DefaultFee;
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            To = reader.ReadSerializableDictionary<UInt160, Fixed8>(Config.TransferToMaxLength);
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteSerializableDictonary(To);
        }

        public override bool Verify()
        {
            if (!base.Verify())
                return false;

            if (To.ContainsKey(From))
            {
                TxResult = ERROR_CODES.E_TX_SELF_TRANSFER_NOT_ALLOWED;
                return false;
            }

            foreach (Fixed8 v in To.Values)
                if (v < Fixed8.Satoshi)
                {
                    TxResult = ERROR_CODES.E_TX_TOO_SMALL_TRANSFER_BALANCE;
                    return false;
                }
            return true;
        }

        public override bool VerifyBlockchain(Storage storage)
        {
            if (!base.VerifyBlockchain(storage))
                return false;

            if (FromAccountState.Balance - Fee - To.Sum(p => p.Value) < Fixed8.Zero)
            {
                TxResult = ERROR_CODES.E_TX_NOT_ENOUGH_BALANCE;
                return false;
            }
            return true;
        }

        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            JArray to = new JArray();
            foreach (var v in To)
            {
                var j = new JObject();
                j["addr"] = v.Key.ToString();
                j["amount"] = v.Value.ToString();
                to.Add(j);
            }
            json["to"] = to;
            return json;
        }
    }
}
