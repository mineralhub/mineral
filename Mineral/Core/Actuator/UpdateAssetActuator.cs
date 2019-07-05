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
    public class UpdateAssetActuator : AbstractActuator
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public UpdateAssetActuator(Any contract, DataBaseManager db_manager) : base(contract, db_manager) { }
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
                UpdateAssetContract update_asset_contract = this.contract.Unpack<UpdateAssetContract>();

                long new_limit = update_asset_contract.NewLimit;
                long new_public_limit = update_asset_contract.NewPublicLimit;
                byte[] owner_address = update_asset_contract.OwnerAddress.ToByteArray();
                ByteString new_url = update_asset_contract.Url;
                ByteString new_description = update_asset_contract.Description;
                AccountCapsule account = this.db_manager.Account.Get(owner_address);
                AssetIssueCapsule asset_issue = null;
                AssetIssueCapsule asset_issue_v2 = null;

                asset_issue_v2 = this.db_manager.AssetIssueV2.Get(account.AssetIssuedID.ToByteArray());
                asset_issue_v2.FreeAssetNetLimit = new_limit;
                asset_issue_v2.PublicFreeAssetNetLimit = new_public_limit;
                asset_issue_v2.Url = new_url;
                asset_issue_v2.Description = new_description;

                if (this.db_manager.DynamicProperties.GetAllowSameTokenName() == 0)
                {
                    asset_issue = this.db_manager.AssetIssue.Get(account.AssetIssuedName.ToByteArray());
                    asset_issue.FreeAssetNetLimit = new_limit;
                    asset_issue.PublicFreeAssetNetLimit = new_public_limit;
                    asset_issue.Url = new_url;
                    asset_issue.Description = new_description;

                    this.db_manager.AssetIssue.Put(asset_issue.CreateDatabaseKey(), asset_issue);
                    this.db_manager.AssetIssueV2.Put(asset_issue_v2.CreateDatabaseKeyV2(), asset_issue_v2);
                }
                else
                {
                    this.db_manager.AssetIssueV2.Put(asset_issue_v2.CreateDatabaseKeyV2(), asset_issue_v2);
                }

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
            return contract.Unpack<AccountUpdateContract>().OwnerAddress;
        }

        public override bool Validate()
        {
            if (this.contract == null)
                throw new ContractValidateException("No contract!");

            if (this.db_manager == null)
                throw new ContractValidateException("No this.db_manager!");

            if (this.contract.Is(UpdateAssetContract.Descriptor))
            {
                UpdateAssetContract update_asset_contract = null;
                try
                {
                    update_asset_contract = this.contract.Unpack<UpdateAssetContract>();
                }
                catch (InvalidProtocolBufferException e)
                {
                    Logger.Debug(e.Message);
                    throw new ContractValidateException(e.Message);
                }

                long new_limit = update_asset_contract.NewLimit;
                long new_public_limit = update_asset_contract.NewPublicLimit;
                byte[] owner_address = update_asset_contract.OwnerAddress.ToByteArray();
                ByteString new_url = update_asset_contract.Url;
                ByteString new_description = update_asset_contract.Description;

                if (!Wallet.AddressValid(owner_address))
                {
                    throw new ContractValidateException("Invalid owner_address");
                }

                AccountCapsule account = this.db_manager.Account.Get(owner_address);
                if (account == null)
                {
                    throw new ContractValidateException("Account has not existed");
                }

                if (this.db_manager.DynamicProperties.GetAllowSameTokenName() == 0)
                {
                    if (account.AssetIssuedName.IsEmpty)
                    {
                        throw new ContractValidateException("Account has not issue any asset");
                    }

                    if (this.db_manager.AssetIssue.Get(account.AssetIssuedName.ToByteArray()) == null)
                    {
                        throw new ContractValidateException("Asset not exists in AssetIssueStore");
                    }
                }
                else
                {
                    if (account.AssetIssuedID.IsEmpty)
                    {
                        throw new ContractValidateException("Account has not issue any asset");
                    }

                    if (this.db_manager.AssetIssueV2.Get(account.AssetIssuedID.ToByteArray()) == null)
                    {
                        throw new ContractValidateException("Asset not exists  in AssetIssueV2Store");
                    }
                }

                if (!TransactionUtil.ValidUrl(new_url.ToByteArray()))
                {
                    throw new ContractValidateException("Invalid url");
                }

                if (!TransactionUtil.ValidAssetDescription(new_description.ToByteArray()))
                {
                    throw new ContractValidateException("Invalid description");
                }

                if (new_limit < 0 || new_limit >= this.db_manager.DynamicProperties.GetOneDayNetLimit())
                {
                    throw new ContractValidateException("Invalid FreeAssetNetLimit");
                }

                if (new_public_limit < 0 || new_public_limit >= this.db_manager.DynamicProperties.GetOneDayNetLimit())
                {
                    throw new ContractValidateException("Invalid PublicFreeAssetNetLimit");
                }
            }
            else
            {
                throw new ContractValidateException(
                    "contract type error,expected type [UpdateAssetContract],real type[" + contract.GetType().Name + "]");
            }

            return true;
        }
        #endregion
    }
}
