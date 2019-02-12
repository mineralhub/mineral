using System.IO;
using Newtonsoft.Json.Linq;
using Mineral.Database.LevelDB;
using Mineral.Utils;

namespace Mineral.Core.Transactions
{
    public class SupplyTransaction : TransactionBase
    {
        public Fixed8 Supply;

        public override int Size => base.Size + Supply.Size;

        public override void CalcFee()
        {
            Fee = Fixed8.Zero;
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Supply = reader.ReadSerializable<Fixed8>();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteSerializable(Supply);
        }

        public override bool Verify()
        {
            if (!base.Verify())
                return false;
            return true;
        }

        public override bool VerifyBlockChain(Storage storage)
        {
            if (0 < BlockChain.Instance.CurrentBlockHeight)
                return false;

            return base.VerifyBlockChain(storage);
        }

        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["supply"] = Supply.Value;
            return json;
        }
    }
}
