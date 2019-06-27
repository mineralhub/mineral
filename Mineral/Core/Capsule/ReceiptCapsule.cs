using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Utils;
using Mineral.Core.Database;
using Mineral.Core.Exception;
using Protocol;
using static Protocol.Transaction.Types.Result.Types;

namespace Mineral.Core.Capsule
{
    using VMConfig = Common.Runtime.Config.VMConfig;

    public class ReceiptCapsule
    {
        #region Field
        private ResourceReceipt receipt = null;
        private long multi_sign_fee = 0;
        private SHA256Hash hash;
        #endregion


        #region Property
        public ResourceReceipt Receipt
        {
            get { return this.receipt; }
        }

        public long MultiSignFee
        {
            get { return this.multi_sign_fee; }
            set { this.multi_sign_fee = value; }
        }

        public SHA256Hash Hash
        {
            get { return this.hash; }
        }

        public long NetUsage
        {
            get { return this.receipt.NetUsage; }
            set { this.receipt.NetUsage = value; }
        }

        public long NetFee
        {
            get { return this.receipt.NetFee; }
            set { this.receipt.NetFee = value; }
        }

        public long EnergyUsage
        {
            get { return this.receipt.EnergyUsage; }
            set { this.receipt.EnergyUsage = value; }
        }

        public long EnergyFee
        {
            get { return this.receipt.EnergyFee; }
            set { this.receipt.EnergyFee = value; }
        }

        public long EnergyUsageTotal
        {
            get { return this.receipt.EnergyUsageTotal; }
            set { this.receipt.EnergyUsageTotal = value; }
        }

        public long OriginEnergyUsage
        {
            get { return this.receipt.OriginEnergyUsage; }
            set { this.receipt.OriginEnergyUsage = value; }
        }

        public contractResult Result
        {
            get { return this.receipt.Result; }
            set { this.receipt.Result = value; }
        }
        #endregion


        #region Constructor
        public ReceiptCapsule(ResourceReceipt receipt, SHA256Hash hash)
        {
            this.receipt = receipt;
            this.hash = hash;
        }

        public ReceiptCapsule(SHA256Hash hash)
        {
            this.receipt = new ResourceReceipt();
            this.hash = hash;
        }

        public ReceiptCapsule(ResourceReceipt receipt)
        {
            this.receipt = receipt;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private long GetOriginUsage(Manager manager,
                                    AccountCapsule origin,
                                    long origin_energy_limit,
                                    EnergyProcessor energy_processor,
                                    long origin_usage)
        {
            if (VMConfig.EnergyLimitHardFork)
            {
                return Math.Min(origin_usage,
                                Math.Min(energy_processor.GetAccountLeftEnergyFromFreeze(origin), origin_energy_limit));
            }

            return Math.Min(origin_usage, energy_processor.GetAccountLeftEnergyFromFreeze(origin));
        }

        private void PayEnergyBill(Manager manager,
                                   AccountCapsule account,
                                   long usage,
                                   EnergyProcessor energy_processor,
                                   long now)
        {
            long accountEnergyLeft = energy_processor.GetAccountLeftEnergyFromFreeze(account);
            if (accountEnergyLeft >= usage)
            {
                energy_processor.UseEnergy(account, usage, now);
                this.receipt.EnergyUsage = usage;
            }
            else
            {
                energy_processor.UseEnergy(account, accountEnergyLeft, now);
                long sun_energy = DefineParameter.SUN_PER_ENERGY;
                long dynamic_energy_fee = manager.DynamicProperties.GetEnergyFee();
                if (dynamic_energy_fee > 0)
                {
                    sun_energy = dynamic_energy_fee;
                }

                long energy_fee =(usage - accountEnergyLeft) * sun_energy;

                this.receipt.EnergyUsage = accountEnergyLeft;
                this.receipt.EnergyFee = energy_fee;

                long balance = account.Balance;
                if (balance < energy_fee)
                {
                    throw new BalanceInsufficientException(
                        account.CreateDatabaseKey().ToHexString() + " insufficient balance");
                }

                account.Balance = balance - energy_fee;
                manager.AdjustBalance(manager.Account.GetBlackHole().Address.ToByteArray(), energy_fee);
            }

            manager.Account.Put(account.Address.ToByteArray(), account);
        }
        #endregion


        #region External Method
        public void AddNetFee(long fee)
        {
            this.receipt.NetFee += fee;
        }

        public void PayEnergyBill(Manager manager,
                                  AccountCapsule origin,
                                  AccountCapsule caller,
                                  long percent,
                                  long origin_energy_limit,
                                  EnergyProcessor energy_processor,
                                  long now)
        {
            if (this.receipt.EnergyUsageTotal <= 0)
            {
                return;
            }

            if (caller.Address.Equals(origin.Address))
            {
                PayEnergyBill(manager, caller, this.receipt.EnergyUsageTotal, energy_processor, now);
            }
            else
            {
                long origin_usage = (this.receipt.EnergyUsageTotal * percent) / 100;
                origin_usage = GetOriginUsage(manager, origin,
                                              origin_energy_limit,
                                              energy_processor,
                                              origin_usage);

                long caller_usage = this.receipt.EnergyUsageTotal - origin_usage;
                energy_processor.UseEnergy(origin, origin_usage, now);
                this.receipt.OriginEnergyUsage = origin_usage;

                PayEnergyBill(manager, caller, caller_usage, energy_processor, now);
            }
        }

        public static ResourceReceipt CopyReceipt(ReceiptCapsule origin)
        {
            return new ResourceReceipt(origin.Receipt);
        }
        #endregion
    }
}
