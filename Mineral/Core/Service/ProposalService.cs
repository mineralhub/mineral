using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Application;
using Mineral.Core.Config.Arguments;

namespace Mineral.Core.Service
{
    public class ProposalService : IService
    {
        public enum ProposalParameters
        {
            MAINTENANCE_TIME_INTERVAL, //ms  ,0
            ACCOUNT_UPGRADE_COST, //drop ,1
            CREATE_ACCOUNT_FEE, //drop ,2
            TRANSACTION_FEE, //drop ,3
            ASSET_ISSUE_FEE, //drop ,4
            WITNESS_PAY_PER_BLOCK, //drop ,5
            WITNESS_STANDBY_ALLOWANCE, //drop ,6
            CREATE_NEW_ACCOUNT_FEE_IN_SYSTEM_CONTRACT, //drop ,7
            CREATE_NEW_ACCOUNT_BANDWIDTH_RATE, // 1 ~ ,8
            ALLOW_CREATION_OF_CONTRACTS, // 0 / >0 ,9
            REMOVE_THE_POWER_OF_THE_GR,  // 1 ,10
            ENERGY_FEE, // drop, 11
            EXCHANGE_CREATE_FEE, // drop, 12
            MAX_CPU_TIME_OF_ONE_TX, // ms, 13
            ALLOW_UPDATE_ACCOUNT_NAME, // 1, 14
            ALLOW_SAME_TOKEN_NAME, // 1, 15
            ALLOW_DELEGATE_RESOURCE, // 0, 16
            TOTAL_ENERGY_LIMIT, // 50,000,000,000, 17
            ALLOW_VM_TRANSFER_TRC10, // 1, 18
            TOTAL_CURRENT_ENERGY_LIMIT, // 50,000,000,000, 19
            ALLOW_MULTI_SIGN, // 1, 20
            ALLOW_ADAPTIVE_ENERGY, // 1, 21
            UPDATE_ACCOUNT_PERMISSION_FEE, // 100, 22
            MULTI_SIGN_FEE, // 1, 23
            ALLOW_PROTO_FILTER_NUM, // 1, 24
            ALLOW_ACCOUNT_STATE_ROOT, // 1, 25
            ALLOW_VM_CONSTANTINOPLE, // 1, 26
        }

        #region Field
        public delegate long ProposalParameterAction();

