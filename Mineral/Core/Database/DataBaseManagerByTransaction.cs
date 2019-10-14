using Mineral.Common.Utils;
using Mineral.Core.Capsule;
using Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Database
{
    public partial class DatabaseManager
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public Transaction GetTransactionById(SHA256Hash hash)
        {
            Transaction transaction = null;
            try
            {
                TransactionCapsule result = this.transaction_store.Get(hash.Hash);
                if (result != null)
                {
                    transaction = result.Instance;
                }
            }
            catch
            {
            }

            return transaction;
        }
        #endregion
    }
}
