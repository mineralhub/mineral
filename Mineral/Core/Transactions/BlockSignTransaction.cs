using Mineral.Database.LevelDB;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Mineral.Core.Transactions
{
    public class BlockSignTransaction : TransactionBase
    {
        public BlockHeader Header;

        public override int Size => base.Size + Header.Size;

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Header = new BlockHeader();
            Header.Deserialize(reader);
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            Header.Serialize(writer);
        }

        public override void CalcFee()
        {
            Fee = Config.Instance.DefaultFee;
        }

        public override bool Verify()
        {
            if (!base.Verify())
                return false;
            return true;
        }

        public override bool VerifyBlockChain(Storage storage)
        {
            if (!base.VerifyBlockChain(storage))
                return false;

            return true;
        }

        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["header"] = Header.ToJson();
            return json;
        }
    }
}
