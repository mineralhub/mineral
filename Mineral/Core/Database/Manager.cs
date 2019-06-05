using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Utils;
using Mineral.Core.Capsule;

namespace Mineral.Core.Database
{
    public class Manager
    {
        #region Field
        public BlockStore block_store;
        public AccountStore account_store;
        private DynamicPropertiesStore dynamic_properties_store = new DynamicPropertiesStore("properties");
        private AssetIssueStore asset_issue_store = new AssetIssueStore("asset-issue");
        #endregion


        #region Property
        public BlockStore Block { get { return this.block_store; } }
        public DynamicPropertiesStore DynamicProperties { get { return this.dynamic_properties_store; } }
        public AssetIssueStore AssetIssue { get { return this.asset_issue_store; } }
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public long GetHeadBlockTimestamp()
        {
            return this.dynamic_properties_store.GetLatestBlockHeaderTimestamp();
        }

        public BlockCapsule GetBlockById(SHA256Hash hash)
        {
            BlockCapsule block = 
        }
        #endregion
    }
}
