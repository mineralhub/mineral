using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Sky.Core
{
    public class OtherSignTransaction : TransactionBase
    {
        public UInt160 To;
        public Fixed8 Amount;
        public HashSet<string> Others;
        public int ValidBlockHeight;

        public override int Size => base.Size + To.Size + Amount.Size + Others.GetSize() + sizeof(int);

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            To = reader.ReadSerializable<UInt160>();
            Amount = reader.ReadSerializable<Fixed8>();
            Others = reader.ReadStringHashSet();
            ValidBlockHeight = reader.ReadInt32();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteSerializable(To);
            writer.WriteSerializable(Amount);
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
            if (From == To)
                return false;
            if (Amount < Fixed8.Satoshi)
                return false;
            if (Others.Count == 0)
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
            json["amount"] = Amount.Value;
            json["Others"] = new JObject();
            foreach (string other in Others)
                json["Others"].AddAfterSelf(other);
            json["validblockheight"] = ValidBlockHeight;
            return json;
        }
    }
}