using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mineral.Core.Capsule;
using Mineral.Core.Config;
using Protocol;

namespace Mineral.Core.Database
{
    public class AssetIssueStore 
        : MineralStoreWithRevoking<AssetIssueCapsule, AssetIssueContract>
    {
        public class AssetIssueCapsuleCompare : IComparer<AssetIssueCapsule>
        {
            public int Compare(AssetIssueCapsule x, AssetIssueCapsule y)
            {
                if (x.Name != y.Name)
                    return string.Compare(x.Name.ToStringUtf8(), y.Name.ToStringUtf8());

                return x.Order.CompareTo(y.Order);
            }
        }

        #region Field
        #endregion


        #region Property
        public List<AssetIssueCapsule> AllAssetIssues
        {
            get
            {
                List<AssetIssueCapsule> result = new List<AssetIssueCapsule>();
                IEnumerator<KeyValuePair<byte[], AssetIssueCapsule>> it = GetEnumerator();
                while (it.MoveNext())
                {
                    result.Add(it.Current.Value);
                }

                return result;
            }
        }
        #endregion


        #region Constructor
        public AssetIssueStore(IRevokingDatabase revoking_database, string db_name = "asset-issue")
            : base(revoking_database, db_name)
        {
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override AssetIssueCapsule Get(byte[] key)
        {
            return GetUnchecked(key);
        }

        public List<AssetIssueCapsule> GetAssetIssuesPaginated(List<AssetIssueCapsule> asset_issues, long offset, long limit)
        {
            if (limit < 0 || offset < 0)
                return null;

            if (asset_issues.GetSize() <= offset)
                return null;

            asset_issues.OrderBy(x => x, new AssetIssueCapsuleCompare());
            limit = limit > Parameter.DatabaseParameters.ASSET_ISSUE_COUNT_LIMIT_MAX ?
                            Parameter.DatabaseParameters.ASSET_ISSUE_COUNT_LIMIT_MAX : limit;

            long end = offset + limit;
            end = end > asset_issues.GetSize() ? asset_issues.GetSize() : end;

            return asset_issues.GetRange((int)offset, (int)end);
        }
        #endregion
    }
}
