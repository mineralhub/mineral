using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using Mineral.Database.LevelDB;

namespace Mineral.Core
{
    public class RegisterDelegateTransaction : TransactionBase
    {
        public byte[] Name;

        public override int Size => base.Size + Name.GetSize();

        public override void CalcFee()
        {
            Fee = Config.Instance.RegisterDelegateFee;
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Name = reader.ReadByteArray();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteByteArray(Name);
        }


        public override bool Verify()
        {
            if (!base.Verify())
                return false;

            if (Name == null || Name.Length == 0)
            {
                TxResult = ErrorCodes.E_TX_DELEGATE_NAME_INVALID;
                return false;
            }

            if (Config.Instance.DelegateNameMaxLength < Name.Length)
            {
                TxResult = ErrorCodes.E_TX_DELEGATE_NAME_INVALID;
                return false;
            }
            return true;
        }

        public override bool VerifyBlockchain(Storage storage)
        {
            if (!base.VerifyBlockchain(storage))
                return false;

            if (storage.GetDelegateState(From) != null)
            {
                TxResult = ErrorCodes.E_TX_DELEGATE_ALREADY_REGISTER;
                return false;
            }
            return true;
        }

        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["name"] = Encoding.UTF8.GetString(Name);
            return json;
        }
    }
}
