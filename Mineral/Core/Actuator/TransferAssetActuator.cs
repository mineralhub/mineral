using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Mineral.Common.Storage;
using Mineral.Core.Capsule;
using Mineral.Core.Database;
using Mineral.Core.Exception;
using Mineral.Utils;
using Protocol;
using static Protocol.Transaction.Types.Result.Types;

namespace Mineral.Core.Actuator
{
    public class TransferAssetActuator : AbstractActuator
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public TransferAssetActuator(Any contract, DatabaseManager db_manager) : base(contract, db_manager) { }
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
                TransferAssetContract transfer_asset_contract = this.contract.Unpack<TransferAssetContract>();
                byte[] owner_address = transfer_asset_contract.OwnerAddress.ToByteArray();
                byte[] to_address = transfer_asset_contract.ToAddress.ToByteArray();
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

                ByteString asset_name = transfer_asset_contract.AssetName;
                long amount = transfer_asset_contract.Amount;

                this.db_manager.AdjustBalance(owner_address, -fee);
                this.db_manager.AdjustBalance(this.db_manager.Account.GetBlackHole().CreateDatabaseKey(), fee);

                AccountCapsule owner_account = this.db_manager.Account.Get(owner_address);
                if (!owner_account.ReduceAssetAmountV2(asset_name.ToByteArray(), amount, this.db_manager))
                    throw new ContractExeException("reduceAssetAmount failed !");

                this.db_manager.Account.Put(owner_address, owner_account);

