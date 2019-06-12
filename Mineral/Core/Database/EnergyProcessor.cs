using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Capsule;

namespace Mineral.Core.Database
{
    public class EnergyProcessor : ResourceProcessor
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Constructor
        public EnergyProcessor(Manager db_manager) : base(db_manager) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override void Consume(TransactionCapsule tx, TransactionTrace tx_trace)
        {
            throw new NotImplementedException();
        }

        public override void UpdateUsage(AccountCapsule account)
        {
            long now = this.db_manager.Witness
        }
        #endregion
    }
}
