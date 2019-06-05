using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Utils;
using Mineral.Core.Database;
using Protocol;

namespace Mineral.Core.Capsule
{
    public class ReceiptCapsule
    {
        #region Field
        private ResourceReceipt receipt = null;
        private long multi_sign_fee = 0;
        private SHA256Hash hash;
        #endregion


        #region Property
        public ResourceReceipt Receipt { get { return this.receipt; } }
        public long MultiSignFee { get { return this.multi_sign_fee; } set { this.multi_sign_fee = value; } }
        public SHA256Hash Hash { get { return this.hash; } }
        #endregion


        #region Constructor
        public ReceiptCapsule(ResourceReceipt receipt, SHA256Hash hash)
        {
            this.receipt = receipt;
            this.hash = hash;
        }

        public ReceiptCapsule(SHA256Hash hash)
        {
            this.receipt = new ResourceReceipt();
            this.hash = hash;
        }

        public ReceiptCapsule(ResourceReceipt receipt)
        {
            this.receipt = receipt;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void AddNetFee(long fee)
        {
            this.receipt.NetFee += fee;
        }

        public void PayEnergyBill(Manager db_manager,
                                AccountCapsule origin, AccountCapsule caller,
                                long percent, long origin_energy_limit,
                                EnergyProcess energe_precess,
                                long now)
        {

        }
        #endregion
    }
}
