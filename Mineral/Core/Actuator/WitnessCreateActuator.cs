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
    public class WitnessCreateActuator : AbstractActuator
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public WitnessCreateActuator(Any contract, DatabaseManager db_manager) : base(contract, db_manager) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private void CreateWitness(WitnessCreateContract contract)
        {
            WitnessCapsule witness = new WitnessCapsule(
                                                    contract.OwnerAddress,
                                                    0,
                                                    contract.Url.ToStringUtf8());

            Logger.Debug(
                string.Format(
                    "CreateWitness, address[{0}]",
                    witness.ToHexString()));

            this.db_manager.Witness.Put(witness.CreateDatabaseKey(), witness);
            AccountCapsule account = this.db_manager.Account.Get(witness.CreateDatabaseKey());
            account.IsWitness = true;

            if (this.db_manager.DynamicProperties.GetAllowMultiSign() == 1)
            {
                account.SetDefaultWitnessPermission(this.db_manager);
            }
            this.db_manager.Account.Put(account.CreateDatabaseKey(), account);

            long cost = this.db_manager.DynamicProperties.GetAccountUpgradeCost();
            this.db_manager.AdjustBalance(contract.OwnerAddress.ToByteArray(), -cost);
            this.db_manager.AdjustBalance(this.db_manager.Account.GetBlackHole().CreateDatabaseKey(), +cost);
            this.db_manager.DynamicProperties.AddTotalCreateWitnessCost(cost);
        }
        #endregion


        #region External Method
        public override long CalcFee()
        {
            return this.db_manager.DynamicProperties.GetAccountUpgradeCost();
        }

        public override bool Execute(TransactionResultCapsule result)
        {
            long fee = CalcFee();

            try
            {
                WitnessCreateContract contract = this.contract.Unpack<WitnessCreateContract>();
                CreateWitness(contract);
                result.SetStatus(fee, code.Sucess);
            }
            catch (InvalidProtocolBufferException e)
            {
                Logger.Debug(e.Message);
                result.SetStatus(fee, code.Failed);
                throw new ContractExeException(e.Message);
            }
            catch (BalanceInsufficientException e)
            {
                Logger.Debug(e.Message);
                result.SetStatus(fee, code.Failed);
                throw new ContractExeException(e.Message);
            }
            return true;
        }

        public override ByteString GetOwnerAddress()
        {
            return contract.Unpack<WitnessCreateContract>().OwnerAddress;
        }

        public override bool Validate()
        {
            if (this.contract == null)
            {
                throw new ContractValidateException("No contract!");
            }
            if (this.db_manager == null)
            {
                throw new ContractValidateException("No dbManager!");
            }

            if (this.contract.Is(WitnessCreateContract.Descriptor))
            {
                WitnessCreateContract witness_create_contract = null;
                try
                {
                    witness_create_contract = this.contract.Unpack<WitnessCreateContract>();
                }
                catch (InvalidProtocolBufferException e)
                {
                    throw new ContractValidateException(e.Message);
                }

                byte[] owner_address = witness_create_contract.OwnerAddress.ToByteArray();
                string owner_address_str = owner_address.ToHexString();

                if (!Wallet.IsValidAddress(owner_address))
                {
                    throw new ContractValidateException("Invalid address");
                }

                if (!TransactionUtil.ValidUrl(witness_create_contract.Url.ToByteArray()))
                {
                    throw new ContractValidateException("Invalid url");
                }

                AccountCapsule account = this.db_manager.Account.Get(owner_address);

                if (account == null)
                {
                    throw new ContractValidateException("account[" + owner_address_str + "] not exists");
                }


                if (this.db_manager.Witness.Contains(owner_address))
                {
                    throw new ContractValidateException("Witness[" + owner_address_str + "] has existed");
                }

                if (account.Balance < this.db_manager.DynamicProperties.GetAccountUpgradeCost())
                {
                    throw new ContractValidateException("balance < AccountUpgradeCost");
                }
            }
            else
            {
                throw new ContractValidateException(
                    "contract type error,expected type [WitnessCreateContract],real type[" + this.contract.GetType().Name + "]");
            }

            return true;
        }
        #endregion
    }
}
