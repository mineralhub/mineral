using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Mineral.Core.Capsule;
using Mineral.Core.Database;
using Mineral.Core.Exception;
using Protocol;
using static Protocol.Account.Types;
using static Protocol.Transaction.Types.Result.Types;

namespace Mineral.Core.Actuator
{
    public class UnfreezeAssetActuator : AbstractActuator
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public UnfreezeAssetActuator(Any contract, DataBaseManager manager) : base(contract, manager) { }
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
                UnfreezeAssetContract unfreeze_asset_contract = contract.Unpack<UnfreezeAssetContract>();
                byte[] owner_address = unfreeze_asset_contract.OwnerAddress.ToByteArray();

                long unfreeze_asset = 0L;
                long now = this.db_manager.GetHeadBlockTimestamp();
                AccountCapsule account = this.db_manager.Account.Get(owner_address);

                List<Frozen> frozen_list = new List<Frozen>(account.FrozenList);
                foreach (Account.Types.Frozen frozen in frozen_list)
                {
                    if (frozen.ExpireTime <= now)
                    {
                        unfreeze_asset += frozen.FrozenBalance;
                        account.FrozenList.Remove(frozen);
                    }
                }

                if (this.db_manager.DynamicProperties.GetAllowSameTokenName() == 0)
                {
                    account.AddAssetAmountV2(account.AssetIssuedName.ToByteArray(), unfreeze_asset, this.db_manager);
                }
                else
                {
                    account.AddAssetAmountV2(account.AssetIssuedID.ToByteArray(), unfreeze_asset, this.db_manager);
                }

                account.Instance.FrozenSupply.Clear();
                account.Instance.FrozenSupply.AddRange(frozen_list);

                this.db_manager.Account.Put(owner_address, account);
                result.SetStatus(fee, code.Sucess);
            }
            catch (InvalidProtocolBufferException e)
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

            return true;
        }

        public override ByteString GetOwnerAddress()
        {
            return contract.Unpack<UnfreezeAssetContract>().OwnerAddress;
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

            if (this.contract.Is(UnfreezeAssetContract.Descriptor))
            {
                UnfreezeAssetContract unfreeze_asset_contract = null;
                try
                {
                    unfreeze_asset_contract = this.contract.Unpack<UnfreezeAssetContract>();
                }
                catch (InvalidProtocolBufferException e)
                {
                    Logger.Debug(e.Message);
                    throw new ContractValidateException(e.Message);
                }
                byte[] owner_address = unfreeze_asset_contract.OwnerAddress.ToByteArray();
                if (!Wallet.AddressValid(owner_address))
                {
                    throw new ContractValidateException("Invalid address");
                }

                AccountCapsule account = this.db_manager.Account.Get(owner_address);
                if (account == null)
                {
                    throw new ContractValidateException(
                        "Account[" + owner_address.ToHexString() + "] not exists");
                }

                if (account.FrozenSupplyCount <= 0)
                {
                    throw new ContractValidateException("no frozen supply balance");
                }

                if (this.db_manager.DynamicProperties.GetAllowSameTokenName() == 0)
                {
                    if (account.AssetIssuedName.IsEmpty)
                    {
                        throw new ContractValidateException("this account did not issue any asset");
                    }
                }
                else
                {
                    if (account.AssetIssuedID.IsEmpty)
                    {
                        throw new ContractValidateException("this account did not issue any asset");
                    }
                }

                long now = this.db_manager.GetHeadBlockTimestamp();
                if (account.FrozenSupplyList.Where(frozen => frozen.ExpireTime < now).Count() <= 0)
                {
                    throw new ContractValidateException("It's not time to unfreeze asset supply");
                }
            }
            else
            {
                throw new ContractValidateException(
                    "contract type error,expected type [UnfreezeAssetContract],real type[" + contract.GetType().Name + "]");
            }

            return true;
        }
        #endregion
    }
}
