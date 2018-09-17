using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Sky.Core
{
    public class OtherSignTransaction : TransactionBase
    {
        public Dictionary<UInt160, Fixed8> To;
        public HashSet<string> Others;
        public int ExpirationBlockHeight;

        public override int Size => base.Size + To.GetSize() + Others.GetSize() + sizeof(int);

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            To = reader.ReadSerializableDictionary<UInt160, Fixed8>(Config.OtherSignToMaxLength);
            Others = reader.ReadStringHashSet();
            ExpirationBlockHeight = reader.ReadInt32();
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
            Fee = Config.DefaultFee;
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
            if (Config.OtherSignMaxLength < Others.Count)
                return false;
            foreach (string addr in Others)
                if (!Wallets.WalletAccount.IsAddress(addr))
                    return false;
            return true;
        }

        public override bool VerifyBlockchain()
        {
            if (!base.VerifyBlockchain())
                return false;
            if (ExpirationBlockHeight < Blockchain.Instance.CurrentBlockHeight)
                return false;
            if (FromAccountState.Balance - To.Sum(p => p.Value) < Fixed8.Zero)
                return false;
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
            json["others"] = new JObject();
            foreach (string other in Others)
                json["others"].AddAfterSelf(other);
            json["expirationblockheight"] = ExpirationBlockHeight;
            return json;
        }
    }
}