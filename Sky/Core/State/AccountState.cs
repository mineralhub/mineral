using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;

namespace Sky.Core
{
    public class AccountState : StateBase
    {
        public UInt160 AddressHash { get; private set; }
        public bool IsFrozen { get; private set; }
        public Fixed8 Balance { get; private set; }
        public Fixed8 LockBalance { get; private set; }
        public Dictionary<UInt160, Fixed8> Votes { get; private set; }

        public override int Size => base.Size + AddressHash.Size + sizeof(bool) + Balance.Size;

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
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            AddressHash = reader.ReadSerializable<UInt160>();
            IsFrozen = reader.ReadBoolean();
            Balance = reader.ReadSerializable<Fixed8>();
            LockBalance = reader.ReadSerializable<Fixed8>();
            Votes = reader.ReadSerializableDictionary<UInt160, Fixed8>();
        }

        public override void Serialize(BinaryWriter writer)
        {
            base.Serialize(writer);
            writer.WriteSerializable(AddressHash);
            writer.Write(IsFrozen);
            writer.WriteSerializable(Balance);
            writer.WriteSerializable(LockBalance);
            writer.WriteSerializableDictonary(Votes);
        }

        public void AddBalance(Fixed8 value)
        {
            Balance += value;
        }

        public void SetVote(Dictionary<UInt160, Fixed8> vote)
        {
            Votes = vote;
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
                json[Wallets.WalletAccount.ToAddress(v.Key)] = v.Value.ToString();
            json["votes"] = votes;
            return json;
        }
    }
}
