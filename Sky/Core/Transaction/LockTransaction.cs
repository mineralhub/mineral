using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using Sky.Database.LevelDB;

namespace Sky.Core
{
    public class UnlockTransaction : TransactionBase
    {
        public override bool VerifyBlockchain(Storage storage)
        {
            if (!base.VerifyBlockchain(storage))
                return false;

            if (FromAccountState.LockBalance == Fixed8.Zero)
            {
                TxResult = ERROR_CODES.E_TX_NO_LOCK_BALANCE;
                return false;
            }

            if (FromAccountState.LastLockTxID != UInt256.Zero)
            {
                int TxHeight = 0;
                Transaction txLast = storage.GetTransaction(FromAccountState.LastLockTxID, out TxHeight);
                if (Blockchain.Instance.CurrentBlockHeight - TxHeight < Config.Instance.LockTTL)
                {
                    TxResult = ERROR_CODES.E_TX_LOCK_TTL_NOT_ARRIVED;
                    return false;
                }
            }

            return true;
        }
    }

    public class LockTransaction : TransactionBase
    {
        public Fixed8 LockValue = Fixed8.Zero;
        public override int Size => base.Size + LockValue.Size;

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            LockValue = reader.ReadSerializable<Fixed8>();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteSerializable(LockValue);
        }

        public override bool Verify()
        {
            return base.Verify();
        }

        public override bool VerifyBlockchain(Storage storage)
        {
            if (!base.VerifyBlockchain(storage))
                return false;

            if (LockValue < Fixed8.Zero)
            {
                TxResult = ERROR_CODES.E_TX_LOCK_VALUE_CANNOT_NEGATIVE;
                return false;
            }

            if (FromAccountState.LastLockTxID != UInt256.Zero)
            {
                int TxHeight = 0;
                Transaction txLast = storage.GetTransaction(FromAccountState.LastLockTxID, out TxHeight);
                if (Blockchain.Instance.CurrentBlockHeight - TxHeight < Config.Instance.LockTTL)
                {
                    TxResult = ERROR_CODES.E_TX_LOCK_TTL_NOT_ARRIVED;
                    return false;
                }
            }

            if (FromAccountState.Balance - LockValue - Fee < Fixed8.Zero)
            {
                TxResult = ERROR_CODES.E_TX_NOT_ENOUGH_BALANCE;
                return false;
            }
            return true;
        }

        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["locks"] = LockValue.Value;
            return json;
        }
    }
}
