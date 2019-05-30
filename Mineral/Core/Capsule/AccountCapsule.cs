using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Mineral.Core.Database;
using Mineral.Utils;

namespace Mineral.Core.Capsule
{
    public class AccountCapsule : IProtoCapsule<Protocol.Account>, IComparable<AccountCapsule>
    {
        #region Field
        private Protocol.Account account = null;
        #endregion


        #region Property
        public Protocol.Account Instance { get { return this.account; } }
        public byte[] Data { get { return this.account.ToByteArray(); } }

        public Protocol.AccountType Type
        {
            get { return this.account.Type; }
        }

        public ByteString Address
        {
            get { return this.account.Address; }
        }

        public ByteString AccountName
        {
            get { return this.account.AccountName; }
        }

        public ByteString AccountId
        {
            get { return this.account.AccountId; }
        }

        public long Balance
        {
            get { return this.account.Balance; }
            set { this.account.Balance = value; }
        }

        public long Allowance
        {
            get { return this.account.Allowance; }
            set { this.account.Allowance = value; }
        }

        public long LatestOperationTime
        {
            get { return this.account.LatestOprationTime; }
            set { this.account.LatestOprationTime = value; }
        }

        public long LatestConsumeTime
        {
            get { return this.account.LatestConsumeTime; }
            set { this.account.LatestConsumeTime = value; }
        }

        public long LatestConsumeFreeTime
        {
            get { return this.account.LatestConsumeFreeTime; }
            set { this.account.LatestConsumeFreeTime = value; }
        }

        public long AcquiredDelegatedFrozenBalanceForBandwidth
        {
            get { return this.account.AcquiredDelegatedFrozenBalanceForBandwidth; }
            set { this.account.AcquiredDelegatedFrozenBalanceForBandwidth = value; }
        }

        public long AcquiredDelegatedFrozenBalanceForEnergy
        {
            get { return this.account.AccountResource.AcquiredDelegatedFrozenBalanceForEnergy; }
        }

        public long DelegatedFrozenBalanceForEnergy
        {
            get { return this.account.AccountResource.DelegatedFrozenBalanceForEnergy; }
        }

        public long DelegatedFrozenBalanceForBandwidth
        {
            get { return this.account.DelegatedFrozenBalanceForBandwidth; }
            set { this.account.DelegatedFrozenBalanceForBandwidth = value; }
        }
        #endregion


        #region Constructor
        public AccountCapsule(Protocol.Account account)
        {
            this.account = account;
        }

        public AccountCapsule(byte[] data)
        {
            try
            {
                this.account = Protocol.Account.Parser.ParseFrom(data);
            }
            catch (System.Exception e)
            {
                Logger.Debug(e.Message);
            }
        }

        public AccountCapsule(ByteString address, Protocol.AccountType account_type)
        {
            this.account = new Protocol.Account();
            this.account.Type = account_type;
            this.account.Address = address;
        }

        public AccountCapsule(ByteString address, ByteString account_name, Protocol.AccountType account_type)
        {
            this.account = new Protocol.Account();
            this.account.Type = account_type;
            this.account.AccountName = account_name;
            this.account.Address = address;
        }

        public AccountCapsule(ByteString account_name, ByteString address, Protocol.AccountType account_type, long balance)
        {
            this.account = new Protocol.Account();
            this.account.AccountName = account_name;
            this.account.Type = account_type;
            this.account.Address = address;
            this.account.Balance = balance;
        }

        public AccountCapsule(ByteString address, Protocol.AccountType account_type, long create_time, bool default_permission, Manager db_manager)
        {
            if (default_permission)
            {
                Protocol.Permission owner = CreateDefaultOwnerPermission(this.account.Address);
                Protocol.Permission active = CreateDefaultActivePermission(this.account.Address, db_manager);

                this.account = new Protocol.Account();
                this.account.Type = account_type;
                this.account.Address = address;
                this.account.CreateTime = create_time;
                this.account.OwnerPermission = owner;
                this.account.ActivePermission.Add(active);
            }
            else
            {
                this.account = new Protocol.Account();
                this.account.Type = account_type;
                this.account.Address = address;
                this.account.CreateTime = create_time;
            }
        }

