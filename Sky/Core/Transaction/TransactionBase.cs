using System.IO;
using Newtonsoft.Json.Linq;
using Sky.Database.LevelDB;

namespace Sky.Core
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
                    _fromAccountState = _storage.GetAccountState(From);

                return _fromAccountState;
            }
        }

        public virtual int Size => Fee.Size + From.Size;

        public virtual ERROR_CODES TxResult { get; protected set; } = ERROR_CODES.E_NO_ERROR;

        public virtual bool Verify()
        {
            if (From == null)
            {
                TxResult = ERROR_CODES.E_TX_FROM_ADDRESS_INVALID;
                return false;
            }
            return true;
        }

        public virtual bool VerifyBlockchain(Storage storage)
        {
            UsingStorage(storage);
            if (FromAccountState == null)
            {
                TxResult = ERROR_CODES.E_TX_FROM_ACCOUNT_INVALID;
                return false;
            }
            if (FromAccountState.Balance - Fee < Fixed8.Zero)
            {
                TxResult = ERROR_CODES.E_TX_NOT_ENOUGH_BALANCE;
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
            Fee = Config.DefaultFee;
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
