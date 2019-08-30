using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Capsule;
using Google.Protobuf;
using static Protocol.Transaction.Types.Contract.Types;
using Mineral.Core.Actuator;
using Protocol;
using Mineral.Core.Exception;
using static Mineral.Core.Capsule.BlockCapsule;
using Mineral.Core.Database;
using Mineral.Core.Config.Arguments;

namespace Mineral.Core.Net.RpcHandler
{
    public class RpcWalletApi
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public static TransactionCapsule CreateTransactionCapsule(IMessage message, ContractType type)
        {
            DatabaseManager db_manager = Manager.Instance.DBManager;

            TransactionCapsule transaction = new TransactionCapsule(message, type);

            if (type != ContractType.CreateSmartContract &&
                type != ContractType.TriggerSmartContract)
            {
                List<IActuator> actuators = ActuatorFactory.CreateActuator(transaction, db_manager);
                foreach (IActuator actuator in actuators)
                {
                    actuator.Validate();
                }
            }

            if (type == ContractType.CreateSmartContract)
            {
                CreateSmartContract contract = ContractCapsule.GetSmartContractFromTransaction(transaction.Instance);
                long percent = contract.NewContract.ConsumeUserResourcePercent;
                if (percent < 0 || percent > 100)
                {
                    throw new ContractValidateException("percent must be >= 0 and <= 100");
                }
            }

            try
            {
                BlockId id = db_manager.HeadBlockId;
                if (Args.Instance.Transaction.ReferenceBlock.Equals("solid"))
                {
                    id = db_manager.SolidBlockId;
                }

                transaction.SetReference(id.Num, id.Hash);
                transaction.Expiration = db_manager.GetHeadBlockTimestamp() + (long)Args.Instance.Transaction.ExpireTimeInMillis;
                transaction.Timestamp = Helper.CurrentTimeMillis();
            }
            catch (System.Exception e)
            {
                Logger.Error("Create transaction capsule failed.", e);
            }

            return transaction;
        }
        #endregion
    }
}