        public AccountCapsule(Protocol.AccountCreateContract contract, long create_time, bool default_permission, Manager db_manager)
        {
            if (default_permission)
            {
                Protocol.Permission owner = CreateDefaultOwnerPermission(this.account.Address);
                Protocol.Permission active = CreateDefaultActivePermission(this.account.Address, db_manager);

                this.account = new Protocol.Account();
                this.account.Type = contract.Type;
                this.account.Address = contract.AccountAddress;
                this.account.CreateTime = create_time;
                this.account.OwnerPermission = owner;
                this.account.ActivePermission.Add(active);
            }
            else
            {
                this.account = new Protocol.Account();
                this.account.Type = contract.Type;
                this.account.Address = contract.AccountAddress;
                this.account.CreateTime = create_time;
            }
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private static ByteString GetActiveDefaultOperations(Manager db_manager)
        {
            return ByteString.CopyFrom(db_manager.DynamicPropertiesStore.GetActiveDefaultOperations());
        }
        #endregion


        #region External Method
        public void SetInstance(Protocol.Account account)
        {
            this.account = account;
        }

        public byte[] CreateDatabaseKey()
        {
            return this.account.Address.ToByteArray();
        }

        public string CreateReadableString()
        {
            return this.account.Address.ToByteArray().ToHexString();
        }

        public void AddDelegatedFrozenBalanceForBandwidth(long balance)
        {
            this.account.DelegatedFrozenBalanceForBandwidth += balance;
        }

        public void AddAcquiredDelegatedFrozenBalanceForBandwidth(long balance)
        {
            this.account.AcquiredDelegatedFrozenBalanceForBandwidth += balance;
        }

        public void AddAcquiredDelegatedFrozenBalanceForEnergy(long balance)
        {
            this.account.AccountResource.AcquiredDelegatedFrozenBalanceForEnergy += balance;
        }

        public void AddDelegatedFrozenBalanceForEnergy(long balance)
        {
            this.account.AccountResource.DelegatedFrozenBalanceForEnergy += balance;
        }

        public static Protocol.Permission CreateDefaultOwnerPermission(ByteString address)
        {
            Protocol.Key key = new Protocol.Key();
            key.Address = address;
            key.Weight = 1;

            Protocol.Permission owner = new Protocol.Permission();
            owner.Type = Protocol.Permission.Types.PermissionType.Owner;
            owner.Id = 0;
            owner.PermissionName = Protocol.Permission.Types.PermissionType.Owner.ToString().ToLower();
            owner.Threshold = 1;
            owner.ParentId = 0;
            owner.Keys.Add(key);

            return owner;
        }

        public static Protocol.Permission CreateDefaultActivePermission(ByteString address, Manager db_manager)
        {
            Protocol.Key key = new Protocol.Key();
            key.Address = address;
            key.Weight = 1;

            Protocol.Permission active = new Protocol.Permission();
            active.Type = Protocol.Permission.Types.PermissionType.Active;
            active.Id = 2;
            active.PermissionName = Protocol.Permission.Types.PermissionType.Active.ToString().ToLower();
            active.Threshold = 1;
            active.ParentId = 0;
            active.Operations = GetActiveDefaultOperations(db_manager);
            active.Keys.Add(key);

            return active;
        }

        public static Protocol.Permission CreateDefaultWitnessPermission(ByteString address)
        {
            Protocol.Key key = new Protocol.Key();
            key.Address = address;
            key.Weight = 1;

            Protocol.Permission witness = new Protocol.Permission();
            witness.Type = Protocol.Permission.Types.PermissionType.Witness;
            witness.Id = 1;
            witness.PermissionName = Protocol.Permission.Types.PermissionType.Witness.ToString().ToLower();
            witness.Threshold = 1;
            witness.ParentId = 0;
            witness.Keys.Add(key);

            return witness;
        }

        public void SetDefaultWitnessPermission(Manager db_manager)
        {
            Protocol.Account account = this.account;

            if (this.account.OwnerPermission == null)
                this.account.OwnerPermission = CreateDefaultOwnerPermission(this.account.Address);

            if (this.account.ActivePermission.Count == 0)
                this.account.ActivePermission.Add(CreateDefaultActivePermission(this.account.Address, db_manager));

            this.account.WitnessPermission = CreateDefaultWitnessPermission(this.account.Address);
        }

        public byte[] GetWitnessPermissionAddress()
        {
            byte[] result = null;

            if (this.account.WitnessPermission.Keys.Count == 0)
                result = this.account.Address.ToByteArray();
            else
                result = this.account.WitnessPermission.Keys[0].Address.ToByteArray();

            return result;
        }

        public void AddVotes(ByteString vote_address, long vote_count)
        {
            this.account.Votes.Add(new Protocol.Vote() { VoteAddress = vote_address, VoteCount = vote_count });
        }

        public List<Protocol.Vote> GetVotesList()
        {
            List<Protocol.Vote> result = new List<Protocol.Vote>();

            if (this.account.Votes != null)
                result new List<Protocol.Vote>(this.account.Votes);

            return result;
        }

        public void ClearVotes()
        {
            this.account.Votes.Clear();
        }

        public void ClearAssetV2()
        {
            this.account.AssetV2.Clear();
        }

        public void ClearLatestAssetOperationTimeV2()
        {
            this.account.LatestAssetOperationTimeV2.Clear();
        }

        public void ClearFreeAssetNetUsageV2()
        {
            this.account.FreeAssetNetUsageV2.Clear();
        }

        public long GetMineralPower()
        {
            long result = 0;
            for (int i = 0; i < account.Frozen.Count; i++)
            {
                result += this.account.Frozen[i].FrozenBalance;
            }

            result += this.account.AccountResource.FrozenBalanceForEnergy.FrozenBalance;
            result += this.account.DelegatedFrozenBalanceForBandwidth;
            result += this.account.AccountResource.DelegatedFrozenBalanceForEnergy;

            return result;
        }

        public bool AssetBalanceEnough(byte[] key, long amount)
        {
            Dictionary<string, long> assets = new Dictionary<string, long>(this.account.Asset);
            string key_name = StringHelper.GetString(key);
            long current_amount = assets.ContainsKey(key_name) ? assets[key_name] : 0;

            return amount > 0 && amount <= current_amount;
        }

        public bool AssetBalanceEnoughV2(byte[] key, long amount, Manager db_manager)
        {
            Dictionary<string, long> assets;
            string key_name;
            long current_amount = 0;

            if (db_manager.DynamicPropertiesStore.GetAllowSameTokenName() == 0)
                assets = new Dictionary<string, long>(this.account.Asset);
            else
                assets = new Dictionary<string, long>(this.account.AssetV2);

            key_name = StringHelper.GetString(key);
            current_amount = assets.ContainsKey(key_name) ? assets[key_name] : 0;

            return amount > 0  && amount <= current_amount;
        }

        public bool ReduceAssetAmount(byte[] key, long amount)
        {
            Dictionary<string, long> assets = new Dictionary<string, long>(this.account.Asset);
            string key_name = StringHelper.GetString(key);
            long current_amount = assets.ContainsKey(key_name) ? assets[key_name] : 0;

            if (amount > 0 && amount <= current_amount)
            {
                this.account.Asset.Add(key_name, current_amount - amount);
                return true;
            }

            return false;
        }

        public bool ReduceAssetAmountV2(byte[] key, long amount, Manager db_manager)
        {
            if (db_manager.DynamicPropertiesStore.GetAllowSameTokenName() == 0)
            {
                Dictionary<string, long> assets = new Dictionary<string, long>(this.account.Asset);
                
            }
            else
            {
            }
        }

        public long Subtract(long value1, long value2)
        {
            if (value1 > 0 && value2 > 0)
                Math.Abs(value1 - value2);
        }

        public int CompareTo(AccountCapsule other)
        {
            return other.Balance.CompareTo(this.Balance);
        }
        #endregion
    }
}
