using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Google.Protobuf;
using Mineral.Common.Utils;
using Mineral.Core.Capsule;
using Mineral.Core.Config;
using Mineral.Core.Config.Arguments;
using Mineral.Utils;

namespace Mineral.Core.Database
{
    public partial class DynamicPropertiesStore : MineralStoreWithRevoking<BytesCapsule, object>
    {
        public DynamicPropertiesStore(string db_name = "properties")
            : base (db_name)
        {
            try { GetTotalSignNum(); } catch { PutTotalSignNum(5); }
            try { GetAllowMultiSign(); } catch { PutAllowMultiSign((int)Args.Instance.Committe.AllowMultiSign); }
            try { GetLatestBlockHeaderTimestamp(); } catch { PutLatestBlockHeaderTimestamp(0); }
            try { GetLatestBlockHeaderNumber(); } catch { PutLatestBlockHeaderNumber(0); }
            try { GetLatestBlockHeaderHash(); } catch { PutLatestBlockHeaderHash(ByteString.CopyFrom("00".HexToBytes())); }
            try { GetStateFlag(); } catch { PutStateFlag(0); }
            try { GetLatestSolidifiedBlockNum(); } catch { PutLatestSolidifiedBlockNum(0); }
            try { GetLatestProposalNum(); } catch { PutLatestProposalNum(0); }
            try { GetLatestExchangeNum(); } catch { PutLatestExchangeNum(0); }
            try { GetBlockFilledSlotsIndex(); } catch { PutBlockFilledSlotsIndex(0); }
            try { GetTokenIdNum(); } catch { PutTokenIdNum(1000000L); }
            try { GetTokenUpdateDone(); } catch { PutTokenUpdateDone(0); }
            try { GetMaxFrozenTime(); } catch { PutMaxFrozenTime(3); }
            try { GetMinFrozenTime(); } catch { PutMinFrozenTime(3); }
            try { GetMaxFrozenSupplyNumber(); } catch { PutMaxFrozenSupplyNumber(10); }
            try { GetMaxFrozenSupplyTime(); } catch { PutMaxFrozenSupplyTime(3652); }
            try { GetMinFrozenSupplyTime(); } catch { PutMinFrozenSupplyTime(1); }
            try { GetWitnessAllowanceFrozenTime(); } catch { PutWitnessAllowanceFrozenTime(1); }
            try { GetWitnessPayPerBlock(); } catch { PutWitnessPayPerBlock(32000000L); }
            try { GetWitnessStandbyAllowance(); } catch { PutWitnessStandbyAllowance(115_200_000_000L); }
            try { GetMaintenanceTimeInterval(); } catch { PutMaintenanceTimeInterval((int)Args.Instance.Block.MaintenanceTimeInterval); }
            try { GetAccountUpgradeCost(); } catch { PutAccountUpgradeCost(9_999_000_000L); }
            try { GetPublicNetUsage(); } catch { PutPublicNetUsage(0L); }
            try { GetOneDayNetLimit(); } catch { PutOneDayNetLimit(57_600_000_000L); }
            try { GetPublicNetLimit(); } catch { PutPublicNetLimit(14_400_000_000L); }
            try { GetPublicNetTime(); } catch { PutPublicNetTime(0L); }
            try { GetFreeNetLimit(); } catch { PutFreeNetLimit(5000L); }
            try { GetTotalNetWeight(); } catch { PutTotalNetWeight(0L); }
            try { GetTotalNetLimit(); } catch { PutTotalNetLimit(43_200_000_000L); }
            try { GetTotalEnergyWeight(); } catch { PutTotalEnergyWeight(0L); }
            try { GetAllowAdaptiveEnergy(); } catch { PutAllowAdaptiveEnergy((int)Args.Instance.Committe.AllowAdaptiveEnergy); }
            try { GetTotalEnergyLimit(); } catch { PutTotalEnergyLimit(50_000_000_000L); }
            try { GetEnergyFee(); } catch { PutEnergyFee(100L); }
            try { GetMaxCpuTimeOfOneTx(); } catch { PutMaxCpuTimeOfOneTx(50L); }
            try { GetCreateAccountFee(); } catch { PutCreateAccountFee(100_000L); }
            try { GetCreateNewAccountFeeInSystemContract(); } catch { PutCreateNewAccountFeeInSystemContract(0L); }
            try { GetCreateNewAccountBandwidthRate(); } catch { PutCreateNewAccountBandwidthRate(1L); }
            try { GetTransactionFee(); } catch { PutTransactionFee(10L); }
            try { GetAssetIssueFee(); } catch { PutAssetIssueFee(1024000000L); }
            try { GetUpdateAccountPermissionFee(); } catch { PutUpdateAccountPermissionFee(100000000L); }
            try { GetMultiSignFee(); } catch { PutMultiSignFee(1000000L); }
            try { GetExchangeCreateFee(); } catch { PutExchangeCreateFee(1024000000L); }
            try { GetExchangeBalanceLimit(); } catch { PutExchangeBalanceLimit(1_000_000_000_000_000L); }
            try { GetTotalTransactionCost(); } catch { PutTotalTransactionCost(0L); }
            try { GetTotalCreateWitnessCost(); } catch { PutTotalCreateWitnessFee(0L); }
            try { GetTotalCreateAccountCost(); } catch { PutTotalCreateAccountFee(0L); }
            try { GetTotalStoragePool(); } catch { PutTotalStoragePool(100_000_000_000_000L); }
            try { GetTotalStorageTax(); } catch { PutTotalStorageTax(0); }
            try { GetTotalStorageReserved(); } catch { PutTotalStorageReserved(128L * 1024 * 1024 * 1024); }
            try { GetStorageExchangeTaxRate(); } catch { PutStorageExchangeTaxRate(10); }
            try { GetRemoveThePowerOfTheGr(); } catch { PutRemoveThePowerOfTheGr(0); }
            try { GetAllowDelegateResource(); } catch { PutAllowDelegateResource((int)Args.Instance.Committe.AllowDelegateResource); }
            try { GetAllowTvmTransferTrc10(); } catch { PutAllowTvmTransferTrc10((int)Args.Instance.Committe.AllowVMTransferTC10); }
            try { GetAllowTvmConstantinople(); } catch { PutAllowTvmConstantinople((int)Args.Instance.Committe.AllowVMConstantinople); }
            try { GetAvailableContractType(); } catch { PutAvailableContractType("7fff1fc0037e0000000000000000000000000000000000000000000000000000".HexToBytes()); }
            try { GetActiveDefaultOperations(); } catch { PutActiveDefaultOperations("7fff1fc0033e0000000000000000000000000000000000000000000000000000".HexToBytes()); }
            try { GetAllowSameTokenName(); } catch { PutAllowSameTokenName((int)Args.Instance.Committe.AllowSameTokenName); }
            try { GetAllowUpdateAccountName(); } catch { PutAllowUpdateAccountName(0); }
            try { GetAllowCreationOfContracts(); } catch { PutAllowCreationOfContracts((int)Args.Instance.Committe.AllowCreationOfContracts); }
            try
            {
                GetBlockFilledSlots();
            }
            catch
            {
                int[] block_filled_slots = Enumerable.Repeat(1, GetBlockFilledSlotsNumber()).ToArray();
                PutBlockFilledSlots(block_filled_slots);
            }
            try { GetNextMaintenanceTime(); } catch { PutNextMaintenanceTime((long)Args.Instance.Genesisblock.Timestamp); }
            try { GetTotalEnergyCurrentLimit(); } catch { PutTotalEnergyCurrentLimit(GetTotalEnergyLimit()); }
            try { GetTotalEnergyTargetLimit(); } catch { PutTotalEnergyTargetLimit(GetTotalEnergyLimit() / 14400); }
            try { GetTotalEnergyAverageUsage(); } catch { PutTotalEnergyAverageUsage(0); }
            try { GetTotalEnergyAverageTime(); } catch { PutTotalEnergyAverageTime(0); }
            try { GetBlockEnergyUsage(); } catch { PutBlockEnergyUsage(0); }
            try { GetAllowAccountStateRoot(); } catch { PutAllowAccountStateRoot((int)Args.Instance.Committe.AllowAccountStateRoot); }
            try { GetAllowProtoFilterNum(); } catch { PutAllowProtoFilterNum((int)Args.Instance.Committe.AllowProtoFilterNum); }
        }

