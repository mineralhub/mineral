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
    public class ClearABIContractActuator : AbstractActuator
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public ClearABIContractActuator(Any contract, DatabaseManager db_manager) : base(contract, db_manager) { }
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
                ClearABIContract us_contract = contract.Unpack<ClearABIContract>();

                byte[] contract_address = us_contract.ContractAddress.ToByteArray();
                ContractCapsule deployed_contract = this.db_manager.Contract.Get(contract_address);

                deployed_contract.ClearABI();
                this.db_manager.Contract.Put(contract_address, deployed_contract);

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
            return contract.Unpack<ClearABIContract>().OwnerAddress;
        }

        public override bool Validate()
        {
            if (!Common.Runtime.Config.VMConfig.AllowTvmConstantinople)
            {
                throw new ContractValidateException(
                    "contract type error,unexpected type [ClearABIContract]");
            }

            if (this.contract == null)
                throw new ContractValidateException("No contract!");

            if (this.db_manager == null)
                throw new ContractValidateException("No db_manager!");

            if (this.contract.Is(ClearABIContract.Descriptor))
            {
                ClearABIContract contract = null;
                try
                {
                    contract = this.contract.Unpack<ClearABIContract>();
                }
                catch (InvalidProtocolBufferException e)
                {
                    Logger.Debug(e.Message);
                    throw new ContractValidateException(e.Message);
                }
                if (!Wallet.AddressValid(contract.OwnerAddress.ToByteArray()))
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

                byte[] contract_address = contract.ContractAddress.ToByteArray();
                ContractCapsule deployed_contract = this.db_manager.Contract.Get(contract_address);

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
                    "contract type error,expected type [ClearABIContract],real type[" + this.contract.GetType().Name + "]");
            }

            return true;
        }
        #endregion
    }
}
