using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Mineral.Core.Database;
using Mineral.Utils;
using Protocol;

namespace Mineral.Core.Capsule
{
    public class AccountCapsule : IProtoCapsule<Account>, IComparable<AccountCapsule>
    {
        #region Field
        private Account account = null;
        #endregion


        #region Property
        public Account Instance { get { return this.account; } }
        public byte[] Data { get { return this.account.ToByteArray(); } }

        public AccountType Type
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
            set { this.account.AccountName = value; }
        }

        public ByteString AccountId
        {
            get { return this.account.AccountId; }
            set { this.account.AccountId = value; }
        }

        public Account.Types.AccountResource AccountResource
        {
            get { return this.account.AccountResource; }
            set { this.account.AccountResource = value; }
        }

        public long Balance
        {
            get { return this.account.Balance; }
            set { this.account.Balance = value; }
        }

        public long FrozenBalance
        {
            get
            {
                List<Account.Types.Frozen> frozen_list = new List<Account.Types.Frozen>(this.account.Frozen);
                long frozen_balance = 0;
                frozen_list.ForEach(frozen => frozen_balance += frozen.FrozenBalance);

                return frozen_balance;
            }
        }

        public long EnergyFrozenBalance
        {
            get { return this.account.AccountResource.FrozenBalanceForEnergy.FrozenBalance; }
        }

        public long AllEnergyFrozenBalance
        {
            get { return EnergyFrozenBalance + AcquiredDelegatedFrozenBalanceForEnergy; }
        }

        public int FrozenCount
        {
            get { return this.account.Frozen.Count; }
        }

        public List<Account.Types.Frozen> FrozenList
        {
            get { return new List<Account.Types.Frozen>(this.account.Frozen); }
        }

        public long Allowance
        {
            get { return this.account.Allowance; }
            set { this.account.Allowance = value; }
        }

        public long NetUsage
        {
            get { return this.account.NetUsage; }
            set { this.account.NetUsage = value; }
        }

        public long FreeNetUsage
        {
            get { return this.account.FreeNetUsage; }
            set { this.account.FreeNetUsage = value; }
        }

        public long EnergyUsage
        {
            get { return this.account.AccountResource.EnergyUsage; }
            set { this.account.AccountResource.EnergyUsage = value; }
        }

        public long StorageUsage
        {
            get { return this.account.AccountResource.StorageUsage; }
            set { this.account.AccountResource.StorageUsage = value; }
        }

        public bool IsWitness
        {
            get { return this.account.IsWitness; }
            set { this.account.IsWitness = value; }
        }

        public bool IsCommittee
        {
            get { return this.account.IsCommittee; }
            set { this.account.IsCommittee = value; }
        }