                to_account.AddAssetAmountV2(asset_name.ToByteArray(), amount, this.db_manager);
                this.db_manager.Account.Put(to_address, to_account);

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
                result.SetStatus(fee, code.Failed);
                throw new ContractExeException(e.Message);
            }
            catch (ArithmeticException e)
            {
                result.SetStatus(fee, code.Failed);
                throw new ContractExeException(e.Message);
            }

            return true;
        }

        public override ByteString GetOwnerAddress()
        {
            return this.contract.Unpack<TransferAssetContract>().OwnerAddress;
        }

        public override bool Validate()
        {
            if (this.contract == null)
                throw new ContractValidateException("No contract!");

            if (this.db_manager == null)
                throw new ContractValidateException("No dbManager!");

            if (this.contract.Is(TransferAssetContract.Descriptor))
            {
                TransferAssetContract transfer_asset_contract = null;
                try
                {
                    transfer_asset_contract = this.contract.Unpack<TransferAssetContract>();
                }
                catch (InvalidProtocolBufferException e)
                {
                    Logger.Debug(e.Message);
                    throw new ContractValidateException(e.Message);
                }

                long fee = CalcFee();
                byte[] owner_address = transfer_asset_contract.OwnerAddress.ToByteArray();
                byte[] to_address = transfer_asset_contract.ToAddress.ToByteArray();
                byte[] asset_name = transfer_asset_contract.AssetName.ToByteArray();
                long amount = transfer_asset_contract.Amount;

                if (!Wallet.IsValidAddress(owner_address))
                    throw new ContractValidateException("Invalid ownerAddress");

                if (!Wallet.IsValidAddress(to_address))
                    throw new ContractValidateException("Invalid toAddress");

                if (amount <= 0)
                    throw new ContractValidateException("Amount must greater than 0.");

                if (owner_address.SequenceEqual(to_address))
                    throw new ContractValidateException("Cannot transfer asset to yourself.");

                AccountCapsule owner_account = this.db_manager.Account.Get(owner_address);
                if (owner_account == null)
                    throw new ContractValidateException("No owner account!");

                if (!this.db_manager.GetAssetIssueStoreFinal().Contains(asset_name))
                    throw new ContractValidateException("No asset !");

                Dictionary<string, long> asset = this.db_manager.DynamicProperties.GetAllowSameTokenName() == 0 ? owner_account.Asset : owner_account.AssetV2;
                if (asset == null || asset.Count == 0)
                    throw new ContractValidateException("Owner no asset!");

                asset.TryGetValue(Encoding.UTF8.GetString(asset_name), out long asset_balance);
                if (asset_balance <= 0)
                    throw new ContractValidateException("assetBalance must greater than 0.");

                if (amount > asset_balance)
                    throw new ContractValidateException("assetBalance is not sufficient.");

                AccountCapsule to_account = this.db_manager.Account.Get(to_address);
                if (to_account != null)
                {
                    bool success = false;
                    if (this.db_manager.DynamicProperties.GetAllowSameTokenName() == 0)
                        success = to_account.Asset.TryGetValue(Encoding.UTF8.GetString(asset_name), out asset_balance);
                    else
                        success = to_account.AssetV2.TryGetValue(Encoding.UTF8.GetString(asset_name), out asset_balance);

                    if (success)
                    {
                        try
                        {
                            asset_balance = asset_balance + amount;
                        }
                        catch (System.Exception e)
                        {
                            Logger.Debug(e.Message);
                            throw new ContractValidateException(e.Message);
                        }
                    }
                }
                else
                {
                    fee = fee + this.db_manager.DynamicProperties.GetCreateNewAccountFeeInSystemContract();
                    if (owner_account.Balance < fee)
                    {
                        throw new ContractValidateException(
                            "Validate TransferAssetActuator error, insufficient fee.");
                    }
                }
            }
            else
            {
                  throw new ContractValidateException(
                      "contract type error,expected type [TransferAssetContract],real type[" + contract.GetType().Name + "]");
            }

            return true;
        }

        public static bool ValidateForSmartContract(IDeposit deposit, byte[] owner_address, byte[] to_address, byte[] token_id, long amount)
        {
            if (deposit == null)
                throw new ContractValidateException("No deposit!");

            byte[] token_id_leading_zero = ByteUtil.StripLeadingZeroes(token_id);

            if (!Wallet.IsValidAddress(owner_address))
                throw new ContractValidateException("Invalid ownerAddress");

            if (!Wallet.IsValidAddress(to_address))
                throw new ContractValidateException("Invalid toAddress");

            if (amount <= 0)
                throw new ContractValidateException("Amount must greater than 0.");

            if (owner_address.SequenceEqual(to_address))
                throw new ContractValidateException("Cannot transfer asset to yourself.");

            AccountCapsule owner_account = deposit.GetAccount(owner_address);
            if (owner_account == null)
                throw new ContractValidateException("No owner account!");

            if (deposit.GetAssetIssue(token_id_leading_zero) == null)
                throw new ContractValidateException("No asset !");

            if (!deposit.DBManager.GetAssetIssueStoreFinal().Contains(token_id_leading_zero))
                throw new ContractValidateException("No asset !");

            Dictionary<string, long> asset = deposit.DBManager.DynamicProperties.GetAllowSameTokenName() == 0 ?
                                owner_account.Asset : owner_account.AssetV2;

            if (asset == null || asset.Count <= 0)
                throw new ContractValidateException("Owner no asset!");

            asset.TryGetValue(Encoding.UTF8.GetString(token_id_leading_zero), out long asset_balance);
            if (asset_balance <= 0)
                throw new ContractValidateException("assetBalance must greater than 0.");

            if (amount > asset_balance)
                throw new ContractValidateException("assetBalance is not sufficient.");

            AccountCapsule to_account = deposit.GetAccount(to_address);
            if (to_account != null)
            {
                bool success = false;
                if (deposit.DBManager.DynamicProperties.GetAllowSameTokenName() == 0)
                {
                    success = to_account.Asset.TryGetValue(Encoding.UTF8.GetString(token_id_leading_zero), out asset_balance);
                }
                else
                {
                    success = to_account.AssetV2.TryGetValue(Encoding.UTF8.GetString(token_id_leading_zero), out asset_balance);
                }

                if (success)
                {
                    try
                    {
                        asset_balance += amount; //check if overflow
                    }
                    catch (System.Exception e)
                    {
                        Logger.Debug(e.Message);
                        throw new ContractValidateException(e.Message);
                    }
                }
            }
            else
            {
                throw new ContractValidateException(
                    "Validate InternalTransfer error, no ToAccount. And not allowed to create account in smart contract.");
            }

            return true;
        }
        #endregion
    }
}
