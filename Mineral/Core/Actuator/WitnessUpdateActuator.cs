using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Mineral.Core.Capsule;
using Mineral.Core.Capsule.Util;
using Mineral.Core.Database;
using Mineral.Core.Exception;
using Protocol;
using static Protocol.Transaction.Types.Result.Types;

namespace Mineral.Core.Actuator
{
    public class WitnessUpdateActuator : AbstractActuator
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public WitnessUpdateActuator(Any contract, DatabaseManager db_manager) : base(contract, db_manager) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private void UpdateWitness(WitnessUpdateContract contract)
        {
            WitnessCapsule witness = this.db_manager.Witness.Get(contract.OwnerAddress.ToByteArray());
            witness.Url = contract.UpdateUrl.ToStringUtf8();
            this.db_manager.Witness.Put(witness.CreateDatabaseKey(), witness);
        }
        #endregion


        #region External Method
        public override long CalcFee()
        {
            return 0;
        }

        public override bool Execute(TransactionResultCapsule result)
        {
            long fee = CalcFee();

            try
            {
                WitnessUpdateContract witness_update_contract = this.contract.Unpack<WitnessUpdateContract>();
                UpdateWitness(witness_update_contract);
                result.SetStatus(fee, code.Sucess);
            }
            catch (InvalidProtocolBufferException e)
            {
                Logger.Debug(e.Message);
                result.SetStatus(fee, code.Failed);
                throw new ContractExeException(e.Message);
            }

            return true;
        }

        public override ByteString GetOwnerAddress()
        {
            return contract.Unpack<WitnessUpdateContract>().OwnerAddress;
    }

        public override bool Validate()
        {
            if (this.contract == null)
                throw new ContractValidateException("No contract!");

            if (this.db_manager == null)
                throw new ContractValidateException("No dbManager!");

            if (this.contract.Is(WitnessUpdateContract.Descriptor))
            {
                WitnessUpdateContract witness_update_contract = null;
                try
                {
                    witness_update_contract = this.contract.Unpack<WitnessUpdateContract>();
                }
                catch (InvalidProtocolBufferException e)
                {
                    Logger.Debug(e.Message);
                    throw new ContractValidateException(e.Message);
                }

                byte[] owner_address = witness_update_contract.OwnerAddress.ToByteArray();
                if (!Wallet.IsValidAddress(owner_address))
                {
                    throw new ContractValidateException("Invalid address");
                }

                if (!this.db_manager.Account.Contains(owner_address))
                {
                    throw new ContractValidateException("account does not exist");
                }

                if (!TransactionUtil.ValidUrl(witness_update_contract.UpdateUrl.ToByteArray()))
                {
                    throw new ContractValidateException("Invalid url");
                }

                if (!this.db_manager.Witness.Contains(owner_address))
                {
                    throw new ContractValidateException("Witness does not exist");
                }
            }
            else
            {
                throw new ContractValidateException(
                        "contract type error,expected type [WitnessUpdateContract],real type[" + contract.GetType().Name + "]");
            }

            return true;
        }
        #endregion
    }
}