        private static Dictionary<ProposalParameters, ProposalParameterAction> proposal_actions = new Dictionary<ProposalParameters, ProposalParameterAction>()
        {
            { ProposalParameters.MAINTENANCE_TIME_INTERVAL, new ProposalParameterAction(Manager.Instance.DBManager.DynamicProperties.GetMaintenanceTimeInterval) },
            { ProposalParameters.ACCOUNT_UPGRADE_COST, new ProposalParameterAction(Manager.Instance.DBManager.DynamicProperties.GetAccountUpgradeCost) },
            { ProposalParameters.CREATE_ACCOUNT_FEE, new ProposalParameterAction(Manager.Instance.DBManager.DynamicProperties.GetCreateAccountFee) },
            { ProposalParameters.TRANSACTION_FEE, new ProposalParameterAction(Manager.Instance.DBManager.DynamicProperties.GetTransactionFee) },
            { ProposalParameters.ASSET_ISSUE_FEE, new ProposalParameterAction(Manager.Instance.DBManager.DynamicProperties.GetAssetIssueFee) },
            { ProposalParameters.WITNESS_PAY_PER_BLOCK, new ProposalParameterAction(Manager.Instance.DBManager.DynamicProperties.GetWitnessPayPerBlock) },
            { ProposalParameters.WITNESS_STANDBY_ALLOWANCE, new ProposalParameterAction(Manager.Instance.DBManager.DynamicProperties.GetWitnessStandbyAllowance) },
            { ProposalParameters.CREATE_NEW_ACCOUNT_FEE_IN_SYSTEM_CONTRACT, new ProposalParameterAction(Manager.Instance.DBManager.DynamicProperties.GetCreateNewAccountFeeInSystemContract) },
            { ProposalParameters.CREATE_NEW_ACCOUNT_BANDWIDTH_RATE, new ProposalParameterAction(Manager.Instance.DBManager.DynamicProperties.GetCreateNewAccountBandwidthRate) },
            { ProposalParameters.ALLOW_CREATION_OF_CONTRACTS, new ProposalParameterAction(Manager.Instance.DBManager.DynamicProperties.GetAllowCreationOfContracts) },
            { ProposalParameters.REMOVE_THE_POWER_OF_THE_GR, new ProposalParameterAction(Manager.Instance.DBManager.DynamicProperties.GetRemoveThePowerOfTheGr) },
            { ProposalParameters.ENERGY_FEE, new ProposalParameterAction(Manager.Instance.DBManager.DynamicProperties.GetEnergyFee) },
            { ProposalParameters.EXCHANGE_CREATE_FEE, new ProposalParameterAction(Manager.Instance.DBManager.DynamicProperties.GetExchangeCreateFee) },
            { ProposalParameters.MAX_CPU_TIME_OF_ONE_TX, new ProposalParameterAction(Manager.Instance.DBManager.DynamicProperties.GetMaxCpuTimeOfOneTx) },
            { ProposalParameters.ALLOW_UPDATE_ACCOUNT_NAME, new ProposalParameterAction(Manager.Instance.DBManager.DynamicProperties.GetAllowUpdateAccountName) },
            { ProposalParameters.ALLOW_SAME_TOKEN_NAME, new ProposalParameterAction(Manager.Instance.DBManager.DynamicProperties.GetAllowSameTokenName) },
            { ProposalParameters.ALLOW_DELEGATE_RESOURCE, new ProposalParameterAction(Manager.Instance.DBManager.DynamicProperties.GetAllowDelegateResource) },
            { ProposalParameters.TOTAL_ENERGY_LIMIT, new ProposalParameterAction(Manager.Instance.DBManager.DynamicProperties.GetTotalEnergyLimit) },
            { ProposalParameters.ALLOW_VM_TRANSFER_TRC10, new ProposalParameterAction(Manager.Instance.DBManager.DynamicProperties.GetAllowVmTransferTrc10) },
            { ProposalParameters.TOTAL_CURRENT_ENERGY_LIMIT, new ProposalParameterAction(Manager.Instance.DBManager.DynamicProperties.GetTotalEnergyCurrentLimit) },
            { ProposalParameters.ALLOW_MULTI_SIGN, new ProposalParameterAction(Manager.Instance.DBManager.DynamicProperties.GetAllowMultiSign) },
            { ProposalParameters.ALLOW_ADAPTIVE_ENERGY, new ProposalParameterAction(Manager.Instance.DBManager.DynamicProperties.GetAllowAdaptiveEnergy) },
            { ProposalParameters.UPDATE_ACCOUNT_PERMISSION_FEE, new ProposalParameterAction(Manager.Instance.DBManager.DynamicProperties.GetUpdateAccountPermissionFee) },
            { ProposalParameters.MULTI_SIGN_FEE, new ProposalParameterAction(Manager.Instance.DBManager.DynamicProperties.GetMultiSignFee) },
            { ProposalParameters.ALLOW_PROTO_FILTER_NUM, new ProposalParameterAction(Manager.Instance.DBManager.DynamicProperties.GetAllowProtoFilterNum) },
            { ProposalParameters.ALLOW_ACCOUNT_STATE_ROOT, new ProposalParameterAction(Manager.Instance.DBManager.DynamicProperties.GetAllowAccountStateRoot) },
            { ProposalParameters.ALLOW_VM_CONSTANTINOPLE, new ProposalParameterAction(Manager.Instance.DBManager.DynamicProperties.GetAllowVmConstantinople) }
        };
        #endregion


        #region Property
        #endregion


        #region Constructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Init()
        {
        }

        public void Init(Args args)
        {
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public static Protocol.ChainParameters GetProposalParameters()
        {
            Protocol.ChainParameters parameters = new Protocol.ChainParameters();

            foreach (var parameter in proposal_actions)
            {
                parameters.ChainParameter.Add(new Protocol.ChainParameters.Types.ChainParameter()
                {
                    Key = parameter.Key.ToString(),
                    Value = parameter.Value()
                });
            }

            return parameters;
        }
        #endregion
    }
}
