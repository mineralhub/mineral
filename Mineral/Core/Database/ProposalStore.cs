﻿using System;
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
        #endregion


        #region Contructor
        public ProposalStore(string db_name = "proposal") : base(db_name) { }
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

        public List<ProposalCapsule> GetAllProposals()
        {
            List<ProposalCapsule> result = new List<ProposalCapsule>();
            IEnumerator<KeyValuePair<byte[], ProposalCapsule>> it = GetEnumerator();
            while (it.MoveNext())
            {
                result.Add(it.Current.Value);
            }

            return result;
        }
        #endregion
    }
}