using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Capsule;
using Mineral.Core.Config.Arguments;
using Protocol;

namespace Mineral.Core.Database
{
    public class TransactionHistoryStore : MineralStoreWithRevoking<TransactionInfoCapsule, TransactionInfo>
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public TransactionHistoryStore(string db_name = "transaction_history_store") : base(db_name) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override TransactionInfoCapsule Get(byte[] key)
        {
            byte[] value = this.revoking_db.GetUnchecked(key);
            return (value != null && value.Length > 0) ? new TransactionInfoCapsule(value) : null;
        }

        public override void Put(byte[] key, TransactionInfoCapsule item)
        {
            if (bool.TryParse(Args.Instance.Storage.TransactionHistorySwitch, out bool result))
            {
                if (result)
                {
                    base.Put(key, item);
                }
            }
        }
        #endregion
    }
}