        public void PutTokenIdNum(long num)
        {
            Put(TOKEN_ID_NUM, new BytesCapsule(BitConverter.GetBytes(num)));
        }

        public long GetTokenIdNum()
        {
            return BitConverter.ToInt64(GetUnchecked(TOKEN_ID_NUM).Data, 0);
        }

        public void PutTokenUpdateDone(long num)
        {
            Put(TOKEN_UPDATE_DONE, new BytesCapsule(BitConverter.GetBytes(num)));
        }

        public long GetTokenUpdateDone()
        {
            return BitConverter.ToInt64(GetUnchecked(TOKEN_UPDATE_DONE).Data, 0);
        }

        public void PutBlockFilledSlotsIndex(int block_fiiled_slots_index)
        {
            Logger.Debug("block_fiiled_slots_index : " + block_fiiled_slots_index);
            Put(BLOCK_FILLED_SLOTS_INDEX, new BytesCapsule(BitConverter.GetBytes(block_fiiled_slots_index)));
        }

        public int GetBlockFilledSlotsIndex()
        {
            return BitConverter.ToInt32(GetUnchecked(BLOCK_FILLED_SLOTS_INDEX).Data, 0);
        }

        public void PutMaxFrozenTime(int max_frozen_time)
        {
            Logger.Debug("MAX_FROZEN_TIME : " + max_frozen_time);
            Put(MAX_FROZEN_TIME, new BytesCapsule(BitConverter.GetBytes(max_frozen_time)));
        }

        public int GetMaxFrozenTime()
        {
            return BitConverter.ToInt32(GetUnchecked(MAX_FROZEN_TIME).Data, 0);
        }

        public void PutMinFrozenTime(int min_frozen_time)
        {
            Logger.Debug("MIN_FROZEN_TIME : " + min_frozen_time);
            Put(MIN_FROZEN_TIME, new BytesCapsule(BitConverter.GetBytes(min_frozen_time)));
        }

        public int GetMinFrozenTime()
        {
            return BitConverter.ToInt32(GetUnchecked(MIN_FROZEN_TIME).Data, 0);
        }

