using System;
using System.IO;
using Sky.Cryptography;
using System.Text;
using Sky.Wallets;

namespace Sky.Core
{
    public class Transaction : IVerifiable
    {
        public short Version;
        public eTransactionType Type;
        public int Timestamp;
        public UInt64 AccountNonce;
        public TransactionBase Data;
        public MakerSignature Signature;

        public UInt160 From => Data.From;
        public Fixed8 Fee => Data.Fee;

        public virtual int Size => sizeof(short) + sizeof(eTransactionType) + sizeof(int) + Data.Size + Signature.Size;
        public UInt256 Hash => this.GetHash();

        public Transaction(eTransactionType type, int timestamp)
        {
            Version = Config.TransactionVersion;
            Type = type;
            Timestamp = timestamp;
            MallocTrasnactionData();
        }

        public Transaction(eTransactionType type, int timestamp, TransactionBase txData)
        {
            Version = Config.TransactionVersion;
            Type = type;
            Timestamp = timestamp;
            Data = txData;
            Data.Owner = this;
        }

        public Transaction()
        {
            Version = Config.TransactionVersion;
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
            AccountNonce = reader.ReadUInt64();
            MallocTrasnactionData();
            Data.Deserialize(reader);
        }

        public void SerializeUnsigned(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write((short)Type);
            writer.Write(Timestamp);
            writer.Write(AccountNonce);
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
            AccountNonce = Data.FromAccountState == null ? 0 : Data.FromAccountState.Nonce;
            Signature = new MakerSignature(Cryptography.Helper.Sign(ToUnsignedArray(), key), key.PublicKey.ToByteArray());
        }

        public bool Verify()
        {
            if (Data.Verify() == false)
                return false;
            return true;
        }
    }
}