using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Mineral.Common.Storage;
using Mineral.Core.Capsule;
using Mineral.Core.Config;
using Mineral.Core.Database;
using Mineral.Core.Exception;
using Protocol;
using static Protocol.Transaction.Types.Result.Types;

namespace Mineral.Core.Actuator
{
    public class TransferActuator : AbstractActuator
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public TransferActuator(Any contract, DataBaseManager db_manager) : base(contract, db_manager) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override long CalcFee()
        {
            return Parameter.ChainParameters.TRANSFER_FEE;
        }

        public override bool Execute(TransactionResultCapsule result)
        {
            long fee = CalcFee();
            try
            {
                TransferContract transfer_contract = this.contract.Unpack<TransferContract>();
                long amount = transfer_contract.Amount;
                byte[] to_address = transfer_contract.ToAddress.ToByteArray();
                byte[] owner_address = transfer_contract.OwnerAddress.ToByteArray();

                AccountCapsule to_account = this.db_manager.Account.Get(to_address);
                if (to_account == null)
                {
                    bool default_permission = this.db_manager.DynamicProperties.GetAllowMultiSign() == 1;

                    to_account = new AccountCapsule(ByteString.CopyFrom(to_address),
                                                    AccountType.Normal,
                                                    this.db_manager.GetHeadBlockTimestamp(),
                                                    default_permission,
                                                    this.db_manager);

                    this.db_manager.Account.Put(to_address, to_account);
                    fee = fee + this.db_manager.DynamicProperties.GetCreateNewAccountFeeInSystemContract();
                }

                this.db_manager.AdjustBalance(owner_address, -fee);
                this.db_manager.AdjustBalance(this.db_manager.Account.GetBlackHole().CreateDatabaseKey(), fee);

                result.SetStatus(fee, code.Sucess);

                this.db_manager.AdjustBalance(owner_address, -amount);
                this.db_manager.AdjustBalance(to_address, amount);
            }
            catch (BalanceInsufficientException e)
            {
                Logger.Debug(e.Message);
                result.SetStatus(fee, code.Failed);
                throw new ContractExeException(e.Message);
            }
            catch (ArithmeticException e)
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
            return this.contract.Unpack<TransferContract>().OwnerAddress;
    }

        public override bool Validate()
        {
            if (this.contract == null)
                throw new ContractValidateException("No contract");

            if (this.db_manager == null)
                throw new ContractValidateException("No DB Manager");

            if (this.contract.Is(TransferContract.Descriptor))
            {
                long fee = CalcFee();
                TransferContract transfer_contract = null;
                try
                {
                    transfer_contract = contract.Unpack<TransferContract>();
                }
                catch (InvalidProtocolBufferException e)
                {
                    Logger.Debug(e.Message);
                    throw new ContractValidateException(e.Message);
                }

                byte[] to_address = transfer_contract.ToAddress.ToByteArray();
                byte[] owner_address = transfer_contract.OwnerAddress.ToByteArray();
                long amount = transfer_contract.Amount;

                if (!Wallet.AddressValid(owner_address))
                    throw new ContractValidateException("Invalid ownerAddress");

                if (!Wallet.AddressValid(to_address))
                    throw new ContractValidateException("Invalid toAddress");

                if (to_address.SequenceEqual(owner_address))
                    throw new ContractValidateException("Cannot transfer trx to yourself.");

                AccountCapsule owner_account = this.db_manager.Account.Get(owner_address);
                if (owner_account == null)
                    throw new ContractValidateException("Validate TransferContract error, no OwnerAccount.");

                long balance = owner_account.Balance;

                if (amount <= 0)
                    throw new ContractValidateException("Amount must greater than 0.");

                try
                {
                    AccountCapsule to_account = this.db_manager.Account.Get(to_address);
                    if (to_account == null)
                    {
                        fee = fee + this.db_manager.DynamicProperties.GetCreateNewAccountFeeInSystemContract();
                    }

                    if (balance < amount + fee)
                    {
                        throw new ContractValidateException(
                            "Validate TransferContract error, balance is not sufficient.");
                    }
                }
                catch (ArithmeticException e)
                {
                    Logger.Debug(e.Message);
                    throw new ContractValidateException(e.Message);
                }
            }
            else
            {
                throw new ContractValidateException(
                    "contract type error,expected type [TransferContract],real type[" + this.contract.GetType().Name + "]");
            }

            return true;
        }

        public static bool ValidateForSmartContract(IDeposit deposit, byte[] owner_address, byte[] to_address, long amount)
        {
            if (!Wallet.AddressValid(owner_address))
                throw new ContractValidateException("Invalid ownerAddress");

            if (!Wallet.AddressValid(to_address))
                throw new ContractValidateException("Invalid toAddress");

            if (to_address.SequenceEqual(owner_address))
                throw new ContractValidateException("Cannot transfer trx to yourself.");

            AccountCapsule owner_account = deposit.GetAccount(owner_address);
            if (owner_account == null)
                throw new ContractValidateException("Validate InternalTransfer error, no OwnerAccount.");

            AccountCapsule to_account = deposit.GetAccount(to_address);
            if (to_account == null)
            {
                throw new ContractValidateException(
                    "Validate InternalTransfer error, no ToAccount. And not allowed to create account in smart contract.");
            }

            long balance = owner_account.Balance;
            if (amount < 0)
                throw new ContractValidateException("Amount must greater than or equals 0.");

            try
            {
                if (balance < amount)
                {
                    throw new ContractValidateException(
                        "Validate InternalTransfer error, balance is not sufficient.");
                }
            }
            catch (ArithmeticException e)
            {
                Logger.Debug(e.Message);
                throw new ContractValidateException(e.Message);
            }

            return true;
        }
        #endregion
    }
}