        public void PutMaxFrozenSupplyNumber(int max_frozen_supply_number)
        {
            Logger.Debug("MAX_FROZEN_SUPPLY_NUMBER : " + max_frozen_supply_number);
            Put(MAX_FROZEN_SUPPLY_NUMBER,
                new BytesCapsule(BitConverter.GetBytes(max_frozen_supply_number)));
        }

        public int GetMaxFrozenSupplyNumber()
        {
            return BitConverter.ToInt32(GetUnchecked(MAX_FROZEN_SUPPLY_NUMBER).Data, 0);
        }

        public void PutMaxFrozenSupplyTime(int max_frozen_supply_time)
        {
            Logger.Debug("MAX_FROZEN_SUPPLY_TIME : " + max_frozen_supply_time);
            Put(MAX_FROZEN_SUPPLY_TIME, new BytesCapsule(BitConverter.GetBytes(max_frozen_supply_time)));
        }

        public int GetMaxFrozenSupplyTime()
        {
            return BitConverter.ToInt32(GetUnchecked(MAX_FROZEN_SUPPLY_TIME).Data, 0);
        }

        public void PutMinFrozenSupplyTime(int min_frozen_supply_time)
        {
            Logger.Debug("MIN_FROZEN_SUPPLY_TIME : " + min_frozen_supply_time);
            Put(MIN_FROZEN_SUPPLY_TIME, new BytesCapsule(BitConverter.GetBytes(min_frozen_supply_time)));
        }

        public int GetMinFrozenSupplyTime()
        {
            return BitConverter.ToInt32(GetUnchecked(MIN_FROZEN_SUPPLY_TIME).Data, 0);
        }

        public void PutWitnessAllowanceFrozenTime(int witness_allowance_frozen_time)
        {
            Logger.Debug("WITNESS_ALLOWANCE_FROZEN_TIME : " + witness_allowance_frozen_time);
            Put(WITNESS_ALLOWANCE_FROZEN_TIME, new BytesCapsule(BitConverter.GetBytes(witness_allowance_frozen_time)));
        }

        public int GetWitnessAllowanceFrozenTime()
        {
            return BitConverter.ToInt32(GetUnchecked(WITNESS_ALLOWANCE_FROZEN_TIME).Data, 0);
        }

        public void PutMaintenanceTimeInterval(int time_interval)
        {
            Logger.Debug("MAINTENANCE_TIME_INTERVAL : " + time_interval);
            Put(MAINTENANCE_TIME_INTERVAL, new BytesCapsule(BitConverter.GetBytes(time_interval)));
        }

        public int GetMaintenanceTimeInterval()
        {
            return BitConverter.ToInt32(GetUnchecked(MAINTENANCE_TIME_INTERVAL).Data, 0);
        }

        public void PutAccountUpgradeCost(long account_upgrade_cost)
        {
            Logger.Debug("ACCOUNT_UPGRADE_COST : " + account_upgrade_cost);
            Put(ACCOUNT_UPGRADE_COST, new BytesCapsule(BitConverter.GetBytes(account_upgrade_cost)));
        }

        public long GetAccountUpgradeCost()
        {
            return BitConverter.ToInt64(GetUnchecked(ACCOUNT_UPGRADE_COST).Data, 0);
        }

        public void PutWitnessPayPerBlock(long pay)
        {
            Logger.Debug("WITNESS_PAY_PER_BLOCK : " + pay);
            Put(WITNESS_PAY_PER_BLOCK, new BytesCapsule(BitConverter.GetBytes(pay)));
        }

        public long GetWitnessPayPerBlock()
        {
            return BitConverter.ToInt64(GetUnchecked(WITNESS_PAY_PER_BLOCK).Data, 0);
        }

        public void PutWitnessStandbyAllowance(long allowance)
        {
            Logger.Debug("WITNESS_STANDBY_ALLOWANCE : " + allowance);
            Put(WITNESS_STANDBY_ALLOWANCE, new BytesCapsule(BitConverter.GetBytes(allowance)));
        }

        public long GetWitnessStandbyAllowance()
        {
            return BitConverter.ToInt64(GetUnchecked(WITNESS_STANDBY_ALLOWANCE).Data, 0);
        }

        public void PutOneDayNetLimit(long one_day_net_limit)
        {
            Put(DynamicResourceProperties.ONE_DAY_NET_LIMIT, new BytesCapsule(BitConverter.GetBytes(one_day_net_limit)));
        }

        public long GetOneDayNetLimit()
        {
            return BitConverter.ToInt64(GetUnchecked(DynamicResourceProperties.ONE_DAY_NET_LIMIT).Data, 0);
        }

        public void PutPublicNetUsage(long public_net_usage)
        {
            Put(DynamicResourceProperties.PUBLIC_NET_USAGE, new BytesCapsule(BitConverter.GetBytes(public_net_usage)));
        }

        public long GetPublicNetUsage()
        {
            return BitConverter.ToInt64(GetUnchecked(DynamicResourceProperties.PUBLIC_NET_USAGE).Data, 0);
        }