        public long LatestWithdrawTime
        {
            get { return this.account.LatestWithdrawTime; }
            set { this.account.LatestWithdrawTime = value; }
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

        public long LatestConsumeTimeForEnergy
        {
            get { return this.account.AccountResource.LatestConsumeTimeForEnergy; }
            set { this.account.AccountResource.LatestConsumeTimeForEnergy = value; }
        }

        public long LatestConsumeFreeTime
        {
            get { return this.account.LatestConsumeFreeTime; }
            set { this.account.LatestConsumeFreeTime = value; }
        }

        public long LatestExchangeStorageTime
        {
            get { return this.account.AccountResource.LatestExchangeStorageTime; }
            set { this.account.AccountResource.LatestExchangeStorageTime = value; }
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

        public long AllFrozenBalanceForEnergy
        {
            get { return EnergyFrozenBalance + AcquiredDelegatedFrozenBalanceForEnergy; }
        }

        public long AllFrozenBalanceForBandwidth
        {
            get { return FrozenBalance + AcquiredDelegatedFrozenBalanceForBandwidth; }
        }

        public List<Account.Types.Frozen> FrozenSupplyList
        {
            get { return new List<Account.Types.Frozen>(this.account.FrozenSupply); }
        }

        public int FrozenSupplyCount
        {
            get { return this.account.FrozenSupply.Count; }
        }

        public long FrozenSupplyBalance
        {
            get
            {
                long frozen_supply_balance = 0;
                FrozenSupplyList.ForEach(frozen => frozen_supply_balance += frozen.FrozenBalance);
                return frozen_supply_balance;
            }
        }

        public long StorageLimit
        {
            get { return this.account.AccountResource.StorageLimit; }
            set { this.account.AccountResource.StorageLimit = value; }
        }

        public ByteString AssetIssuedName
        {
            get { return this.account.AssetIssuedName; }
            set { this.account.AssetIssuedName = value; }
        }

        public ByteString AssetIssuedID
        {
            get { return this.account.AssetIssuedID; }
            set { this.account.AssetIssuedID = value; }
        }

        public Dictionary<string, long> Asset
        {
            get { return new Dictionary<string, long>(this.account.Asset); }
        }

        public Dictionary<string, long> AssetV2
        {
            get { return new Dictionary<string, long>(this.account.AssetV2); }
        }

        public Dictionary<string, long> LatestAssetOperationTime
        {
            get { return new Dictionary<string, long>(this.account.LatestAssetOperationTime); }
        }

        public Dictionary<string, long> LatestAssetOperationTimeV2
        {
            get { return new Dictionary<string, long>(this.account.LatestAssetOperationTimeV2); }
        }

        public Dictionary<string, long> FreeAssetNetUsage
        {
            get { return new Dictionary<string, long>(this.account.FreeAssetNetUsage); }
        }

        public Dictionary<string, long> FreeAssetNetUsageV2
        {
            get { return new Dictionary<string, long>(this.account.FreeAssetNetUsageV2); }
        }
        #endregion


        #region Constructor
        public AccountCapsule(Account account)
        {
            this.account = account;
        }

        public AccountCapsule(byte[] data)
        {
            try
            {
                this.account = Account.Parser.ParseFrom(data);
            }
            catch (System.Exception e)
            {
                Logger.Debug(e.Message);
            }
        }

        public AccountCapsule(ByteString address, AccountType account_type)
        {
            this.account = new Account();
            this.account.Type = account_type;
            this.account.Address = address;
        }

        public AccountCapsule(ByteString address, ByteString account_name, AccountType account_type)
        {
            this.account = new Account();
            this.account.Type = account_type;
            this.account.AccountName = account_name;
            this.account.Address = address;
        }

        public AccountCapsule(ByteString account_name, ByteString address, AccountType account_type, long balance)
        {
            this.account = new Account();
            this.account.AccountName = account_name;
            this.account.Type = account_type;
            this.account.Address = address;
            this.account.Balance = balance;
        }

        public AccountCapsule(ByteString address, AccountType account_type, long create_time, bool default_permission, DatabaseManager db_manager)
        {
            if (default_permission)
            {
                Permission owner = CreateDefaultOwnerPermission(this.account.Address);
                Permission active = CreateDefaultActivePermission(this.account.Address, db_manager);

                this.account = new Account();
                this.account.Type = account_type;
                this.account.Address = address;
                this.account.CreateTime = create_time;
                this.account.OwnerPermission = owner;
                this.account.ActivePermission.Add(active);
            }
            else
            {
                this.account = new Account();
                this.account.Type = account_type;
                this.account.Address = address;
                this.account.CreateTime = create_time;
            }
        }

        public AccountCapsule(AccountCreateContract contract, long create_time, bool default_permission, DatabaseManager db_manager)
        {
            if (default_permission)
            {
                Permission owner = CreateDefaultOwnerPermission(this.account.Address);
                Permission active = CreateDefaultActivePermission(this.account.Address, db_manager);

                this.account = new Account();
                this.account.Type = contract.Type;
                this.account.Address = contract.AccountAddress;
                this.account.CreateTime = create_time;
                this.account.OwnerPermission = owner;
                this.account.ActivePermission.Add(active);
            }
            else
            {
                this.account = new Account();
                this.account.Type = contract.Type;
                this.account.Address = contract.AccountAddress;
                this.account.CreateTime = create_time;
            }
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private static ByteString GetActiveDefaultOperations(DatabaseManager db_manager)
        {
            return ByteString.CopyFrom(db_manager.DynamicProperties.GetActiveDefaultOperations());
        }
        #endregion


        #region External Method
        public void SetInstance(Account account)
        {
            this.account = account;
        }

        public byte[] CreateDatabaseKey()
        {
            return this.account.Address.ToByteArray();
        }

        public string ToHexString()
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

        public void PutLatestAssetOperationTimeV2(Dictionary<string, long> assets)
        {
            this.account.LatestAssetOperationTimeV2.Add(assets);
        }

        public void PutAllFreeAssetNetUsageV2(Dictionary<string, long> assets)
        {
            this.account.FreeAssetNetUsageV2.Add(assets);
        }

        public static Permission CreateDefaultOwnerPermission(ByteString address)
        {
            Key key = new Key();
            key.Address = address;
            key.Weight = 1;

            Permission owner = new Permission();
            owner.Type = Permission.Types.PermissionType.Owner;
            owner.Id = 0;
            owner.PermissionName = Permission.Types.PermissionType.Owner.ToString().ToLower();
            owner.Threshold = 1;
            owner.ParentId = 0;
            owner.Keys.Add(key);

            return owner;
        }

        public static Permission CreateDefaultActivePermission(ByteString address, DatabaseManager db_manager)
        {
            Key key = new Key();
            key.Address = address;
            key.Weight = 1;

            Permission active = new Permission();
            active.Type = Permission.Types.PermissionType.Active;
            active.Id = 2;
            active.PermissionName = Permission.Types.PermissionType.Active.ToString().ToLower();
            active.Threshold = 1;
            active.ParentId = 0;
            active.Operations = GetActiveDefaultOperations(db_manager);
            active.Keys.Add(key);

            return active;
        }

        public static Permission CreateDefaultWitnessPermission(ByteString address)
        {
            Key key = new Key();
            key.Address = address;
            key.Weight = 1;

            Permission witness = new Permission();
            witness.Type = Permission.Types.PermissionType.Witness;
            witness.Id = 1;
            witness.PermissionName = Permission.Types.PermissionType.Witness.ToString().ToLower();
            witness.Threshold = 1;
            witness.ParentId = 0;
            witness.Keys.Add(key);

            return witness;
        }

        public void SetDefaultWitnessPermission(DatabaseManager db_manager)
        {
            Account account = this.account;

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
            this.account.Votes.Add(new Vote() { VoteAddress = vote_address, VoteCount = vote_count });
        }

        public List<Vote> GetVotesList()
        {
            List<Vote> result = new List<Vote>();

            if (this.account.Votes != null)
                result = new List<Vote>(this.account.Votes);

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

        public bool AssetBalanceEnoughV2(byte[] key, long amount, DatabaseManager db_manager)
        {
            Dictionary<string, long> assets;
            string key_name;
            long current_amount = 0;

            if (db_manager.DynamicProperties.GetAllowSameTokenName() == 0)
                assets = new Dictionary<string, long>(this.account.Asset);
            else
                assets = new Dictionary<string, long>(this.account.AssetV2);

            key_name = StringHelper.GetString(key);
            current_amount = assets.ContainsKey(key_name) ? assets[key_name] : 0;

            return amount > 0 && amount <= current_amount;
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

        public bool ReduceAssetAmountV2(byte[] key, long amount, DatabaseManager db_manager)
        {
            if (db_manager.DynamicProperties.GetAllowSameTokenName() == 0)
            {
                Dictionary<string, long> assets = new Dictionary<string, long>(this.account.Asset);
                AssetIssueCapsule asset_issue = db_manager.AssetIssue.Get(key);
                string token_id = asset_issue.Id;
                string key_name = StringHelper.GetString(key);
                long current_amount = assets.ContainsKey(key_name) ? assets[key_name] : 0;

                if (amount > 0 && amount <= current_amount)
                {
                    this.account.Asset.Add(key_name, current_amount - amount);
                    this.account.AssetV2.Add(token_id, current_amount - amount);
                    return true;
                }
            }

            if (db_manager.DynamicProperties.GetAllowSameTokenName() == 1)
            {
                Dictionary<string, long> assets = new Dictionary<string, long>(this.account.AssetV2);
                string key_name = StringHelper.GetString(key);
                long current_amount = assets.ContainsKey(key_name) ? assets[key_name] : 0;

                if (amount > 0 && amount <= current_amount)
                {
                    this.account.AssetV2.Add(key_name, current_amount - amount);
                    return true;
                }
            }

            return false;
        }

        public bool AddAssetAmount(byte[] key, long amount)
        {
            Dictionary<string, long> assets = new Dictionary<string, long>(this.account.Asset);
            string key_name = StringHelper.GetString(key);
            long current_amount = assets.ContainsKey(key_name) ? assets[key_name] : 0;

            this.account.Asset.Add(key_name, current_amount - amount);

            return true;
        }

        public bool AddAssetAmountV2(byte[] key, long amount, DatabaseManager db_manager)
        {
            if (db_manager.DynamicProperties.GetAllowSameTokenName() == 0)
            {
                Dictionary<string, long> assets = new Dictionary<string, long>(this.account.Asset);
                AssetIssueCapsule asset_issue = db_manager.AssetIssue.Get(key);
                string token_id = asset_issue.Id;
                string key_name = StringHelper.GetString(key);
                long current_amount = assets.ContainsKey(key_name) ? assets[key_name] : 0;

                this.account.Asset.Add(key_name, current_amount - amount);
                this.account.AssetV2.Add(token_id, current_amount - amount);
            }

            if (db_manager.DynamicProperties.GetAllowSameTokenName() == 1)
            {
                Dictionary<string, long> assets = new Dictionary<string, long>(this.account.AssetV2);
                string key_name = StringHelper.GetString(key);
                long current_amount = assets.ContainsKey(key_name) ? assets[key_name] : 0;

                this.account.AssetV2.Add(key_name, current_amount - amount);
            }

            return true;
        }

        public bool AddAsset(byte[] key, long value)
        {
            Dictionary<string, long> assets = new Dictionary<string, long>(this.account.Asset);
            string key_name = StringHelper.GetString(key);

            if (assets.IsNotNullOrEmpty() && assets.ContainsKey(key_name))
                return false;

            this.account.Asset.Add(key_name, value);

            return true;
        }

        public bool AddAssetV2(byte[] key, long value)
        {
            Dictionary<string, long> assets = new Dictionary<string, long>(this.account.AssetV2);
            string key_name = StringHelper.GetString(key);

            if (assets.IsNotNullOrEmpty() && assets.ContainsKey(key_name))
                return false;

            this.account.AssetV2.Add(key_name, value);

            return true;
        }

        public bool AddAssetV2(string key, long value)
        {
            Dictionary<string, long> assets = new Dictionary<string, long>(this.account.AssetV2);

            if (assets.IsNotNullOrEmpty() && assets.ContainsKey(key))
                return false;

            this.account.AssetV2.Add(key, value);

            return true;
        }

        public bool AddAssetV2(Dictionary<string, long> assets)
        {
            this.account.AssetV2.Add(assets);

            return true;
        }

        public long GetLatestAssetOperationTime(string asset_name)
        {
            return this.LatestAssetOperationTime.ContainsKey(asset_name) ? this.LatestAssetOperationTime[asset_name] : 0;
        }

        public void PutLatestAssetOperationTime(string asset_name, long value)
        {
            this.Instance.LatestAssetOperationTime.Add(asset_name, value);
        }

        public long GetLatestAssetOperationTimeV2(string asset_name)
        {
            return this.LatestAssetOperationTimeV2.ContainsKey(asset_name) ? this.LatestAssetOperationTimeV2[asset_name] : 0;
        }

        public void PutLatestAssetOperationTimeV2(string asset_name, long value)
        {
            this.Instance.LatestAssetOperationTimeV2.Add(asset_name, value);
        }

        public void SetFrozen(long frozen_balance, long expire_time)
        {
            Account.Types.Frozen frozen = new Account.Types.Frozen();
            frozen.FrozenBalance = frozen_balance;
            frozen.ExpireTime = expire_time;

            this.account.Frozen.Add(frozen);
        }

        public void SetFrozenForEnergy(long frozen_balance, long expire_time)
        {
            Account.Types.Frozen frozen = new Account.Types.Frozen();
            frozen.FrozenBalance = frozen_balance;
            frozen.ExpireTime = expire_time;

            this.account.AccountResource.FrozenBalanceForEnergy = frozen;
        }

        public void SetFrozenForBandwidth(long frozen_balance, long expire_time)
        {
            Account.Types.Frozen frozen = new Account.Types.Frozen();
            frozen.FrozenBalance = frozen_balance;
            frozen.ExpireTime = expire_time;

            if (FrozenCount == 0)
                this.account.Frozen.Add(frozen);
            else
                this.account.Frozen[0] = frozen;
        }

        public long GetFreeAssetNetUsange(string asset_name)
        {
            return this.account.FreeAssetNetUsage.ContainsKey(asset_name) ? this.account.FreeAssetNetUsage[asset_name] : 0;
        }

        public long GetFreeAssetNetUsangeV2(string asset_name)
        {
            return this.account.FreeAssetNetUsageV2.ContainsKey(asset_name) ? this.account.FreeAssetNetUsageV2[asset_name] : 0;
        }

        public void PutFreeAssetNetUsage(string asset_name, long free_asset)
        {
            this.account.FreeAssetNetUsage.Add(asset_name, free_asset);
        }

        public void PutFreeAssetNetUsageV2(string asset_name, long free_asset)
        {
            this.account.FreeAssetNetUsageV2.Add(asset_name, free_asset);
        }

        public void AddStorageUsage(long storage_usage)
        {
            if (storage_usage <= 0)
                return;

            this.account.AccountResource.StorageUsage += storage_usage;
        }

        public static Permission GetDefaultPermission(ByteString owner)
        {
            return CreateDefaultOwnerPermission(owner);
        }

        public Permission GetPermissionById(int id)
        {
            if (id == 0)
            {
                if (this.account.OwnerPermission != null)
                {
                    return this.account.OwnerPermission;
                }
                return GetDefaultPermission(this.account.Address);
            }

            if (id == 1)
            {
                if (this.account.WitnessPermission != null)
                {
                    return this.account.WitnessPermission;
                }
                return null;
            }

            foreach (Permission permission in this.account.ActivePermission)
            {
                if (id == permission.Id)
                    return permission;
            }

            return null;
        }

        public void UpdatePermissions(Permission owner, Permission witness, List<Permission> actives)
        {
            owner.Id = 0;
            this.account.OwnerPermission = owner;

            if (this.account.IsWitness)
            {
                witness.Id = 1;
                this.account.WitnessPermission = witness;
            }

            this.account.ActivePermission.Clear();
            for (int i = 0; i < actives.Count; i++)
            {
                actives[i].Id = i + 2;
                this.account.ActivePermission.Add(actives[i]);
            }
        }

        public int CompareTo(AccountCapsule other)
        {
            return other.Balance.CompareTo(this.Balance);
        }
        #endregion
    }
}
