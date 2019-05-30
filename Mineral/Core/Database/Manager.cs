using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Database
{
    public class Manager
    {
        #region Field
        public BlockStore block_store;
        private DynamicPropertiesStore dynamic_properties_store = new DynamicPropertiesStore("properties");
        private AssetIssueStore asset_issue_store = new AssetIssueStore("asset-issue");
        #endregion


        #region Property
        public BlockStore BlockStore { get { return this.block_store; } }
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
        #endregion
    }
}
