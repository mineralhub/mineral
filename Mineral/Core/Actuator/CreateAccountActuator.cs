using System;
using System.Collections.Generic;
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
    public class CreateAccountActuator : AbstractActuator
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public CreateAccountActuator(Any contract, DatabaseManager db_manager) : base(contract, db_manager) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override long CalcFee()
        {
            return this.db_manager.DynamicProperties.GetCreateNewAccountFeeInSystemContract();
        }

        public override bool Execute(TransactionResultCapsule result)
        {
            long fee = CalcFee();
            try
            {
                AccountCreateContract account_create_contract = contract.Unpack<AccountCreateContract>();
                bool default_permission = this.db_manager.DynamicProperties.GetAllowMultiSign() == 1;
                AccountCapsule account = new AccountCapsule(
                                                account_create_contract,
                                                this.db_manager.GetHeadBlockTimestamp(),
                                                default_permission,
                                                this.db_manager);

                this.db_manager.Account.Put(account_create_contract.AccountAddress.ToByteArray(), account);
                this.db_manager.AdjustBalance(account_create_contract.OwnerAddress.ToByteArray(), -fee);
                this.db_manager.AdjustBalance(this.db_manager.Account.GetBlackHole().CreateDatabaseKey(), fee);

                result.SetStatus(fee, code.Sucess);
            }
            catch (BalanceInsufficientException e)
            {
                Logger.Debug(e.Message);
                result.SetStatus(fee, code.Failed);
                throw new ContractExeException(e.Message);
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
            return contract.Unpack<AccountCreateContract>().OwnerAddress;
        }

        public override bool Validate()
        {
            if (this.contract == null)
                throw new ContractValidateException("No contract!");

            if (this.db_manager == null)
                throw new ContractValidateException("No dbManager!");

            if (this.contract.Is(AccountCreateContract.Descriptor))
            {
                AccountCreateContract account_create_contract = null;
                try
                {
                    account_create_contract = this.contract.Unpack<AccountCreateContract>();
                }
                catch (InvalidProtocolBufferException e)
                {
                    Logger.Debug(e.Message);
                    throw new ContractValidateException(e.Message);
                }

                byte[] owner_address = account_create_contract.OwnerAddress.ToByteArray();
                if (!Wallet.AddressValid(owner_address))
                    throw new ContractValidateException("Invalid ownerAddress");

                AccountCapsule account = this.db_manager.Account.Get(owner_address);
                if (account == null)
                {
                    throw new ContractValidateException(
                        "Account[" + owner_address.ToHexString() + "] not exists");
                }

                long fee = CalcFee();
                if (account.Balance < fee)
                {
                    throw new ContractValidateException(
                        "Validate CreateAccountActuator error, insufficient fee.");
                }

                byte[] account_address = account_create_contract.AccountAddress.ToByteArray();
                if (!Wallet.AddressValid(account_address))
                    throw new ContractValidateException("Invalid account address");

                if (this.db_manager.Account.Contains(account_address))
                    throw new ContractValidateException("Account has existed");
            }
            else
            {
                throw new ContractValidateException(
                    "contract type error,expected type [AccountCreateContract],real type[" + contract.GetType().Name + "]");
            }

            return true;
        }
        #endregion
    }
}
