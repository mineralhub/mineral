using System.IO;
using Newtonsoft.Json.Linq;

namespace Sky.Core
{
    public class RegisterDelegateTransaction : TransactionBase
    {
        public byte[] Name;

        public override int Size => base.Size + Name.GetSize();

        public override void CalcFee()
        {
            Fee = Config.RegisterDelegateFee;
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
            {
                TxResult = ERROR_CODES.E_TX_SIGNATURE_INVALID;
                return false;
            }

            if (Name == null || Name.Length == 0)
            {
                TxResult = ERROR_CODES.E_TX_DELEGATE_NAME_INVALID;
                return false;
            }

            if (Config.DelegateNameMaxLength < Name.Length)
            {
                TxResult = ERROR_CODES.E_TX_DELEGATE_NAME_INVALID;
                return false;
            }
            return true;
        }

        public override bool VerifyBlockchain()
        {
            if (!base.VerifyBlockchain())
                return false;
            if (FromAccountState.Balance - Fee < Fixed8.Zero)
                return false;
            return true;
        }

        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["name"] = Name;
            return json;
        }
    }
}
