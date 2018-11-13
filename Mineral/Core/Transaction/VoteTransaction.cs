using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using Mineral.Database.LevelDB;

namespace Mineral.Core
{
    public class VoteTransaction : TransactionBase
    {
        public Dictionary<UInt160, Fixed8> Votes;
        public override int Size => base.Size + Votes.GetSize();

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            Votes = reader.ReadSerializableDictionary<UInt160, Fixed8>();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteSerializableDictonary(Votes);
        }

        public override bool Verify()
        {
            if (!base.Verify())
                return false;

            if (Config.Instance.VoteMaxLength < Votes.Count)
            {
                TxResult = ERROR_CODES.E_TX_VOTE_OVERCOUNT;
                return false;
            }

            return true;
        }

        public override bool VerifyBlockchain(Storage storage)
        {
            if (!base.VerifyBlockchain(storage))
                return false;

            int TxHeight = 0;

            if (FromAccountState.LastVoteTxID != UInt256.Zero)
            {
                if (Blockchain.Instance.HasTransactionPool(FromAccountState.LastLockTxID))
                {
                    TxHeight = Blockchain.Instance.CurrentBlockHeight;
                }
                else
                {
                    storage.GetTransaction(FromAccountState.LastVoteTxID, out TxHeight);
                }
                if (Blockchain.Instance.CurrentBlockHeight - TxHeight < Config.Instance.VoteTTL)
                {
                    TxResult = ERROR_CODES.E_TX_VOTE_TTL_NOT_ARRIVED;
                    return false;
                }
            }

            foreach(var vote in Votes)
            {
                if (storage.GetDelegateState(vote.Key) == null)
                {
                    TxResult = ERROR_CODES.E_TX_DELEGATE_NOT_REGISTERED;
                    return false;
                }
                if (vote.Value == Fixed8.Zero)
                {
                    TxResult = ERROR_CODES.E_TX_ZERO_VOTE_VALUE_NOT_ALLOWED;
                    return false;
                }
            }

            if (FromAccountState.LockBalance - Votes.Sum(p => p.Value) < Fixed8.Zero)
            {
                TxResult = ERROR_CODES.E_TX_NOT_ENOUGH_LOCKBALANCE;
                return false;
            }

            return true;
        }

        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["votes"] = new JObject();
            foreach (var v in Votes)
                json["votes"][v.Key.ToString()] = v.Value.Value.ToString();
            return json;
        }
    }
}