        public void PutPublicNetLimit(long public_net_limit)
        {
            Put(DynamicResourceProperties.PUBLIC_NET_LIMIT, new BytesCapsule(BitConverter.GetBytes(public_net_limit)));
        }

        public long GetPublicNetLimit()
        {
            return BitConverter.ToInt64(GetUnchecked(DynamicResourceProperties.PUBLIC_NET_LIMIT).Data, 0);
        }

        public void PutPublicNetTime(long public_net_time)
        {
            Put(DynamicResourceProperties.PUBLIC_NET_TIME, new BytesCapsule(BitConverter.GetBytes(public_net_time)));
        }

        public long GetPublicNetTime()
        {
            return BitConverter.ToInt64(GetUnchecked(DynamicResourceProperties.PUBLIC_NET_TIME).Data, 0);
        }

        public void PutFreeNetLimit(long free_net_limit)
        {
            Put(DynamicResourceProperties.FREE_NET_LIMIT, new BytesCapsule(BitConverter.GetBytes(free_net_limit)));
        }

        public long GetFreeNetLimit()
        {
            return BitConverter.ToInt64(GetUnchecked(DynamicResourceProperties.FREE_NET_LIMIT).Data, 0);
        }

        public void PutTotalNetWeight(long total_net_weight)
        {
            Put(DynamicResourceProperties.TOTAL_NET_WEIGHT, new BytesCapsule(BitConverter.GetBytes(total_net_weight)));
        }

        public long GetTotalNetWeight()
        {
            return BitConverter.ToInt64(GetUnchecked(DynamicResourceProperties.TOTAL_NET_WEIGHT).Data, 0);
        }

        public void PutTotalEnergyWeight(long totalEnergyWeight)
        {
            Put(DynamicResourceProperties.TOTAL_ENERGY_WEIGHT, new BytesCapsule(BitConverter.GetBytes(totalEnergyWeight)));
        }

        public long GetTotalEnergyWeight()
        {
            return BitConverter.ToInt64(GetUnchecked(DynamicResourceProperties.TOTAL_ENERGY_WEIGHT).Data, 0);
        }

        public void PutTotalNetLimit(long total_net_limit)
        {
            Put(DynamicResourceProperties.TOTAL_NET_LIMIT, new BytesCapsule(BitConverter.GetBytes(total_net_limit)));
        }

        public long GetTotalNetLimit()
        {
            return BitConverter.ToInt64(GetUnchecked(DynamicResourceProperties.TOTAL_NET_LIMIT).Data, 0);
        }

        public void PutTotalEnergyLimit(long total_energy_limit)
        {
            Put(DynamicResourceProperties.TOTAL_ENERGY_LIMIT, new BytesCapsule(BitConverter.GetBytes(total_energy_limit)));
            PutTotalEnergyTargetLimit(total_energy_limit / 14400);
        }

        public void PutTotalEnergyLimit2(long total_energy_limit)
        {
            Put(DynamicResourceProperties.TOTAL_ENERGY_LIMIT, new BytesCapsule(BitConverter.GetBytes(total_energy_limit)));

            PutTotalEnergyTargetLimit(total_energy_limit / 14400);
            if (GetAllowAdaptiveEnergy() == 0)
            {
                PutTotalEnergyCurrentLimit(total_energy_limit);
            }
        }

        public long GetTotalEnergyLimit()
        {
            try
            {
                return BitConverter.ToInt64(GetUnchecked(DynamicResourceProperties.TOTAL_ENERGY_LIMIT).Data, 0);
            }
            catch (System.Exception e)
            {
                throw e;
            }
        }

        public void PutTotalEnergyCurrentLimit(long total_energy_current_limit)
        {
            Put(DynamicResourceProperties.TOTAL_ENERGY_CURRENT_LIMIT, new BytesCapsule(BitConverter.GetBytes(total_energy_current_limit)));
        }

        public long GetTotalEnergyCurrentLimit()
        {
            return BitConverter.ToInt64(GetUnchecked(DynamicResourceProperties.TOTAL_ENERGY_CURRENT_LIMIT).Data, 0);
        }

        public void PutTotalEnergyTargetLimit(long target_total_energy_limit)
        {
            Put(DynamicResourceProperties.TOTAL_ENERGY_TARGET_LIMIT, new BytesCapsule(BitConverter.GetBytes(target_total_energy_limit)));
        }

        public long GetTotalEnergyTargetLimit()
        {
            return BitConverter.ToInt64(GetUnchecked(DynamicResourceProperties.TOTAL_ENERGY_TARGET_LIMIT).Data, 0);
        }

        public void PutTotalEnergyAverageUsage(long total_Energy_Average_Usage)
        {
            Put(DynamicResourceProperties.TOTAL_ENERGY_AVERAGE_USAGE, new BytesCapsule(BitConverter.GetBytes(total_Energy_Average_Usage)));
        }

        public long GetTotalEnergyAverageUsage()
        {
            return BitConverter.ToInt64(GetUnchecked(DynamicResourceProperties.TOTAL_ENERGY_AVERAGE_USAGE).Data, 0);
        }

        public void PutTotalEnergyAverageTime(long total_energy_average_time)
        {
            Put(DynamicResourceProperties.TOTAL_ENERGY_AVERAGE_TIME, new BytesCapsule(BitConverter.GetBytes(total_energy_average_time)));
        }

