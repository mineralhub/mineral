using System.IO;
using Sky.Cryptography;
using System.Text;
using Sky.Wallets;
using Newtonsoft.Json.Linq;
using Sky.Database.LevelDB;

namespace Sky.Core
{
    public class Transaction : IVerifiable
    {
        public short Version;
        public eTransactionType Type;
        public int Timestamp;
        public TransactionBase Data;
        public MakerSignature Signature;

        public UInt160 From => Data.From;
        public Fixed8 Fee => Data.Fee;
        public ERROR_CODES TxResult = ERROR_CODES.E_NO_ERROR;

        public virtual int Size => sizeof(short) + sizeof(eTransactionType) + sizeof(int) + Data.Size + Signature.Size;
        private UInt256 _hash = null;
        public UInt256 Hash
        {
            get
            {
                if (_hash == null)
                    _hash = this.GetHash();
                return _hash;
            }
        }

        public Transaction(eTransactionType type, int timestamp)
        {
            Version = Config.Instance.TransactionVersion;
            Type = type;
            Timestamp = timestamp;
            MallocTrasnactionData();
        }

        public Transaction(eTransactionType type, int timestamp, TransactionBase txData)
        {
            Version = Config.Instance.TransactionVersion;
            Type = type;
            Timestamp = timestamp;
            Data = txData;
            Data.Owner = this;
        }

        public Transaction()
        {
            Version = Config.Instance.TransactionVersion;
        }

        static public Transaction DeserializeFrom(byte[] value, int offset = 0)
        {
            using (MemoryStream ms = new MemoryStream(value, offset, value.Length - offset, false))
            using (BinaryReader reader = new BinaryReader(ms, Encoding.UTF8))
            {
                Transaction tx = new Transaction();
                tx.Deserialize(reader);
                return tx;
            }
        }

        public byte[] ToUnsignedArray()
        {
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(ms, Encoding.UTF8))
            {
                SerializeUnsigned(writer);
                writer.Flush();
                return ms.ToArray();
            }
        }

        private void MallocTrasnactionData()
        {
            switch (Type)
            {
                case eTransactionType.TransferTransaction:
                    Data = new TransferTransaction();
                    break;
                case eTransactionType.VoteTransaction:
                    Data = new VoteTransaction();
                    break;
                case eTransactionType.RegisterDelegateTransaction:
                    Data = new RegisterDelegateTransaction();
                    break;
                case eTransactionType.RewardTransaction:
                    Data = new RewardTransaction();
                    break;
                case eTransactionType.LockTransaction:
                    Data = new LockTransaction();
                    break;
                case eTransactionType.UnlockTransaction:
                    Data = new UnlockTransaction();
                    break;
                default:
                    Data = new TransactionBase();
                    break;
            }
            Data.Owner = this;
        }

        public void DeserializeUnsigned(BinaryReader reader)
        {
            Version = reader.ReadInt16();
            Type = (eTransactionType)reader.ReadInt16();
            Timestamp = reader.ReadInt32();
            MallocTrasnactionData();
            Data.Deserialize(reader);
        }

        public void SerializeUnsigned(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write((short)Type);
            writer.Write(Timestamp);
            Data.Serialize(writer);
        }

        public virtual void Deserialize(BinaryReader reader)
        {
            DeserializeUnsigned(reader);
            Signature = reader.ReadSerializable<MakerSignature>();
        }

        public virtual void Serialize(BinaryWriter writer)
        {
            SerializeUnsigned(writer);
            writer.WriteSerializable(Signature);
        }

        public void Sign(WalletAccount account)
        {
            Sign(account.Key);
        }

        public void Sign(ECKey key)
        {
            Signature = new MakerSignature(Cryptography.Helper.Sign(ToUnsignedArray().SHA256(), key), key.PublicKey.ToByteArray());
        }

        public bool VerifySignature()
        {
            return Cryptography.Helper.VerifySignature(Signature, ToUnsignedArray().SHA256());
        }

        public bool Verify()
        {
            if (VerifySignature() == false)
            {
                TxResult = ERROR_CODES.E_TX_SIGNATURE_INVALID;
                return false;
            }
            if (!Data.Verify())
            {
                TxResult = Data.TxResult;
                return false;
            }
            return true;
        }

        public bool VerifyBlockchain(Storage storage = null)
        {
            if (storage == null)
                storage = Blockchain.Instance.storage;

            if (storage.GetTransaction(Hash) != null)
            {
                TxResult = ERROR_CODES.E_SYS_EXIST_TRANSACTION;
                return false;
            }
            if (!Data.VerifyBlockchain(storage))
            {
                TxResult = Data.TxResult;
                return false;
            }
            return true;
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["version"] = Version;
            json["type"] = (short)Type;
            json["timestamp"] = Timestamp;
            json["data"] = Data.ToJson();
            json["signature"] = Signature.ToJson();
            json["hash"] = Hash.ToString();
            return json;
        }
    }
}