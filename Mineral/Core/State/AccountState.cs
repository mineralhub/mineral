using Newtonsoft.Json.Linq;
using System.IO;
using System.Collections.Generic;

namespace Mineral.Core
{
    public class AccountState : StateBase
    {
        public UInt160 AddressHash { get; private set; }
        public bool IsFrozen { get; private set; }
        public Fixed8 Balance { get; private set; }
        public Fixed8 LockBalance { get; private set; }
        public Fixed8 TotalBalance => Balance + LockBalance;
        public Dictionary<UInt160, Fixed8> Votes { get; private set; }
        public UInt256 LastVoteTxID { get; set; }
        public UInt256 LastLockTxID { get; set; }

        public override int Size => base.Size + AddressHash.Size + sizeof(bool) + Balance.Size + LastVoteTxID.Size + LastLockTxID.Size;

        public AccountState()
        {
            Votes = new Dictionary<UInt160, Fixed8>();
        }

        public AccountState(UInt160 hash)
        {
            AddressHash = hash;
            IsFrozen = false;
            Balance = Fixed8.Zero;
            LockBalance = Fixed8.Zero;
            Votes = new Dictionary<UInt160, Fixed8>();
            LastVoteTxID = UInt256.Zero;
            LastLockTxID = UInt256.Zero;
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            AddressHash = reader.ReadSerializable<UInt160>();
            IsFrozen = reader.ReadBoolean();
            Balance = reader.ReadSerializable<Fixed8>();
            LockBalance = reader.ReadSerializable<Fixed8>();
            Votes = reader.ReadSerializableDictionary<UInt160, Fixed8>();
            LastVoteTxID = reader.ReadSerializable<UInt256>();
            LastLockTxID = reader.ReadSerializable<UInt256>();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteSerializable(AddressHash);
            writer.Write(IsFrozen);
            writer.WriteSerializable(Balance);
            writer.WriteSerializable(LockBalance);
            writer.WriteSerializableDictonary(Votes);
            writer.WriteSerializable(LastVoteTxID);
            writer.WriteSerializable(LastLockTxID);
        }

        public void AddBalance(Fixed8 value)
        {
            Balance += value;
        }

        public void SetVote(Dictionary<UInt160, Fixed8> vote)
        {
            Votes = vote;
        }

        public void AddLock(Fixed8 value)
        {
            LockBalance += value;
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["address"] = Wallets.WalletAccount.ToAddress(AddressHash);
            json["frozen"] = IsFrozen;
            json["balance"] = Balance.ToString();
            json["lockbalance"] = LockBalance.ToString();
            JObject votes = new JObject();
            foreach (var v in Votes)
                votes[Wallets.WalletAccount.ToAddress(v.Key)] = v.Value.ToString();
            json["votes"] = votes;
            json["lastVoteTxID"] = LastVoteTxID.ToString();
            json["lastLockTxID"] = LastLockTxID.ToString();
            return json;
        }
    }
}
