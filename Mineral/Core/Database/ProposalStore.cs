using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Capsule;
using Protocol;

namespace Mineral.Core.Database
{
    public class ProposalStore : MineralStoreWithRevoking<ProposalCapsule, Proposal>
    {
        #region Field
        #endregion


        #region Property
        public List<ProposalCapsule> AllProposals
        {
            get
            {
                List<ProposalCapsule> result = new List<ProposalCapsule>();
                IEnumerator<KeyValuePair<byte[], ProposalCapsule>> it = GetEnumerator();
                while (it.MoveNext())
                {
                    result.Add(it.Current.Value);
                }

                return result;
            }
        }
        #endregion


        #region Contructor
        public ProposalStore(IRevokingDatabase revoking_database, string db_name = "proposal")
            : base(revoking_database, db_name)
        {
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override ProposalCapsule Get(byte[] key)
        {
            return new ProposalCapsule(this.revoking_db.Get(key));
        }
        #endregion
    }
}
