using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Capsule;
using Mineral.Core.Database;
using static Protocol.Transaction.Types;
using static Protocol.Transaction.Types.Contract.Types;

namespace Mineral.Core.Actuator
{
    public class ActuatorFactory
    {
        #region Field
        private static ActuatorFactory instance = null;
        #endregion


        #region Property
        public static ActuatorFactory Instance
        {
            get
            {
                if (instance == null)
                    instance = new ActuatorFactory();

                return instance;
            }
        }
        #endregion


        #region Contructor
        private ActuatorFactory() { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private static IActuator GetActuatorByContract(Contract contract, Manager db_manager)
        {
            switch (contract.Type)
            {
                case ContractType.AccountUpdateContract:
                    return new UpdateAccountActuator(contract.Parameter, db_manager);
                case ContractType.TransferContract:
                    return new TransferActuator(contract.Parameter, db_manager);
                case ContractType.TransferAssetContract:
                    return new TransferAssetActuator(contract.Parameter, db_manager);
                case ContractType.VoteAssetContract:
                    break;
                case ContractType.VoteWitnessContract:
                    return new VoteWitnessActuator(contract.Parameter, db_manager);
                case ContractType.WitnessCreateContract:
                    return new WitnessCreateActuator(contract.Parameter, db_manager);
                case ContractType.AccountCreateContract:
                    return new CreateAccountActuator(contract.Parameter, db_manager);
                case ContractType.AssetIssueContract:
                    return new AssetIssueActuator(contract.Parameter, db_manager);
                case ContractType.UnfreezeAssetContract:
                    return new UnfreezeAssetActuator(contract.Parameter, db_manager);
                case ContractType.WitnessUpdateContract:
                    return new WitnessUpdateActuator(contract.Parameter, db_manager);
                case ContractType.ParticipateAssetIssueContract:
                    return new ParticipateAssetIssueActuator(contract.Parameter, db_manager);
                case ContractType.FreezeBalanceContract:
                    return new FreezeBalanceActuator(contract.Parameter, db_manager);
                case ContractType.UnfreezeBalanceContract:
                    return new UnfreezeBalanceActuator(contract.Parameter, db_manager);
                case ContractType.WithdrawBalanceContract:
                    return new WithdrawBalanceActuator(contract.Parameter, db_manager);
                case ContractType.UpdateAssetContract:
                    return new UpdateAssetActuator(contract.Parameter, db_manager);
                case ContractType.ProposalCreateContract:
                    return new ProposalCreateActuator(contract.Parameter, db_manager);
                case ContractType.ProposalApproveContract:
                    return new ProposalApproveActuator(contract.Parameter, db_manager);
                case ContractType.ProposalDeleteContract:
                    return new ProposalDeleteActuator(contract.Parameter, db_manager);
                case ContractType.SetAccountIdContract:
                    return new SetAccountIdActuator(contract.Parameter, db_manager);
                case ContractType.UpdateSettingContract:
                    return new UpdateSettingContractActuator(contract.Parameter, db_manager);
                case ContractType.UpdateEnergyLimitContract:
                    return new UpdateEnergyLimitContractActuator(contract.Parameter, db_manager);
                case ContractType.ClearAbicontract:
                    return new ClearABIContractActuator(contract.Parameter, db_manager);
                case ContractType.ExchangeCreateContract:
                    return new ExchangeCreateActuator(contract.Parameter, db_manager);
                case ContractType.ExchangeInjectContract:
                    return new ExchangeInjectActuator(contract.Parameter, db_manager);
                case ContractType.ExchangeWithdrawContract:
                    return new ExchangeWithdrawActuator(contract.Parameter, db_manager);
                case ContractType.ExchangeTransactionContract:
                    return new ExchangeTransactionActuator(contract.Parameter, db_manager);
                case ContractType.AccountPermissionUpdateContract:
                    return new AccountPermissionUpdateActuator(contract.Parameter, db_manager);
                default:
                    break;
            }
            return null;
        }
        #endregion


        #region External Method
        public static List<IActuator> CreateActuator(TransactionCapsule transaction, Manager db_manager)
        {
            List<IActuator> actuators = new List<IActuator>();
            if (transaction == null || transaction.Instance == null)
            {
                Logger.Info("Transaction capsule or Transaction is null");
                return actuators;
            }

            if (db_manager == null)
            {
                throw new NullReferenceException("Manager is null.");
            }

            Protocol.Transaction.Types.raw raw = transaction.Instance.RawData;

            foreach (Contract contract in raw.Contract)
            {
                actuators.Add(GetActuatorByContract(contract, db_manager));
            }

            return actuators;
        }
        #endregion
    }
}
