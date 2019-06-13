using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Capsule;
using Mineral.Core.Config;
using static Protocol.Account.Types;

namespace Mineral.Core.Database
{
    public class EnergyProcessor : ResourceProcessor
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Constructor
        public EnergyProcessor(Manager db_manager) : base(db_manager) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override void Consume(TransactionCapsule tx, TransactionTrace tx_trace)
        {
            throw new System.Exception("Not support");
        }

        public override void UpdateUsage(AccountCapsule account)
        {
            long now = this.db_manager.WitnessController.GetHeadSlot();
            UpdateUsage(account, now);
        }

        public void UpdateUsage(AccountCapsule account, long now)
        {
            AccountResource resource = account.AccountResource;
            account.EnergyUsage = Increase(resource.EnergyUsage, 0, resource.LatestConsumeTimeForEnergy, now);
        }

        public void UpdateTotalEnergyAverageUsage()
        {
            long now = this.db_manager.WitnessController.GetHeadSlot();
            long energy_average_usage = Increase(this.db_manager.DynamicProperties.GetTotalEnergyAverageUsage(),
                                                 this.db_manager.DynamicProperties.GetBlockEnergyUsage(),
                                                 this.db_manager.DynamicProperties.GetTotalEnergyAverageTime(),
                                                 now,
                                                 this.average_window_size);

            this.db_manager.DynamicProperties.PutTotalEnergyAverageUsage(energy_average_usage);
            this.db_manager.DynamicProperties.PutTotalEnergyAverageTime(now);
        }

        public void UpdateAdaptiveTotalEnergyLimit()
        {
            long total_energy_average_usage = this.db_manager.DynamicProperties.GetTotalEnergyAverageUsage();
            long target_total_energy_limit = this.db_manager.DynamicProperties.GetTotalEnergyTargetLimit();
            long total_energy_current_limit = this.db_manager.DynamicProperties.GetTotalEnergyCurrentLimit();
            long total_energy_limit = this.db_manager.DynamicProperties.GetTotalEnergyLimit();

            long result = 0;
            if (total_energy_average_usage > target_total_energy_limit)
            {
                result = total_energy_current_limit * Parameter.AdaptiveResourceLimitParameters.CONTRACT_RATE_NUMERATOR
                        / Parameter.AdaptiveResourceLimitParameters..CONTRACT_RATE_DENOMINATOR;
            }
            else
            {
                result = total_energy_current_limit * Parameter.AdaptiveResourceLimitParameters..EXPAND_RATE_NUMERATOR
                    / Parameter.AdaptiveResourceLimitParameters..EXPAND_RATE_DENOMINATOR;
            }

            result = Math.Min(
                Math.Max(result, total_energy_limit),
                total_energy_limit * Parameter.AdaptiveResourceLimitParameters.LIMIT_MULTIPLIER
            );

            this.db_manager.DynamicProperties.PutTotalEnergyCurrentLimit(result);
            Logger.Debug("adjust totalEnergyCurrentLimit, old[" + total_energy_current_limit + "], new[" + result+ "]");
        }

        public long CalculateGlobalEnergyLimit(AccountCapsule account)
        {
            long frozeBalance = account.AllFrozenBalanceForEnergy;
            if (frozeBalance < 1_000_000L)
                return 0;

            long energy_weight = frozeBalance / 1_000_000L;
            long total_energy_limit = this.db_manager.DynamicProperties.GetTotalEnergyCurrentLimit();
            long total_energy_weight = this.db_manager.DynamicProperties.GetTotalEnergyWeight();

            return (long)(energy_weight * ((double)total_energy_limit / total_energy_weight));
        }

        public bool UseEnergy(AccountCapsule account, long energy, long now)
        {
            long energy_usage = account.EnergyUsage;
            long latest_consume_time = account.AccountResource.LatestConsumeTimeForEnergy;
            long energy_limit = CalculateGlobalEnergyLimit(account);

            long new_energy_usage = Increase(energy_usage, 0, latest_consume_time, now);

            if (energy > (energy_limit - new_energy_usage))
            {
                return false;
            }

            latest_consume_time = now;
            long latest_operation_time = this.db_manager.GetHeadBlockTimestamp();
            new_energy_usage = Increase(new_energy_usage, energy, latest_consume_time, now);
            account.EnergyUsage = new_energy_usage;
            account.LatestOperationTime = latest_operation_time;
            account.LatestConsumeTimeForEnergy = latest_consume_time;

            this.db_manager.Account.Put(account.CreateDatabaseKey(), account);

            if (this.db_manager.DynamicProperties.GetAllowAdaptiveEnergy() == 1)
            {
                long blockEnergyUsage = this.db_manager.DynamicProperties.GetBlockEnergyUsage() + energy;
                this.db_manager.DynamicProperties.PutBlockEnergyUsage(blockEnergyUsage);
            }

            return true;
        }

        public long GetAccountLeftEnergyFromFreeze(AccountCapsule account)
        {
            long now = this.db_manager.WitnessController.GetHeadSlot();
            long energy_usage = account.EnergyUsage;
            long latest_consume_time = account.AccountResource.LatestConsumeTimeForEnergy;
            long energy_limit = CalculateGlobalEnergyLimit(account);
            long newEnergyUsage = Increase(energy_usage, 0, latest_consume_time, now);

            return Math.Max(energy_limit - newEnergyUsage, 0);
        }
        #endregion
    }
}