        public long GetTotalEnergyAverageTime()
        {
            return BitConverter.ToInt64(GetUnchecked(DynamicResourceProperties.TOTAL_ENERGY_AVERAGE_TIME).Data, 0);
        }

        public void PutBlockEnergyUsage(long block_energy_usage)
        {
            Put(DynamicResourceProperties.BLOCK_ENERGY_USAGE, new BytesCapsule(BitConverter.GetBytes(block_energy_usage)));
        }

        public long GetBlockEnergyUsage()
        {
            return BitConverter.ToInt64(GetUnchecked(DynamicResourceProperties.BLOCK_ENERGY_USAGE).Data, 0);
        }

        public void PutEnergyFee(long total_energy_fee)
        {
            Put(ENERGY_FEE, new BytesCapsule(BitConverter.GetBytes(total_energy_fee)));
        }

        public long GetEnergyFee()
        {
            return BitConverter.ToInt64(GetUnchecked(ENERGY_FEE).Data, 0);
        }

        public void PutMaxCpuTimeOfOneTx(long time)
        {
            Put(MAX_CPU_TIME_OF_ONE_TX, new BytesCapsule(BitConverter.GetBytes(time)));
        }

        public long GetMaxCpuTimeOfOneTx()
        {
            return BitConverter.ToInt64(GetUnchecked(MAX_CPU_TIME_OF_ONE_TX).Data, 0);
        }

        public void PutCreateAccountFee(long fee)
        {
            Put(CREATE_ACCOUNT_FEE, new BytesCapsule(BitConverter.GetBytes(fee)));
        }

        public long GetCreateAccountFee()
        {
            return BitConverter.ToInt64(GetUnchecked(CREATE_ACCOUNT_FEE).Data, 0);
        }

        public void PutCreateNewAccountFeeInSystemContract(long fee)
        {
            Put(CREATE_NEW_ACCOUNT_FEE_IN_SYSTEM_CONTRACT, new BytesCapsule(BitConverter.GetBytes(fee)));
        }

        public long GetCreateNewAccountFeeInSystemContract()
        {
            return BitConverter.ToInt64(GetUnchecked(CREATE_NEW_ACCOUNT_FEE_IN_SYSTEM_CONTRACT).Data, 0);
        }

        public void PutCreateNewAccountBandwidthRate(long rate)
        {
            Put(CREATE_NEW_ACCOUNT_BANDWIDTH_RATE, new BytesCapsule(BitConverter.GetBytes(rate)));
        }

        public long GetCreateNewAccountBandwidthRate()
        {
            return BitConverter.ToInt64(GetUnchecked(CREATE_NEW_ACCOUNT_BANDWIDTH_RATE).Data, 0);
        }

        public void PutTransactionFee(long fee)
        {
            Put(TRANSACTION_FEE, new BytesCapsule(BitConverter.GetBytes(fee)));
        }

        public long GetTransactionFee()
        {
            return BitConverter.ToInt64(GetUnchecked(TRANSACTION_FEE).Data, 0);
        }

        public void PutAssetIssueFee(long fee)
        {
            Put(ASSET_ISSUE_FEE, new BytesCapsule(BitConverter.GetBytes(fee)));
        }

        public void PutUpdateAccountPermissionFee(long fee)
        {
            Put(UPDATE_ACCOUNT_PERMISSION_FEE, new BytesCapsule(BitConverter.GetBytes(fee)));
        }

        public void PutMultiSignFee(long fee)
        {
            Put(MULTI_SIGN_FEE, new BytesCapsule(BitConverter.GetBytes(fee)));
        }

        public long GetAssetIssueFee()
        {
            return BitConverter.ToInt64(GetUnchecked(ASSET_ISSUE_FEE).Data, 0);
        }

        public long GetUpdateAccountPermissionFee()
        {
            return BitConverter.ToInt64(GetUnchecked(UPDATE_ACCOUNT_PERMISSION_FEE).Data, 0);
        }

        public long GetMultiSignFee()
        {
            return BitConverter.ToInt64(GetUnchecked(MULTI_SIGN_FEE).Data, 0);
        }

        public void PutExchangeCreateFee(long fee)
        {
            Put(EXCHANGE_CREATE_FEE, new BytesCapsule(BitConverter.GetBytes(fee)));
        }

        public long GetExchangeCreateFee()
        {
            return BitConverter.ToInt64(GetUnchecked(EXCHANGE_CREATE_FEE).Data, 0);
        }

        public void PutExchangeBalanceLimit(long limit)
        {
            Put(EXCHANGE_BALANCE_LIMIT, new BytesCapsule(BitConverter.GetBytes(limit)));
        }

        public long GetExchangeBalanceLimit()
        {
            return BitConverter.ToInt64(GetUnchecked(EXCHANGE_BALANCE_LIMIT).Data, 0);
        }

        public void PutTotalTransactionCost(long value)
        {
            Put(TOTAL_TRANSACTION_COST, new BytesCapsule(BitConverter.GetBytes(value)));
        }

        public long GetTotalTransactionCost()
        {
            return BitConverter.ToInt64(GetUnchecked(TOTAL_TRANSACTION_COST).Data, 0);
        }

