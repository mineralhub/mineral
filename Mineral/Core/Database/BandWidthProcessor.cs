using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Mineral.Core.Capsule;
using Mineral.Core.Exception;
using Protocol;
using static Protocol.Transaction.Types;
using static Protocol.Transaction.Types.Contract.Types;

namespace Mineral.Core.Database
{
    public class BandwidthProcessor : ResourceProcessor
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public BandwidthProcessor(DatabaseManager db_manager) : base(db_manager) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private void UpdateUsage(AccountCapsule account, long now)
        {
            account.NetUsage = Increase(account.NetUsage, 0, account.LatestConsumeTime, now);
            account.FreeNetUsage = Increase(account.FreeNetUsage, 0, account.LatestConsumeFreeTime, now);

            if (this.db_manager.DynamicProperties.GetAllowSameTokenName() == 0)
            {
                foreach (var asset in account.Asset)
                {
                    long last_usage = account.GetFreeAssetNetUsange(asset.Key);
                    long last_time = account.GetLatestAssetOperationTime(asset.Key);
                    account.PutFreeAssetNetUsage(asset.Key,
                                                 Increase(last_usage, 0, last_time, now));
                }
            }

            foreach (var asset_v2 in account.AssetV2)
            {
                long last_usage = account.GetFreeAssetNetUsange(asset_v2.Key);
                long last_time = account.GetLatestAssetOperationTime(asset_v2.Key);
                account.PutFreeAssetNetUsage(asset_v2.Key,
                                             Increase(last_usage, 0, last_time, now));
            }
        }

        private void ConsumeForCreateNewAccount(AccountCapsule account, long bytes, long now, TransactionTrace trace)
        {
            bool ret = ConsumeBandwidthForCreateNewAccount(account, bytes, now);

            if (!ret)
            {
                ret = ConsumeFeeForCreateNewAccount(account, trace);
                if (!ret)
                {
                    throw new AccountResourceInsufficientException();
                }
            }
        }

        private bool UseAccountNet(AccountCapsule account, long bytes, long now)
        {
            long net_usage = account.NetUsage;
            long latest_consume_time = account.LatestConsumeTime;
            long net_limit = CalculateGlobalNetLimit(account);
            long new_net_usage = Increase(net_usage, 0, latest_consume_time, now);

            if (bytes > (net_limit - new_net_usage))
            {
                Logger.Debug("net usage is running out. now use free net usage");
                return false;
            }

            latest_consume_time = now;
            long latestOperationTime = this.db_manager.GetHeadBlockTimestamp();

            new_net_usage = Increase(new_net_usage, bytes, latest_consume_time, now);

            account.NetUsage = new_net_usage;
            account.LatestOperationTime = latestOperationTime;
            account.LatestConsumeTime = latest_consume_time;

            this.db_manager.Account.Put(account.CreateDatabaseKey(), account);

            return true;
        }

