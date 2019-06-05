using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Capsule;
using Protocol;

namespace Mineral.Common.Runtime.VM
{
    public class InternalTransaction
    {
        public enum TransactionType
        {
            TX_PRECOMPILED_TYPE,
            TX_CONTRACT_CREATION_TYPE,
            TX_CONTRACT_CALL_TYPE,
            TX_UNKNOWN_TYPE,
        }

        public enum ExecutorType
        {
            ET_PRE_TYPE,
            ET_NORMAL_TYPE,
            ET_CONSTANT_TYPE,
            ET_UNKNOWN_TYPE
        }

        #region Field
        private Transaction transaction = null;
        private long value = 0;
        private byte[] hash = null;
        private byte[] parent_hash = null;

        private long nonce = 0;
        private byte[] data = null;
        private byte[] transfer_to_address = null;

        private int deep;
        private int index;
        private string node;
        private bool is_rejected;
        private byte[] send_address = null;
        private byte[] proto_encoded;

        private byte[] receive_address = null;
        private Dictionary<string, long> token_info = new Dictionary<string, long>();
        #endregion


        #region Property
        public int Deep { get { return this.deep; } }
        public int Index { get { return this.index; } }
        #endregion


        #region Constructor
        public InternalTransaction(Transaction tx, InternalTransaction.TransactionType tx_type)
        {
            this.transaction = tx;
            TransactionCapsule tx_capsule =new TransactionCapsule()
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        #endregion
    }
}