        public void PutTotalCreateAccountFee(long value)
        {
            Put(TOTAL_CREATE_ACCOUNT_COST, new BytesCapsule(BitConverter.GetBytes(value)));
        }

        public long GetTotalCreateAccountCost()
        {
            return BitConverter.ToInt64(GetUnchecked(TOTAL_CREATE_ACCOUNT_COST).Data, 0);
        }

        public void PutTotalCreateWitnessFee(long value)
        {
            Put(TOTAL_CREATE_WITNESS_COST, new BytesCapsule(BitConverter.GetBytes(value)));
        }

        public long GetTotalCreateWitnessCost()
        {
            return BitConverter.ToInt64(GetUnchecked(TOTAL_CREATE_WITNESS_COST).Data, 0);
        }

        public void PutTotalStoragePool(long value)
        {
            Put(TOTAL_STORAGE_POOL, new BytesCapsule(BitConverter.GetBytes(value)));
        }

        public long GetTotalStoragePool()
        {
            return BitConverter.ToInt64(GetUnchecked(TOTAL_STORAGE_POOL).Data, 0);
        }

        public void PutTotalStorageTax(long value)
        {
            Put(TOTAL_STORAGE_TAX, new BytesCapsule(BitConverter.GetBytes(value)));
        }

        public long GetTotalStorageTax()
        {
            return BitConverter.ToInt64(GetUnchecked(TOTAL_STORAGE_TAX).Data, 0);
        }

        public void PutTotalStorageReserved(long bytes)
        {
            Put(TOTAL_STORAGE_RESERVED, new BytesCapsule(BitConverter.GetBytes(bytes)));
        }

        public long GetTotalStorageReserved()
        {
            return BitConverter.ToInt64(GetUnchecked(TOTAL_STORAGE_RESERVED).Data, 0);
        }

        public void PutStorageExchangeTaxRate(long rate)
        {
            Put(STORAGE_EXCHANGE_TAX_RATE, new BytesCapsule(BitConverter.GetBytes(rate)));
        }

        public long GetStorageExchangeTaxRate()
        {
            return BitConverter.ToInt64(GetUnchecked(STORAGE_EXCHANGE_TAX_RATE).Data, 0);
        }

        public void PutRemoveThePowerOfTheGr(long rate)
        {
            Put(REMOVE_THE_POWER_OF_THE_GR, new BytesCapsule(BitConverter.GetBytes(rate)));
        }

        public long GetRemoveThePowerOfTheGr()
        {
            return BitConverter.ToInt64(GetUnchecked(REMOVE_THE_POWER_OF_THE_GR).Data, 0);
        }

        public void PutAllowDelegateResource(int value)
        {
            Put(ALLOW_DELEGATE_RESOURCE, new BytesCapsule(BitConverter.GetBytes(value)));
        }

        public int GetAllowDelegateResource()
        {
            return BitConverter.ToInt32(GetUnchecked(ALLOW_DELEGATE_RESOURCE).Data, 0);
        }

        public void PutAllowAdaptiveEnergy(int value)
        {
            Put(ALLOW_ADAPTIVE_ENERGY, new BytesCapsule(BitConverter.GetBytes(value)));
        }

        public int GetAllowAdaptiveEnergy()
        {
            return BitConverter.ToInt32(GetUnchecked(ALLOW_ADAPTIVE_ENERGY).Data, 0);
        }

        public void PutAllowTvmTransferTrc10(int value)
        {
            Put(ALLOW_TVM_TRANSFER_TRC10, new BytesCapsule(BitConverter.GetBytes(value)));
        }

        public int GetAllowTvmTransferTrc10()
        {
            return BitConverter.ToInt32(GetUnchecked(ALLOW_TVM_TRANSFER_TRC10).Data, 0);
        }

        public void PutAllowTvmConstantinople(int value)
        {
            Put(ALLOW_TVM_CONSTANTINOPLE, new BytesCapsule(BitConverter.GetBytes(value)));
        }

        public int GetAllowTvmConstantinople()
        {
            return BitConverter.ToInt32(GetUnchecked(ALLOW_TVM_CONSTANTINOPLE).Data, 0);
        }

        public void PutAvailableContractType(byte[] value)
        {
            Put(AVAILABLE_CONTRACT_TYPE, new BytesCapsule(value));
        }

        public byte[] GetAvailableContractType()
        {
            return GetUnchecked(AVAILABLE_CONTRACT_TYPE).Data;
        }

        public void PutActiveDefaultOperations(byte[] value)
        {
            Put(ACTIVE_DEFAULT_OPERATIONS,
                    new BytesCapsule(value));
        }

        public byte[] GetActiveDefaultOperations()
        {
            return GetUnchecked(ACTIVE_DEFAULT_OPERATIONS).Data;
        }

        public void PutAllowUpdateAccountName(long rate)
        {
            Put(ALLOW_UPDATE_ACCOUNT_NAME, new BytesCapsule(BitConverter.GetBytes(rate)));
        }

        public long GetAllowUpdateAccountName()
        {
            return BitConverter.ToInt64(GetUnchecked(ALLOW_UPDATE_ACCOUNT_NAME).Data, 0);
        }

