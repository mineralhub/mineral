using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Capsule;
using Mineral.Utils;
using Protocol;

namespace Mineral.Core.Database
{
    public class VotesStore : MineralStoreWithRevoking<VotesCapsule, Votes>
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public VotesStore(IRevokingDatabase revoking_database, string db_name = "votes")
            : base(revoking_database, db_name)
        {
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override VotesCapsule Get(byte[] key)
        {
            byte[] value = this.revoking_db.GetUnchecked(key);
            return value.IsNotNullOrEmpty() ? new VotesCapsule(value) : null;
        }
        #endregion
    }
}
