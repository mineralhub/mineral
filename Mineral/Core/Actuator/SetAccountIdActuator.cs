using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Mineral.Core.Capsule;
using Mineral.Core.Capsule.Util;
using Mineral.Core.Config;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Database;
using Mineral.Core.Exception;
using Protocol;
using static Protocol.Transaction.Types.Result.Types;

namespace Mineral.Core.Actuator
{
    public class SetAccountIdActuator : AbstractActuator
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public SetAccountIdActuator(Any contract, DataBaseManager db_manager) : base(contract, db_manager) { }
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
            SetAccountIdContract account_id_contract = null;

            try
            {
                account_id_contract = contract.Unpack<SetAccountIdContract>();
            }
            catch (InvalidProtocolBufferException e)
            {
                Logger.Debug(e.Message);
                result.SetStatus(fee, code.Failed);
                throw new ContractExeException(e.Message);
            }

            byte[] owner_address = account_id_contract.OwnerAddress.ToByteArray();
            AccountCapsule account = this.db_manager.Account.Get(owner_address);

            account.AccountId = ByteString.CopyFrom(account_id_contract.AccountId.ToByteArray());
            this.db_manager.Account.Put(owner_address, account);
            this.db_manager.AccountIdIndex.Put(account);
            result.SetStatus(fee, code.Sucess);

            return true;
        }

        public override ByteString GetOwnerAddress()
        {
            return contract.Unpack<SetAccountIdContract>().OwnerAddress;
        }

        public override bool Validate()
        {
            if (this.contract == null)
                throw new ContractValidateException("No contract!");

            if (this.db_manager == null)
                throw new ContractValidateException("No this.db_manager!");

            if (this.contract.Is(SetAccountIdContract.Descriptor))
            {
                SetAccountIdContract account_id_contract = null;

                try
                {
                    account_id_contract = contract.Unpack<SetAccountIdContract>();
                }
                catch (InvalidProtocolBufferException e)
                {
                    Logger.Debug(e.Message);
                    throw new ContractValidateException(e.Message);
                }

                byte[] owner_address = account_id_contract.OwnerAddress.ToByteArray();
                byte[] account_id = account_id_contract.AccountId.ToByteArray();
                if (!TransactionUtil.ValidAccountId(account_id))
                    throw new ContractValidateException("Invalid accountId");

                if (!Wallet.AddressValid(owner_address))
                    throw new ContractValidateException("Invalid owner_address");

                AccountCapsule account = this.db_manager.Account.Get(owner_address);
                if (account == null)
                    throw new ContractValidateException("Account has not existed");

                if (account.AccountId != null && !account.AccountId.IsEmpty)
                    throw new ContractValidateException("This account id already set");

                if (this.db_manager.AccountIdIndex.Contains(account_id))
                    throw new ContractValidateException("This id has existed");
            }
            else
            {
                throw new ContractValidateException(
                    "contract type error,expected type [SetAccountIdContract],real type[" + contract.GetType().Name + "]");
            }

            return true;
        }
        #endregion
    }
}
