using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Sky.Core
{
    public class OtherSignTransaction : TransactionBase
    {
        public Dictionary<UInt160, Fixed8> To;
        public HashSet<string> Others;
        public int ValidBlockHeight;

        public override int Size => base.Size + To.GetSize() + Others.GetSize() + sizeof(int);

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            To = reader.ReadSerializableDictionary<UInt160, Fixed8>(Config.OtherSignToMaxLength);
            Others = reader.ReadStringHashSet();
            ValidBlockHeight = reader.ReadInt32();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteSerializableDictonary(To);
            writer.WriteStringHashSet(Others);
            writer.Write(ValidBlockHeight);
        }

        public override void CalcFee()
        {
            Fee = Config.DefaultFee;
        }

        public override bool Verify()
        {
            if (!base.Verify())
                return false;
            Fixed8 totalAmount = Fixed8.Zero;
            foreach (Fixed8 v in To.Values)
            {
                if (v < Fixed8.Satoshi)
                    return false;
                totalAmount += v;
            }
            if (FromAccountState.Balance - totalAmount < Fixed8.Zero)
                return false;
            if (Others.Count == 0)
                return false;
            if (Config.OtherSignMaxLength < Others.Count)
                return false;
            if (ValidBlockHeight < Blockchain.Instance.CurrentBlockHeight)
                return false;
            foreach (string addr in Others)
            {
                if (Wallets.WalletAccount.IsAddress(addr))
                    return false;
            }
            return true;
        }

        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["to"] = To.ToString();
            json["Others"] = new JObject();
            foreach (string other in Others)
                json["Others"].AddAfterSelf(other);
            json["validblockheight"] = ValidBlockHeight;
            return json;
        }
    }
}