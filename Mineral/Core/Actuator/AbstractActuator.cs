using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Mineral.Common.Storage;
using Mineral.Core.Capsule;
using Mineral.Core.Database;

namespace Mineral.Core.Actuator
{
    public abstract class AbstractActuator : IActuator
    {
        #region Field
        protected Any contract = null;
        protected Manager db_manager = null;
        protected IDeposit deposit = null;
        #endregion


        #region Property
        public IDeposit Deposit
        {
            get { return this.deposit; }
            set { this.deposit = value; }
        }
        #endregion


        #region Contructor
        public AbstractActuator(Any contract, Manager db_manager)
        {
            this.contract = contract;
            this.db_manager = db_manager;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public abstract long CalcFee();
        public abstract bool Execute(TransactionResultCapsule result);
        public abstract ByteString GetOwnerAddress();
        public abstract bool Validate();
        #endregion
    }
}