        private bool UseAssetAccountNet(Contract contract, AccountCapsule account, long now, long bytes)
        {
            ByteString asset_name = null;
            try
            {
                asset_name = contract.Parameter.Unpack<TransferAssetContract>().AssetName;
            }
            catch (System.Exception ex)
            {
                throw new System.Exception(ex.Message);
            }

            AssetIssueCapsule asset_issue = this.db_manager.GetAssetIssueStoreFinal().Get(asset_name.ToByteArray());
            AssetIssueCapsule asset_issue_v2 = null;

            if (asset_issue == null)
            {
                throw new ContractValidateException("asset not exists");
            }

            string token_name = Encoding.UTF8.GetString(asset_name.ToByteArray());
            string tokenID = asset_issue.Id;

            if (asset_issue.OwnerAddress == account.Address)
            {
                return UseAccountNet(account, bytes, now);
            }

            long new_public_free_asset = Increase(asset_issue.PublicFreeAssetNetUsage,
                                                  0,
                                                  asset_issue.PublicLatestFreeNetTime,
                                                  now);

            if (bytes > (asset_issue.PublicFreeAssetNetLimit - new_public_free_asset))
            {
                Logger.Debug("The " + tokenID + " public free bandwidth is not enough");
                return false;
            }

            long free_asset_net_usage = 0;
            long latest_asset_opration_time = 0;

            if (this.db_manager.DynamicProperties.GetAllowSameTokenName() == 0)
            {
                free_asset_net_usage = account.GetFreeAssetNetUsange(token_name);
                latest_asset_opration_time = account.GetLatestAssetOperationTime(token_name);
            }
            else
            {
                free_asset_net_usage = account.GetFreeAssetNetUsangeV2(tokenID);
                latest_asset_opration_time = account.GetLatestAssetOperationTimeV2(tokenID);
            }

            long new_free_asset_net_usage = Increase(free_asset_net_usage, 0, latest_asset_opration_time, now);

            if (bytes > (asset_issue.FreeAssetNetLimit - new_free_asset_net_usage))
            {
                Logger.Debug("The " + tokenID + " free bandwidth is not enough");
                return false;
            }

            AccountCapsule issuer_account = this.db_manager.Account.Get(asset_issue.OwnerAddress.ToByteArray());

            long issuer_net_usage = issuer_account.NetUsage;
            long latest_consume_time = issuer_account.LatestConsumeTime;
            long issuer_net_limit = CalculateGlobalNetLimit(issuer_account);

            long newIssuerNetUsage = Increase(issuer_net_usage, 0, latest_consume_time, now);

            if (bytes > (issuer_net_limit - newIssuerNetUsage))
            {
                Logger.Debug("The " + tokenID + " issuer'bandwidth is not enough");
                return false;
            }

            latest_consume_time = now;
            latest_asset_opration_time = now;
            asset_issue.PublicLatestFreeNetTime = now;
            long latestOperationTime = this.db_manager.GetHeadBlockTimestamp();

            newIssuerNetUsage = Increase(newIssuerNetUsage, bytes, latest_consume_time, now);
            new_free_asset_net_usage = Increase(new_free_asset_net_usage,
                bytes, latest_asset_opration_time, now);
            new_public_free_asset = Increase(new_public_free_asset, bytes,
                asset_issue.PublicLatestFreeNetTime, now);

            issuer_account.NetUsage = newIssuerNetUsage;
            issuer_account.LatestConsumeTime = latest_consume_time;

            asset_issue.PublicFreeAssetNetUsage = new_public_free_asset;
            asset_issue.PublicLatestFreeNetTime = asset_issue.PublicLatestFreeNetTime;

            account.LatestOperationTime = latestOperationTime;
            if (this.db_manager.DynamicProperties.GetAllowSameTokenName() == 0)
            {
                account.PutLatestAssetOperationTime(token_name,latest_asset_opration_time);
                account.PutFreeAssetNetUsage(token_name, new_free_asset_net_usage);
                account.PutLatestAssetOperationTimeV2(tokenID, latest_asset_opration_time);
                account.PutFreeAssetNetUsageV2(tokenID, new_free_asset_net_usage);

                this.db_manager.AssetIssue.Put(asset_issue.CreateDatabaseKey(), asset_issue);

                asset_issue_v2 = this.db_manager.AssetIssueV2.Get(asset_issue.CreateDatabaseKeyV2());
                asset_issue_v2.PublicFreeAssetNetUsage = new_public_free_asset;
                asset_issue_v2.PublicLatestFreeNetTime = asset_issue.PublicLatestFreeNetTime;
                this.db_manager.AssetIssueV2.Put(asset_issue_v2.CreateDatabaseKeyV2(), asset_issue_v2);
            }
            else
            {
                account.PutLatestAssetOperationTimeV2(tokenID, latest_asset_opration_time);
                account.PutFreeAssetNetUsageV2(tokenID, new_free_asset_net_usage);
                this.db_manager.AssetIssueV2.Put(asset_issue.CreateDatabaseKeyV2(), asset_issue);
            }

            this.db_manager.Account.Put(account.CreateDatabaseKey(), account);
            this.db_manager.Account.Put(issuer_account.CreateDatabaseKey(), issuer_account);

            return true;
        }

