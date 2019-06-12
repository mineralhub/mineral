using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Mineral.Core.Database;
using Mineral.Utils;
using Protocol;

namespace Mineral.Core.Capsule
{
    public class AssetIssueCapsule : IProtoCapsule<AssetIssueContract>
    {
        #region Field
        private AssetIssueContract asset_issue = null;
        #endregion


        #region Property
        public AssetIssueContract Instance { get { return this.asset_issue; } }
        public byte[] Data { get { return this.asset_issue.ToByteArray(); } }

        public ByteString Name
        {
            get { return this.asset_issue.Name; }
        }

        public string Id
        {
            get { return this.asset_issue.Id; }
            set { this.asset_issue.Id = value; }
        }

        public int Percision
        {
            get { return this.asset_issue.Precision; }
            set { this.asset_issue.Precision = value; }
        }

        public long Order
        {
            get { return this.asset_issue.Order; }
            set { this.asset_issue.Order = value; }
        }

        public int Num
        {
            get { return this.asset_issue.Num; }
        }

        public int TransactionNum
        {
            get { return this.asset_issue.TrxNum; }
        }

        public long StartTime
        {
            get { return this.asset_issue.StartTime; }
        }

        public long EndTime
        {
            get { return this.asset_issue.EndTime; }
        }

        public ByteString OwnerAddress
        {
            get { return this.asset_issue.OwnerAddress; }
        }

        public int FrozenSupplyCount
        {
            get { return this.asset_issue.FrozenSupply.Count; }
        }

        public List<AssetIssueContract.Types.FrozenSupply> FrozenSupplyList
        {
            get { return new List<AssetIssueContract.Types.FrozenSupply>(this.asset_issue.FrozenSupply); }
        }

        public long FrozenSupply
        {
            get
            {
                long frozenBalance = 0;
                foreach (var frozen in this.asset_issue.FrozenSupply)
                {
                    frozenBalance += frozen.FrozenAmount;
                }

                return frozenBalance;
            }
        }

        public long FreeAssetNetLimit
        {
            get { return this.asset_issue.FreeAssetNetLimit; }
            set { this.asset_issue.FreeAssetNetLimit = value; }
        }

        public long PublicFreeAssetNetLimit
        {
            get { return this.asset_issue.PublicFreeAssetNetLimit; }
            set { this.asset_issue.PublicFreeAssetNetLimit = value; }
        }

        public long PublicFreeAssetNetUsage
        {
            get { return this.asset_issue.PublicFreeAssetNetUsage; }
            set { this.asset_issue.PublicFreeAssetNetUsage = value; }
        }

        public long PublicLatestFreeNetTime
        {
            get { return this.asset_issue.PublicLatestFreeNetTime; }
            set { this.asset_issue.PublicLatestFreeNetTime = value; }
        }
        #endregion


        #region Constructor
        public AssetIssueCapsule(byte[] data)
        {
            try
            {
                this.asset_issue = AssetIssueContract.Parser.ParseFrom(data);
            }
            catch (System.Exception e)
            {
                Logger.Error(e.Message);
            }
        }

        public AssetIssueCapsule(AssetIssueContract asset_issue)
        {
            this.asset_issue = asset_issue;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public byte[] CreateDatabaseKey()
        {
            return this.asset_issue.Name.ToByteArray();
        }

        public byte[] CreateDatabaseKeyV2()
        {
            return StringHelper.GetBytes(this.asset_issue.Id);
        }

        public byte[] CreateDatabaseKeyFinal(Manager db_manager)
        {
            if (db_manager.DynamicProperties.GetAllowSameTokenName() == 0)
                return CreateDatabaseKey();
            else
                return CreateDatabaseKeyV2();
        }

        public static string CreateDatabaseKeyString(string name, long order)
        {
            return name + "_" + order;
        }

        public void SetUrl(ByteString url)
        {
            this.asset_issue.Url = url;
        }

        public void SetDescription(ByteString description)
        {
            this.asset_issue.Description = description;
        }

        public override string ToString()
        {
            return this.asset_issue.ToString();
        }
        #endregion
    }
}
