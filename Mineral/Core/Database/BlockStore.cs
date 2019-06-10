using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mineral.Common.Utils;
using Mineral.Core.Capsule;
using Protocol;
using static Mineral.Core.Capsule.BlockCapsule;

namespace Mineral.Core.Database
{
    public class BlockStore : MineralStoreWithRevoking<BlockCapsule, Block>
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Constructor
        public BlockStore(string db_name = "block") : base (db_name) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public List<BlockCapsule> GetLimitNumber(long start, long limit)
        {
            BlockId id = new BlockId(SHA256Hash.ZERO_HASH, start);
            return this.revoking_db.GetValuesNext(id.Hash, limit)
                    .Select(data =>
                    {
                        try { return new BlockCapsule(data); }
                        catch { }
                        return null;
                    })
                    .ToArray()
                    .Where(block => block != null)
                    .OrderBy(block => block.Num)
                    .ToList();
        }

        public List<BlockCapsule> GetBlockByLatestNum(long num)
        {
            return this.revoking_db.GetLatestValues(num)
                    .Select(data =>
                    {
                        try { return new BlockCapsule(data); }
                        catch { }
                        return null;
                    })
                    .ToArray()
                    .Where(block => block != null)
                    .OrderBy(block => block.Num)
                    .ToList();
        }
        #endregion
    }
}