        private bool UseFreeNet(AccountCapsule account, long bytes, long now)
        {
            long free_net_limit = this.db_manager.DynamicProperties.GetFreeNetLimit();
            long free_net_usage = account.FreeNetUsage;
            long latest_consume_time = account.LatestConsumeFreeTime;
            long new_free_net_usage = Increase(free_net_usage, 0, latest_consume_time, now);

            if (bytes > (free_net_limit - new_free_net_usage))
            {
                Logger.Debug("free net usage is running out");
                return false;
            }

            long public_net_limit = this.db_manager.DynamicProperties.GetPublicNetLimit();
            long public_net_usage = this.db_manager.DynamicProperties.GetPublicNetUsage();
            long public_net_time = this.db_manager.DynamicProperties.GetPublicNetTime();
            long new_public_net_usage = Increase(public_net_usage, 0, public_net_time, now);

            if (bytes > (public_net_limit - new_public_net_usage))
            {
                Logger.Debug("free public net usage is running out");
                return false;
            }

            latest_consume_time = now;
            public_net_time = now;

            new_free_net_usage = Increase(new_free_net_usage, bytes, latest_consume_time, now);
            new_public_net_usage = Increase(new_public_net_usage, bytes, public_net_time, now);

            account.FreeNetUsage = new_free_net_usage;
            account.LatestConsumeFreeTime = latest_consume_time;
            account.LatestOperationTime = this.db_manager.GetHeadBlockTimestamp(); ;

            this.db_manager.DynamicProperties.PutPublicNetUsage(new_public_net_usage);
            this.db_manager.DynamicProperties.PutPublicNetTime(public_net_time);
            this.db_manager.Account.Put(account.CreateDatabaseKey(), account);
            return true;
        }

        private bool UseTransactionFee(AccountCapsule account, long bytes, TransactionTrace trace)
        {
            bool result = false;
            long fee = this.db_manager.DynamicProperties.GetTransactionFee() * bytes;
            if (ConsumeFee(account, fee))
            {
                trace.SetNetBill(0, fee);
                this.db_manager.DynamicProperties.AddTotalTransactionCost(fee);
                result = true;
            }

            return result;
        }
        #endregion


        #region External Method
        public override void Consume(TransactionCapsule tx, TransactionTrace tx_trace)
        {
            List<Contract> contracts = new List<Contract>(tx.Instance.RawData.Contract);
            if (tx.ResultSize > DefineParameter.MAX_RESULT_SIZE_IN_TX * contracts.Count)
            {
                throw new TooBigTransactionResultException();
            }

            long size = 0;

            if (this.db_manager.DynamicProperties.SupportVm())
            {
                tx.Instance.Ret.Clear();
                size += tx.Instance.CalculateSize();
            }
            else
            {
                size += tx.Size;
            }

            foreach (Contract contract in contracts)
            {
                if (this.db_manager.DynamicProperties.SupportVm())
                {
                    size += DefineParameter.MAX_RESULT_SIZE_IN_TX;
                }

                Logger.Debug(string.Format("tx id {0}, bandwidth cost {1}",
                                           tx.Id,
                                           size));

                tx_trace.SetNetBill(size, 0);
                byte[] address = TransactionCapsule.GetOwner(contract);
                AccountCapsule account = this.db_manager.Account.Get(address);

                if (account == null)
                {
                    throw new ContractValidateException("Account is not exists");
                }

                long now = this.db_manager.WitnessController.GetHeadSlot();
                if (ContractCreateNewAccount(contract))
                {
                    ConsumeForCreateNewAccount(account, size, now, tx_trace);
                    continue;
                }

                if (contract.Type == ContractType.TransferAssetContract
                    && UseAssetAccountNet(contract, account, now, size))
                {
                    continue;
                }

                if (UseAccountNet(account, size, now))
                {
                    continue;
                }

                if (UseFreeNet(account, size, now))
                {
                    continue;
                }

                if (UseTransactionFee(account, size, tx_trace))
                {
                    continue;
                }

                long fee = this.db_manager.DynamicProperties.GetTransactionFee() * size;


                throw new AccountResourceInsufficientException(
                    "Account Insufficient bandwidth[" + size + "] and balance[" + fee + "] to create new account");
            }
        }

