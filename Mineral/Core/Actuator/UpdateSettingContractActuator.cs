using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Mineral.Core.Capsule;
using Mineral.Core.Database;
using Mineral.Core.Exception;
using Protocol;
using static Protocol.Transaction.Types.Result.Types;

namespace Mineral.Core.Actuator
{
    public class UpdateSettingContractActuator : AbstractActuator
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public UpdateSettingContractActuator(Any contract, DatabaseManager db_manager) : base(contract, db_manager) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
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
                UpdateSettingContract us_contract = contract.Unpack<UpdateSettingContract>();
                long new_percent = us_contract.ConsumeUserResourcePercent;
                byte[] contract_address = us_contract.ContractAddress.ToByteArray();

                ContractCapsule deployed_contract = this.db_manager.Contract.Get(contract_address);

                deployed_contract.Instance.ConsumeUserResourcePercent = new_percent;
                this.db_manager.Contract.Put(contract_address, new ContractCapsule(deployed_contract.Instance));

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
            return contract.Unpack<UpdateSettingContract>().OwnerAddress;
        }

        public override bool Validate()
        {
            if (this.contract == null)
            {
                throw new ContractValidateException("No contract!");
            }
            if (this.db_manager == null)
            {
                throw new ContractValidateException("No db_manager!");
            }
            if (this.contract.Is(UpdateSettingContract.Descriptor))
            {
                UpdateSettingContract contract = null;

                try
                {
                    contract = this.contract.Unpack<UpdateSettingContract>();
                }
                catch (InvalidProtocolBufferException e)
                {
                    Logger.Debug(e.Message);
                    throw new ContractValidateException(e.Message);
                }

                if (!Wallet.IsValidAddress(contract.OwnerAddress.ToByteArray()))
                {
                    throw new ContractValidateException("Invalid address");
                }

                byte[] owner_address = contract.OwnerAddress.ToByteArray();

                AccountCapsule account = this.db_manager.Account.Get(owner_address);
                if (account == null)
                {
                    throw new ContractValidateException(
                        "Account[" + owner_address.ToHexString() + "] not exists");
                }

                long new_percent = contract.ConsumeUserResourcePercent;
                if (new_percent > 100 || new_percent < 0)
                {
                    throw new ContractValidateException("percent not in [0, 100]");
                }

                byte[] contractAddress = contract.ContractAddress.ToByteArray();
                ContractCapsule deployed_contract = this.db_manager.Contract.Get(contractAddress);

                if (deployed_contract == null)
                {
                    throw new ContractValidateException("Contract not exists");
                }

                byte[] contract_owner_address = deployed_contract.Instance.OriginAddress.ToByteArray();
                if (owner_address.SequenceEqual(contract_owner_address))
                {
                    throw new ContractValidateException(
                        "Account[" + owner_address.ToHexString() + "] is not the owner of the contract");
                }
            }
            else
            {
                throw new ContractValidateException(
                    "contract type error,expected type [UpdateSettingContract],real type[" + contract.GetType().Name + "]");
            }

            return true;
        }
        #endregion
    }
}