        public void PutAllowSameTokenName(int rate)
        {
            Put(ALLOW_SAME_TOKEN_NAME, new BytesCapsule(BitConverter.GetBytes(rate)));
        }

        public int GetAllowSameTokenName()
        {
            return BitConverter.ToInt32(GetUnchecked(ALLOW_SAME_TOKEN_NAME).Data, 0);
        }

        public void PutAllowCreationOfContracts(int allow_creation_of_contracts)
        {
            Put(ALLOW_CREATION_OF_CONTRACTS, new BytesCapsule(BitConverter.GetBytes(allow_creation_of_contracts)));
        }

        public long GetAllowCreationOfContracts()
        {
            return BitConverter.ToInt32(GetUnchecked(ALLOW_CREATION_OF_CONTRACTS).Data, 0);
        }

        public void PutTotalSignNum(int num)
        {
            Put(DynamicPropertiesStore.TOTAL_SIGN_NUM, new BytesCapsule(BitConverter.GetBytes(num)));
        }

        public int GetTotalSignNum()
        {
            return BitConverter.ToInt32(GetUnchecked(TOTAL_SIGN_NUM).Data, 0);
        }

        public void PutAllowMultiSign(int allow_multi_sing)
        {
            Put(ALLOW_MULTI_SIGN, new BytesCapsule(BitConverter.GetBytes(allow_multi_sing)));
        }

        public int GetAllowMultiSign()
        {
            return BitConverter.ToInt32(GetUnchecked(ALLOW_MULTI_SIGN).Data, 0);
        }

        public void PutBlockFilledSlots(int[] block_filled_slots)
        {
            Logger.Debug("blockFilledSlots:" + block_filled_slots.IntToString());
            Put(BLOCK_FILLED_SLOTS, new BytesCapsule(block_filled_slots.IntToString().ToBytes()));
        }

        public int[] GetBlockFilledSlots()
        {
            return base.GetUnchecked(BLOCK_FILLED_SLOTS).Data.BytesToString().ToIntArray();
        }

        public int GetBlockFilledSlotsNumber()
        {
            return Parameter.ChainParameters.BLOCK_FILLED_SLOTS_NUMBER;
        }

        public void PutLatestSolidifiedBlockNum(long number)
        {
            Put(LATEST_SOLIDIFIED_BLOCK_NUM, new BytesCapsule(BitConverter.GetBytes(number)));
        }

        public long GetLatestSolidifiedBlockNum()
        {
            return BitConverter.ToInt64(GetUnchecked(LATEST_SOLIDIFIED_BLOCK_NUM).Data, 0);
        }

        public void PutLatestProposalNum(long number)
        {
            Put(LATEST_PROPOSAL_NUM, new BytesCapsule(BitConverter.GetBytes(number)));
        }

        public long GetLatestProposalNum()
        {
            return BitConverter.ToInt64(GetUnchecked(LATEST_PROPOSAL_NUM).Data, 0);
        }

        public void PutLatestExchangeNum(long number)
        {
            Put(LATEST_EXCHANGE_NUM, new BytesCapsule(BitConverter.GetBytes(number)));
        }

        public long GetLatestExchangeNum()
        {
            return BitConverter.ToInt64(GetUnchecked(LATEST_EXCHANGE_NUM).Data, 0);
        }

        public long GetLatestBlockHeaderTimestamp()
        {
            return BitConverter.ToInt64(GetUnchecked(LATEST_BLOCK_HEADER_TIMESTAMP).Data, 0);
        }

        public long GetLatestBlockHeaderNumber()
        {
            return BitConverter.ToInt64(GetUnchecked(LATEST_BLOCK_HEADER_NUMBER).Data, 0);
        }

        public int GetStateFlag()
        {
            return BitConverter.ToInt32(GetUnchecked(STATE_FLAG).Data, 0);
        }

        public SHA256Hash GetLatestBlockHeaderHash()
        {
            return SHA256Hash.Wrap(GetUnchecked(LATEST_BLOCK_HEADER_HASH).Data);
        }

        public void PutLatestBlockHeaderTimestamp(long t)
        {
            Logger.Info("update latest block header timestamp : " +  t);
            Put(LATEST_BLOCK_HEADER_TIMESTAMP, new BytesCapsule(BitConverter.GetBytes(t)));
        }

        public void PutLatestBlockHeaderNumber(long n)
        {
            Logger.Info("update latest block header number : " + n);
            Put(LATEST_BLOCK_HEADER_NUMBER, new BytesCapsule(BitConverter.GetBytes(n)));
        }

        public void PutLatestBlockHeaderHash(ByteString h)
        {
            Logger.Info("update latest block header id : " + h.ToByteArray().ToHexString());
            Put(LATEST_BLOCK_HEADER_HASH, new BytesCapsule(h.ToByteArray()));
        }

        public void PutStateFlag(int n)
        {
            Logger.Info("update state flag : " + n);
            Put(STATE_FLAG, new BytesCapsule(BitConverter.GetBytes(n)));
        }

        public long GetNextMaintenanceTime()
        {
            return BitConverter.ToInt64(GetUnchecked(NEXT_MAINTENANCE_TIME).Data, 0);
        }

        public long GetMaintenanceSkipSlots()
        {
            return Parameter.ChainParameters.MAINTENANCE_SKIP_SLOTS;
        }

