using System;
using System.IO;
using Sky.Cryptography;
using Newtonsoft.Json.Linq;

namespace Sky.Core
{
    public class TransactionBase : ISerializable
    {
        public Fixed8 Fee;
        public UInt160 From;

        public Transaction Owner;

        private AccountState _fromAccountState;
        public AccountState FromAccountState 
        {
            get 
            {
                if (_fromAccountState == null)
                {
                    _fromAccountState = Blockchain.Instance.GetAccountState(From);
                    if (_fromAccountState == null)
                        _fromAccountState = new AccountState(From);
                }
                    
                return _fromAccountState;
            }
        }

        public virtual int Size => Fee.Size + From.Size + sizeof(UInt64);

        public TransactionBase()
        {
            
        }

        public virtual bool Verify()
        {
            if (From == null)
                return false;
            if (FromAccountState == null)
                return false;
            FromAccountState.AddBalance(-Fee);
            if (FromAccountState.Balance < Fixed8.Zero)
                return false;
            if (!Cryptography.Helper.VerifySignature(Owner.Signature, Owner.ToUnsignedArray()))
                return false;
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