        public override void UpdateUsage(AccountCapsule account)
        {
            long now = this.db_manager.WitnessController.GetHeadSlot();
            UpdateUsage(account, now);
        }

        public bool ContractCreateNewAccount(Contract contract)
        {
            bool result = false;
            AccountCapsule to_account = null;

            switch (contract.Type)
            {
                case ContractType.AccountCreateContract:
                    {
                        result = true;
                    }
                    break;
                case ContractType.TransferContract:
                    {
                        TransferContract transfer;
                        try
                        {
                            transfer = contract.Parameter.Unpack<TransferContract>();
                        }
                        catch (System.Exception e)
                        {
                            throw new System.Exception(e.Message);
                        }
                        to_account = this.db_manager.Account.Get(transfer.ToAddress.ToByteArray());

                        result = to_account == null;

                    }
                    break;
                case ContractType.TransferAssetContract:
                    {
                        TransferAssetContract transfer_asset;
                        try
                        {
                            transfer_asset = contract.Parameter.Unpack<TransferAssetContract>();
                        }
                        catch (System.Exception e)
                        {
                            throw new System.Exception(e.Message);
                        }

                        to_account = this.db_manager.Account.Get(transfer_asset.ToAddress.ToByteArray());
                        result = to_account == null;
                    }
                    break;
                default:
                    {
                        result = false;
                    }
                    break;
            }

            return result;
        }

        public bool ConsumeBandwidthForCreateNewAccount(AccountCapsule account, long bytes, long now)
        {
            long new_bandwidth_rate = this.db_manager.DynamicProperties.GetCreateNewAccountBandwidthRate();
            long net_usage = account.NetUsage;
            long latest_consume_time = account.LatestConsumeTime;
            long net_limit = CalculateGlobalNetLimit(account);
            long new_net_usage = Increase(net_usage, 0, latest_consume_time, now);

            if (bytes * new_bandwidth_rate <= (net_limit - new_net_usage))
            {
                latest_consume_time = now;
                long latest_operation_time = this.db_manager.GetHeadBlockTimestamp();
                new_net_usage = Increase(new_net_usage, bytes * new_bandwidth_rate, latest_consume_time,
                    now);
                account.LatestConsumeTime = latest_consume_time;
                account.LatestOperationTime = latest_operation_time;
                account.NetUsage = new_net_usage;
                this.db_manager.Account.Put(account.CreateDatabaseKey(), account);

                return true;
            }

            return false;
        }

        public bool ConsumeFeeForCreateNewAccount(AccountCapsule account, TransactionTrace trace)
        {
            bool result = false;
            long fee = this.db_manager.DynamicProperties.GetCreateAccountFee();

            if (ConsumeFee(account, fee))
            {
                trace.SetNetBill(0, fee);
                this.db_manager.DynamicProperties.AddTotalCreateAccountCost(fee);
                result = true;
            }

            return result;
        }

        public long CalculateGlobalNetLimit(AccountCapsule account)
        {
            long frozen_balance = account.AllFrozenBalanceForBandwidth;
            if (frozen_balance < 1_000_000L)
            {
                return 0;
            }

            long net_weight = frozen_balance / 1_000_000L;
            long total_net_limit = this.db_manager.DynamicProperties.GetTotalNetLimit();
            long total_net_weight = this.db_manager.DynamicProperties.GetTotalNetWeight();

            if (total_net_weight == 0)
            {
                return 0;
            }

            return (long)(net_weight * ((double)total_net_limit / total_net_weight));
        }
        #endregion
    }
}
