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
using static Protocol.Account.Types;
using static Protocol.AssetIssueContract.Types;
using static Protocol.Transaction.Types.Result.Types;

namespace Mineral.Core.Actuator
{
    public class AssetIssueActuator : AbstractActuator
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public AssetIssueActuator(Any contract, DatabaseManager db_manager) : base(contract, db_manager) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override long CalcFee()
        {
            return this.db_manager.DynamicProperties.GetAssetIssueFee();
        }

        public override bool Execute(TransactionResultCapsule result)
        {
            long fee = CalcFee();

            try
            {
                AssetIssueContract asset_issue_contract = contract.Unpack<AssetIssueContract>();
                AssetIssueCapsule asset_issue = new AssetIssueCapsule(asset_issue_contract);
                AssetIssueCapsule asset_issue_v2 = new AssetIssueCapsule(asset_issue_contract);
                byte[] owner_address = asset_issue_contract.OwnerAddress.ToByteArray();
                long token_id = this.db_manager.DynamicProperties.GetTokenIdNum();

                token_id++;
                asset_issue.Id = token_id.ToString();
                asset_issue_v2.Id = token_id.ToString();
                this.db_manager.DynamicProperties.PutTokenIdNum(token_id);

                if (this.db_manager.DynamicProperties.GetAllowSameTokenName() == 0)
                {
                    asset_issue_v2.Percision = 0;
                    this.db_manager.AssetIssue.Put(asset_issue.CreateDatabaseKey(), asset_issue);
                    this.db_manager.AssetIssueV2.Put(asset_issue_v2.CreateDatabaseKeyV2(), asset_issue_v2);
                }
                else
                {
                    this.db_manager.AssetIssueV2.Put(asset_issue_v2.CreateDatabaseKeyV2(), asset_issue_v2);
                }

                this.db_manager.AdjustBalance(owner_address, -fee);
                this.db_manager.AdjustBalance(this.db_manager.Account.GetBlackHole().Address.ToByteArray(), fee);

                AccountCapsule account = this.db_manager.Account.Get(owner_address);
                List<FrozenSupply> frozen_supplys = new List<FrozenSupply>(asset_issue_contract.FrozenSupply);

                long remain_supply = asset_issue_contract.TotalSupply;
                List<Frozen> frozens = new List<Frozen>();
                long startTime = asset_issue_contract.StartTime;

                foreach (AssetIssueContract.Types.FrozenSupply supply in asset_issue_contract.FrozenSupply)
                {
                    long expire_time = startTime + supply.FrozenDays * 86_400_000;
                    Frozen frozen = new Frozen();
                    frozen.FrozenBalance = supply.FrozenAmount;
                    frozen.ExpireTime = expire_time;
                    frozens.Add(frozen);
                    remain_supply -= supply.FrozenAmount;
                }

                if (this.db_manager.DynamicProperties.GetAllowSameTokenName() == 0)
                {
                    account.AddAsset(asset_issue.CreateDatabaseKey(), remain_supply);
                }
                account.AssetIssuedName = ByteString.CopyFrom(asset_issue.CreateDatabaseKey());
                account.AssetIssuedID = ByteString.CopyFrom(asset_issue.CreateDatabaseKeyV2());
                account.AddAssetV2(asset_issue_v2.CreateDatabaseKeyV2(), remain_supply);
                account.FrozenSupplyList.AddRange(frozens);

                this.db_manager.Account.Put(owner_address, account);

                result.AssetIssueID = token_id.ToString();
                result.SetStatus(fee, code.Sucess);
            }
            catch (InvalidProtocolBufferException e)
            {
                Logger.Debug(e.Message);
                result.SetStatus(fee, code.Failed);
                throw new ContractExeException(e.Message);
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

            return true;
        }

        public override ByteString GetOwnerAddress()
        {
            return contract.Unpack<AssetIssueContract>().OwnerAddress;
        }

        public override bool Validate()
        {
            if (this.contract == null)
                throw new ContractValidateException("No contract!");

            if (this.db_manager == null)
                throw new ContractValidateException("No dbManager!");

            if (this.contract.Is(AssetIssueContract.Descriptor))
            {

                AssetIssueContract asset_issue_contract = null;
                try
                {
                    asset_issue_contract = this.contract.Unpack<AssetIssueContract>();
                }
                catch (InvalidProtocolBufferException e)
                {
                    Logger.Debug(e.Message);
                    throw new ContractValidateException(e.Message);
                }

                byte[] owner_address = asset_issue_contract.OwnerAddress.ToByteArray();
                if (!Wallet.IsValidAddress(owner_address))
                    throw new ContractValidateException("Invalid ownerAddress");

                if (!TransactionUtil.ValidAssetName(asset_issue_contract.Name.ToByteArray()))
                    throw new ContractValidateException("Invalid assetName");

                if (this.db_manager.DynamicProperties.GetAllowSameTokenName() != 0)
                {
                    string name = asset_issue_contract.Name.ToStringUtf8().ToLower();
                    if (name.Equals("tx"))
                    {
                        throw new ContractValidateException("assetName can't be tx");
                    }
                }

                int precision = asset_issue_contract.Precision;
                if (precision != 0 && this.db_manager.DynamicProperties.GetAllowSameTokenName() != 0)
                {
                    if (precision < 0 || precision > 6)
                    {
                        throw new ContractValidateException("precision cannot exceed 6");
                    }
                }

                if ((!asset_issue_contract.Abbr.IsEmpty) && !TransactionUtil.ValidAssetName(asset_issue_contract.Abbr.ToByteArray()))
                {
                    throw new ContractValidateException("Invalid abbreviation for token");
                }

                if (!TransactionUtil.ValidUrl(asset_issue_contract.Url.ToByteArray()))
                {
                    throw new ContractValidateException("Invalid url");
                }

                if (!TransactionUtil.ValidAssetDescription(asset_issue_contract.Description.ToByteArray()))
                {
                    throw new ContractValidateException("Invalid description");
                }

                if (asset_issue_contract.StartTime == 0)
                {
                    throw new ContractValidateException("Start time should be not empty");
                }
                if (asset_issue_contract.EndTime == 0)
                {
                    throw new ContractValidateException("End time should be not empty");
                }
                if (asset_issue_contract.EndTime <= asset_issue_contract.StartTime)
                {
                    throw new ContractValidateException("End time should be greater than start time");
                }
                if (asset_issue_contract.StartTime <= this.db_manager.GetHeadBlockTimestamp())
                {
                    throw new ContractValidateException("Start time should be greater than HeadBlockTime");
                }

                if (this.db_manager.DynamicProperties.GetAllowSameTokenName() == 0
                    && this.db_manager.AssetIssue.Get(asset_issue_contract.Name.ToByteArray()) != null)
                {
                    throw new ContractValidateException("Token exists");
                }

                if (asset_issue_contract.TotalSupply <= 0)
                {
                    throw new ContractValidateException("TotalSupply must greater than 0!");
                }

                if (asset_issue_contract.TrxNum <= 0)
                {
                    throw new ContractValidateException("TrxNum must greater than 0!");
                }

                if (asset_issue_contract.Num <= 0)
                {
                    throw new ContractValidateException("Num must greater than 0!");
                }

                if (asset_issue_contract.PublicFreeAssetNetUsage != 0)
                {
                    throw new ContractValidateException("PublicFreeAssetNetUsage must be 0!");
                }

                if (asset_issue_contract.FrozenSupply.Count > this.db_manager.DynamicProperties.GetMaxFrozenSupplyNumber())
                {
                    throw new ContractValidateException("Frozen supply list length is too long");
                }

                if (asset_issue_contract.FreeAssetNetLimit < 0
                    || asset_issue_contract.FreeAssetNetLimit >= this.db_manager.DynamicProperties.GetOneDayNetLimit())
                {
                    throw new ContractValidateException("Invalid FreeAssetNetLimit");
                }

                if (asset_issue_contract.PublicFreeAssetNetLimit < 0
                    || asset_issue_contract.PublicFreeAssetNetLimit >= this.db_manager.DynamicProperties.GetOneDayNetLimit())
                {
                    throw new ContractValidateException("Invalid PublicFreeAssetNetLimit");
                }

                long remain_supply = asset_issue_contract.TotalSupply;
                long min_frozen_time = this.db_manager.DynamicProperties.GetMinFrozenSupplyTime();
                long max_frozen_time = this.db_manager.DynamicProperties.GetMaxFrozenSupplyTime();

                foreach (AssetIssueContract.Types.FrozenSupply frozen in asset_issue_contract.FrozenSupply)
                {
                    if (frozen.FrozenAmount <= 0)
                    {
                        throw new ContractValidateException("Frozen supply must be greater than 0!");
                    }
                    if (frozen.FrozenAmount > remain_supply)
                    {
                        throw new ContractValidateException("Frozen supply cannot exceed total supply");
                    }
                    if (!(frozen.FrozenDays >= min_frozen_time
                        && frozen.FrozenDays <= max_frozen_time))
                    {
                        throw new ContractValidateException(
                            "frozenDuration must be less than " + max_frozen_time + " days "
                                + "and more than " + min_frozen_time + " days");
                    }
                    remain_supply -= frozen.FrozenAmount;
                }

                AccountCapsule account = this.db_manager.Account.Get(owner_address);
                if (account == null)
                {
                    throw new ContractValidateException("Account not exists");
                }

                if (!account.AssetIssuedName.IsEmpty)
                {
                    throw new ContractValidateException("An account can only issue one asset");
                }

                if (account.Balance < CalcFee())
                {
                    throw new ContractValidateException("No enough balance for fee!");
                }
            }
            else
            {
                throw new ContractValidateException(
                    "contract type error,expected type [AssetIssueContract],real type[" + contract.GetType().Name + "]");
            }

            return true;
        }
        #endregion
    }
}
