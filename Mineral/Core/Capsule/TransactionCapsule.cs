using System;
using System.Collections.Generic;
using System.Text;
using Protocol;

namespace Mineral.Core.Capsule
{
    public class TransactionCapsule : IProtoCapsule<Protocol.Transaction>
    {
        #region Field
        private Transaction transaction = null;
        private bool is_verifyed = false;
        private long block_num = -1;
        private TransactionTrace tx_trace;
        #endregion


        #region Property
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public byte[] GetData()
        {
            throw new NotImplementedException();
        }

        public Transaction GetInstance()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
