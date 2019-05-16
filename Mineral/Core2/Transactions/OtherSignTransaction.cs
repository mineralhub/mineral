using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using Mineral.Database.LevelDB;
using Mineral.Utils;

namespace Mineral.Core2.Transactions
{
    public class OtherSignTransaction : TransactionBase
    {
        public Dictionary<UInt160, Fixed8> To;
        public HashSet<string> Others;
        public uint ExpirationBlockHeight;

        public override int Size => base.Size + To.GetSize() + Others.GetSize() + sizeof(uint);

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            To = reader.ReadSerializableDictionary<UInt160, Fixed8>(Config.Instance.OtherSignToMaxLength);
            Others = reader.ReadStringHashSet();
            ExpirationBlockHeight = reader.ReadUInt32();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteSerializableDictonary(To);
            writer.WriteStringHashSet(Others);
            writer.Write(ExpirationBlockHeight);
        }

        public override void CalcFee()
        {
            Fee = Config.Instance.DefaultFee;
        }

        public override bool Verify()
        {
            if (!base.Verify())
                return false;
            foreach (Fixed8 v in To.Values)
                if (v < Fixed8.Satoshi)
                    return false;
            if (Others.Count == 0)
                return false;
            if (Config.Instance.OtherSignMaxLength < Others.Count)
                return false;
            foreach (string addr in Others)
                if (!Wallets.WalletAccount.IsAddress(addr))
                    return false;
            return true;
        }

        public override bool VerifyBlockChain(Storage storage)
        {
            if (!base.VerifyBlockChain(storage))
                return false;

            if (ExpirationBlockHeight < BlockChain.Instance.CurrentBlockHeight)
                return false;

            if (FromAccountState.Balance - Fee - To.Sum(p => p.Value) < Fixed8.Zero)
                return false;

            return true;
        }

        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            JArray to = new JArray();
            foreach (var v in To)
            {
                var j = new JObject
                {
                    ["addr"] = v.Key.ToString(),
                    ["amount"] = v.Value.ToString()
                };
                to.Add(j);
            }
            json["to"] = to;
            json["others"] = JToken.FromObject(Others);
            json["expirationblockheight"] = ExpirationBlockHeight;
            return json;
        }
    }
}