        public void PutNextMaintenanceTime(long next_maintenance_time)
        {
            Put(NEXT_MAINTENANCE_TIME, new BytesCapsule(BitConverter.GetBytes(next_maintenance_time)));
        }

        public void PutAllowProtoFilterNum(int num)
        {
            Logger.Info("update allow protobuf number : " +  num);
            Put(ALLOW_PROTO_FILTER_NUM, new BytesCapsule(BitConverter.GetBytes(num)));
        }

        public int GetAllowProtoFilterNum()
        {
            return BitConverter.ToInt32(GetUnchecked(ALLOW_PROTO_FILTER_NUM).Data, 0);
        }

        public void PutAllowAccountStateRoot(int allow_account_state_root)
        {
            Put(ALLOW_ACCOUNT_STATE_ROOT, new BytesCapsule(BitConverter.GetBytes(allow_account_state_root)));
        }

        public int GetAllowAccountStateRoot()
        {
            return BitConverter.ToInt32(GetUnchecked(ALLOW_ACCOUNT_STATE_ROOT).Data, 0);
        }

        public bool AllowAccountStateRoot()
        {
            return GetAllowAccountStateRoot() == 1;
        }


        public void UpdateNextMaintenanceTime(long block_time)
        {
            long maintenance_time_interval = GetMaintenanceTimeInterval();

            long current_maintenance_time = GetNextMaintenanceTime();
            long round = (block_time - current_maintenance_time) / maintenance_time_interval;
            long next_maintenance_time = current_maintenance_time + (round + 1) * maintenance_time_interval;
            PutNextMaintenanceTime(next_maintenance_time);

            Logger.Info(
                string.Format(
                    "do update nextMaintenanceTime,currentMaintenanceTime:{0}, blockTime:{1},nextMaintenanceTime:{2}",
                    new DateTime(current_maintenance_time), new DateTime(block_time),
                    new DateTime(next_maintenance_time)
            ));
        }

        public bool SupportDR()
        {
            return GetAllowDelegateResource() == 1L;
        }

        public bool SupportVM()
        {
            return GetAllowCreationOfContracts() == 1L;
        }

        public void AddSystemContractAndSetPermission(int id)
        {
            byte[] available_contract_type = GetAvailableContractType();
            available_contract_type[id / 8] |= (byte)(1 << id % 8);
            PutAvailableContractType(available_contract_type);

            byte[] active_Default_Operations = GetActiveDefaultOperations();
            active_Default_Operations[id / 8] |= (byte)(1 << id % 8);
            PutActiveDefaultOperations(active_Default_Operations);
        }

        public void UpdateDynamicStoreByConfig()
        {
            if (Args.Instance.Committe.AllowVMConstantinople != 0)
            {
                PutAllowTvmConstantinople((int)Args.Instance.Committe.AllowVMConstantinople);
                AddSystemContractAndSetPermission(48);
            }
        }

        public void ApplyBlock(bool fill_block)
        {
            int[] block_filled_slots = GetBlockFilledSlots();
            int block_filled_slots_index = GetBlockFilledSlotsIndex();

            block_filled_slots[block_filled_slots_index] = fill_block ? 1 : 0;
            PutBlockFilledSlotsIndex((block_filled_slots_index + 1) % GetBlockFilledSlotsNumber());
            PutBlockFilledSlots(block_filled_slots);
        }

        public int CalculateFilledSlotsCount()
        {
            int[] block_filled_slots = GetBlockFilledSlots();
            return 100 * block_filled_slots.Sum() / GetBlockFilledSlotsNumber();
        }

        public void AddTotalNetWeight(long amount)
        {
            long total_net_weight = GetTotalNetWeight();
            total_net_weight += amount;
            PutTotalNetWeight(total_net_weight);
        }

        //The unit is trx
        public void AddTotalEnergyWeight(long amount)
        {
            long total_energy_weight = GetTotalEnergyWeight();
            total_energy_weight += amount;
            PutTotalEnergyWeight(total_energy_weight);
        }

        public void AddTotalCreateAccountCost(long fee)
        {
            long value = GetTotalCreateAccountCost() + fee;
            PutTotalCreateAccountFee(value);
        }

        public void AddTotalCreateWitnessCost(long fee)
        {
            long value = GetTotalCreateWitnessCost() + fee;
            PutTotalCreateWitnessFee(value);
        }

        public void AddTotalTransactionCost(long fee)
        {
            long value = GetTotalTransactionCost() + fee;
            PutTotalTransactionCost(value);
        }

        public void Forked()
        {
            Put(FORK_CONTROLLER, new BytesCapsule(true.ToString().ToBytes()));
        }

        public void StatsByVersion(int version, byte[] stats)
        {
            string stats_key = FORK_PREFIX + version;
            Put(stats_key.ToBytes(), new BytesCapsule(stats));
        }

        public byte[] StatsByVersion(int version)
        {
            string statsKey = FORK_PREFIX + version;
            return this.revoking_db.GetUnchecked(statsKey.ToBytes());
        }

        public bool GetForked()
        {
            byte[] value = this.revoking_db.GetUnchecked(FORK_CONTROLLER);
            return value == null ? false : bool.Parse(value.BytesToString());
        }
    }
}
