using System.IO;
using Newtonsoft.Json.Linq;
using Mineral.Database.LevelDB;
using Mineral.Utils;
using Mineral.Core.State;

namespace Mineral.Core.Transactions
{
    public class TransactionBase : ISerializable
    {
        public Fixed8 Fee;
        public UInt160 From;

        public Transaction Owner;

        private Storage _storage;

        public void UsingStorage(Storage storage)
        {
            _storage = storage;
            _fromAccountState = null;
        }

        private AccountState _fromAccountState;
        public AccountState FromAccountState
        {
            get
            {
                if (_fromAccountState == null)
                    _fromAccountState = _storage.Account.GetAndChange(From);

                return _fromAccountState;
            }
        }

        public virtual int Size => Fee.Size + From.Size;

        public virtual MINERAL_ERROR_CODES TxResult { get; protected set; } = MINERAL_ERROR_CODES.NO_ERROR;

        public virtual bool Verify()
        {
            if (From == null)
            {
                TxResult = MINERAL_ERROR_CODES.TX_FROM_ADDRESS_INVALID;
                return false;
            }
            Fixed8 oFee = Fee;
            CalcFee();
            if ((oFee - Fee) != Fixed8.Zero)
            {
                TxResult = MINERAL_ERROR_CODES.TX_FEE_VALUE_MISMATCH;
                return false;
            }
            return true;
        }

        public virtual bool VerifyBlockChain(Storage storage)
        {
            UsingStorage(storage);
            if (FromAccountState == null)
            {
                TxResult = MINERAL_ERROR_CODES.TX_FROM_ACCOUNT_INVALID;
                return false;
            }
            if (FromAccountState.Balance - Fee < Fixed8.Zero)
            {
                TxResult = MINERAL_ERROR_CODES.TX_NOT_ENOUGH_BALANCE;
                return false;
            }
            return true;
        }

        public virtual void Serialize(BinaryWriter writer)
        {
            writer.WriteSerializable(Fee);
            writer.WriteSerializable(From);
        }

        public virtual void Deserialize(BinaryReader reader)
        {
            Fee = reader.ReadSerializable<Fixed8>();
            From = reader.ReadSerializable<UInt160>();
        }

        public virtual void CalcFee()
        {
            Fee = Config.Instance.DefaultFee;
        }

        public virtual JObject ToJson()
        {
            JObject json = new JObject();
            json["fee"] = Fee.Value;
            json["from"] = From.ToString();
            return json;
        }
    }
}
