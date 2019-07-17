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
    public class UpdateAccountActuator : AbstractActuator
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public UpdateAccountActuator(Any contract, DatabaseManager db_manager) : base(contract, db_manager) { }
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
            AccountUpdateContract account_update_contract;
            long fee = CalcFee();

            try
            {
                account_update_contract = contract.Unpack<AccountUpdateContract>();
            }
            catch (InvalidProtocolBufferException e)
            {
                Logger.Debug(e.Message);
                result.SetStatus(fee, Transaction.Types.Result.Types.code.Failed);

                throw new ContractExeException(e.Message);
            }

            byte[] owner_address = account_update_contract.OwnerAddress.ToByteArray();

            AccountCapsule account = db_manager.Account.Get(owner_address);

            account.AccountName = ByteString.CopyFrom(account_update_contract.AccountName.ToByteArray());
            db_manager.Account.Put(owner_address, account);
            db_manager.AccountIndex.Put(account);

            result.SetStatus(fee, code.Sucess);

            return true;
        }

        public override ByteString GetOwnerAddress()
        {
            throw new NotImplementedException();
        }

        public override bool Validate()
        {
            if (this.contract == null)
                throw new ContractValidateException("No contract!");

            if (this.db_manager == null)
                throw new ContractValidateException("No dbManager!");

            if (this.contract.Is(AccountUpdateContract.Descriptor))
            {
                AccountUpdateContract account_update_contract;
                try
                {
                    account_update_contract = contract.Unpack<AccountUpdateContract>();
                }
                catch (InvalidProtocolBufferException e)
                {
                    Logger.Debug(e.Message);
                    throw new ContractValidateException(e.Message);
                }

                byte[] owner_address = account_update_contract.OwnerAddress.ToByteArray();
                byte[] account_name = account_update_contract.AccountName.ToByteArray();

                if (!TransactionUtil.ValidAccountName(account_name))
                {
                    throw new ContractValidateException("Invalid accountName");
                }
                if (!Wallet.IsValidAddress(owner_address))
                {
                    throw new ContractValidateException("Invalid ownerAddress");
                }

                AccountCapsule account = db_manager.Account.Get(owner_address);
                if (account == null)
                {
                    throw new ContractValidateException("Account has not existed");
                }

                if (account.AccountName != null && !account.AccountName.IsEmpty
                    && db_manager.DynamicProperties.GetAllowUpdateAccountName() == 0)
                {
                    throw new ContractValidateException("This account name already exist");
                }

                if (db_manager.AccountIndex.Contains(account_name)
                    && db_manager.DynamicProperties.GetAllowUpdateAccountName() == 0)
                {
                    throw new ContractValidateException("This name has existed");
                }
            }
            else
            {
                throw new ContractValidateException(
                            "contract type error,expected type [AccountUpdateContract], real type[" + contract.GetType().Name + "]");
            }

            return true;
        }
        #endregion
    }
}
