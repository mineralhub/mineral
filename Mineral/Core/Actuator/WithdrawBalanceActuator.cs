using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Mineral.Core.Capsule;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Database;
using Mineral.Core.Exception;
using Protocol;
using static Protocol.Transaction.Types.Result.Types;

namespace Mineral.Core.Actuator
{
    public class WithdrawBalanceActuator : AbstractActuator
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public WithdrawBalanceActuator(Any contract, DatabaseManager db_manager) : base(contract, db_manager) { }
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
            WithdrawBalanceContract withdraw_contract;

            try
            {
                withdraw_contract = contract.Unpack<WithdrawBalanceContract>();
            }
            catch (InvalidProtocolBufferException e)
            {
                Logger.Debug(e.Message);
                result.SetStatus(fee, code.Failed);
                throw new ContractExeException(e.Message);
            }

            AccountCapsule account = (Deposit == null) ? 
                this.db_manager.Account.Get(withdraw_contract.OwnerAddress.ToByteArray()) : Deposit.GetAccount(withdraw_contract.OwnerAddress.ToByteArray());

            long now = this.db_manager.GetHeadBlockTimestamp();
            account.Instance.Balance = account.Balance + account.Allowance;
            account.Allowance = 0;
            account.LatestWithdrawTime = now;

            if (Deposit == null)
            {
                this.db_manager.Account.Put(account.CreateDatabaseKey(), account);
            }
            else
            {
                Deposit.PutAccountValue(account.CreateDatabaseKey(), account);
            }

            result.WithdrawAmount = account.Allowance;
            result.SetStatus(fee, code.Sucess);

            return true;
        }

        public override ByteString GetOwnerAddress()
        {
            return contract.Unpack<WithdrawBalanceContract>().OwnerAddress;
        }

        public override bool Validate()
        {
            if (this.contract == null)
                throw new ContractValidateException("No contract!");

            if (this.db_manager == null && (Deposit == null || Deposit.DBManager == null))
                throw new ContractValidateException("No this.db_manager!");

            if (this.contract.Is(WithdrawBalanceContract.Descriptor))
            {
                WithdrawBalanceContract withdraw_contract = null;
                try
                {
                    withdraw_contract = this.contract.Unpack<WithdrawBalanceContract>();
                }
                catch (InvalidProtocolBufferException e)
                {
                    Logger.Debug(e.Message);
                    throw new ContractValidateException(e.Message);
                }

                byte[] owner_address = withdraw_contract.OwnerAddress.ToByteArray();
                if (!Wallet.AddressValid(owner_address))
                    throw new ContractValidateException("Invalid address");

                AccountCapsule account = Deposit == null ?
                    this.db_manager.Account.Get(owner_address) : Deposit.GetAccount(owner_address);

                if (account == null)
                {
                    throw new ContractValidateException(
                        ActuatorParameter.ACCOUNT_EXCEPTION_STR + owner_address.ToHexString() + "] not exists");
                }

                if (!this.db_manager.Witness.Contains(owner_address))
                {
                    throw new ContractValidateException(
                        ActuatorParameter.ACCOUNT_EXCEPTION_STR + owner_address.ToHexString() + "] is not a witnessAccount");
                }

                if (Args.Instance.Genesisblock.Witnesses.Where(witness => owner_address.SequenceEqual(witness.Address)).Count() > 0)
                {
                    throw new ContractValidateException(
                        ActuatorParameter.ACCOUNT_EXCEPTION_STR + owner_address.ToHexString()
                            + "] is a guard representative and is not allowed to withdraw Balance");
                }

                long now = this.db_manager.GetHeadBlockTimestamp();
                long frozen_time = Deposit == null ?
                    this.db_manager.DynamicProperties.GetWitnessAllowanceFrozenTime() * 86_400_000L
                    : Deposit.GetWitnessAllowanceFrozenTime() * 86_400_000L;

                if (now - account.LatestWithdrawTime < frozen_time)
                {
                    throw new ContractValidateException("The last withdraw time is "
                        + account.LatestWithdrawTime + ",less than 24 hours");
                }

                if (account.Allowance <= 0)
                {
                    throw new ContractValidateException("witnessAccount does not have any allowance");
                }

                try
                {
                    long add_result = checked(account.Balance + account.Allowance);
                }
                catch (OverflowException e)
                {
                    Logger.Debug(e.Message);
                    throw new ContractValidateException(e.Message);
                }
            }
            else
            {
                throw new ContractValidateException(
                    "contract type error, expected type [WithdrawBalanceContract],real type[" + contract.GetType().Name + "]");
            }

            return true;
        }
        #endregion
    }
}
