using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Mineral.Core.Capsule;
using Mineral.Core.Config;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Database;
using Mineral.Core.Exception;
using Protocol;
using static Protocol.Transaction.Types.Result.Types;

namespace Mineral.Core.Actuator
{
    public class ProposalCreateActuator : AbstractActuator
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public ProposalCreateActuator(Any contract, DatabaseManager db_manager) : base(contract, db_manager) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private bool ValidKey(long index)
        {
            int parameter_count = System.Enum.GetValues(typeof(Parameter.ChainParameters)).Length;

            return index >= 0 && index < parameter_count;
        }

        private void ValidateValue(KeyValuePair<long, long> entry)
        {
            switch (entry.Key)
            {
                case (0):
                    {
                        if (entry.Value < 3 * 27 * 1000 || entry.Value > 24 * 3600 * 1000)
                        {
                            throw new ContractValidateException(
                                "Bad chain parameter value,valid range is [3 * 27 * 1000,24 * 3600 * 1000]");
                        }
                        return;
                    }
                case (1):
                case (2):
                case (3):
                case (4):
                case (5):
                case (6):
                case (7):
                case (8):
                    {
                        if (entry.Value < 0 || entry.Value > 100_000_000_000_000_000L)
                        {
                            throw new ContractValidateException(
                                "Bad chain parameter value,valid range is [0,100_000_000_000_000_000L]");
                        }
                        break;
                    }
                case (9):
                    {
                        if (entry.Value != 1)
                        {
                            throw new ContractValidateException(
                                "This value[ALLOW_CREATION_OF_CONTRACTS] is only allowed to be 1");
                        }
                        break;
                    }
                case (10):
                    {
                        if (this.db_manager.DynamicProperties.GetRemoveThePowerOfTheGr() == -1)
                        {
                            throw new ContractValidateException(
                                "This proposal has been executed before and is only allowed to be executed once");
                        }

                        if (entry.Value != 1)
                        {
                            throw new ContractValidateException(
                                "This value[REMOVE_THE_POWER_OF_THE_GR] is only allowed to be 1");
                        }
                        break;
                    }
                case (11):
                    break;
                case (12):
                    break;
                case (13):
                    if (entry.Value < 10 || entry.Value > 100)
                    {
                        throw new ContractValidateException(
                            "Bad chain parameter value,valid range is [10,100]");
                    }
                    break;
                case (14):
                    {
                        if (entry.Value != 1)
                        {
                            throw new ContractValidateException(
                                "This value[ALLOW_UPDATE_ACCOUNT_NAME] is only allowed to be 1");
                        }
                        break;
                    }
                case (15):
                    {
                        if (entry.Value != 1)
                        {
                            throw new ContractValidateException(
                                "This value[ALLOW_SAME_TOKEN_NAME] is only allowed to be 1");
                        }
                        break;
                    }
                case (16):
                    {
                        if (entry.Value != 1)
                        {
                            throw new ContractValidateException(
                                "This value[ALLOW_DELEGATE_RESOURCE] is only allowed to be 1");
                        }
                        break;
                    }
                case (17):
                    {
                        if (!this.db_manager.ForkController.Pass(Parameter.ForkBlockVersionParameters.ENERGY_LIMIT))
                        {
                            throw new ContractValidateException("Bad chain parameter id");
                        }
                        if (this.db_manager.ForkController.Pass(Parameter.ForkBlockVersion.VERSION_3_2_2))
                        {
                            throw new ContractValidateException("Bad chain parameter id");
                        }
                        if (entry.Value < 0 || entry.Value > 100_000_000_000_000_000L)
                        {
                            throw new ContractValidateException(
                                "Bad chain parameter value,valid range is [0,100_000_000_000_000_000L]");
                        }
                        break;
                    }
                case (18):
                    {
                        if (entry.Value != 1)
                        {
                            throw new ContractValidateException(
                                "This value[ALLOW_TVM_TRANSFER_TRC10] is only allowed to be 1");
                        }
                        if (this.db_manager.DynamicProperties.GetAllowSameTokenName() == 0)
                        {
                            throw new ContractValidateException("[ALLOW_SAME_TOKEN_NAME] proposal must be approved "
                                + "before [ALLOW_TVM_TRANSFER_TRC10] can be proposed");
                        }
                        break;
                    }
                case (19):
                    {
                        if (!this.db_manager.ForkController.Pass(Parameter.ForkBlockVersion.VERSION_3_2_2))
                        {
                            throw new ContractValidateException("Bad chain parameter id");
                        }
                        if (entry.Value < 0 || entry.Value > 100_000_000_000_000_000L)
                        {
                            throw new ContractValidateException(
                                "Bad chain parameter value,valid range is [0,100_000_000_000_000_000L]");
                        }
                        break;
                    }
                case (20):
                    {
                        if (!this.db_manager.ForkController.Pass(Parameter.ForkBlockVersion.VERSION_3_5))
                        {
                            throw new ContractValidateException("Bad chain parameter id: ALLOW_MULTI_SIGN");
                        }
                        if (entry.Value != 1)
                        {
                            throw new ContractValidateException(
                                "This value[ALLOW_MULTI_SIGN] is only allowed to be 1");
                        }
                        break;
                    }
                case (21):
                    {
                        if (!this.db_manager.ForkController.Pass(Parameter.ForkBlockVersion.VERSION_3_5))
                        {
                            throw new ContractValidateException("Bad chain parameter id: ALLOW_ADAPTIVE_ENERGY");
                        }
                        if (entry.Value != 1)
                        {
                            throw new ContractValidateException(
                                "This value[ALLOW_ADAPTIVE_ENERGY] is only allowed to be 1");
                        }
                        break;
                    }
                case (22):
                    {
                        if (!this.db_manager.ForkController.Pass(Parameter.ForkBlockVersion.VERSION_3_5))
                        {
                            throw new ContractValidateException(
                                "Bad chain parameter id: UPDATE_ACCOUNT_PERMISSION_FEE");
                        }
                        if (entry.Value < 0 || entry.Value > 100_000_000_000L)
                        {
                            throw new ContractValidateException(
                                "Bad chain parameter value,valid range is [0,100_000_000_000L]");
                        }
                        break;
                    }
                case (23):
                    {
                        if (!this.db_manager.ForkController.Pass(Parameter.ForkBlockVersion.VERSION_3_5))
                        {
                            throw new ContractValidateException("Bad chain parameter id: MULTI_SIGN_FEE");
                        }
                        if (entry.Value < 0 || entry.Value > 100_000_000_000L)
                        {
                            throw new ContractValidateException(
                                "Bad chain parameter value,valid range is [0,100_000_000_000L]");
                        }
                        break;
                    }
                case (24):
                    {
                        if (!this.db_manager.ForkController.Pass(Parameter.ForkBlockVersion.VERSION_3_6))
                        {
                            throw new ContractValidateException("Bad chain parameter id");
                        }
                        if (entry.Value != 1 && entry.Value != 0)
                        {
                            throw new ContractValidateException(
                                "This value[ALLOW_PROTO_FILTER_NUM] is only allowed to be 1 or 0");
                        }
                        break;
                    }
                case (25):
                    {
                        if (!this.db_manager.ForkController.Pass(Parameter.ForkBlockVersion.VERSION_3_6))
                        {
                            throw new ContractValidateException("Bad chain parameter id");
                        }
                        if (entry.Value != 1 && entry.Value != 0)
                        {
                            throw new ContractValidateException(
                                "This value[ALLOW_ACCOUNT_STATE_ROOT] is only allowed to be 1 or 0");
                        }
                        break;
                    }
                case (26):
                    {
                        if (entry.Value != 1)
                        {
                            throw new ContractValidateException(
                                "This value[ALLOW_TVM_CONSTANTINOPLE] is only allowed to be 1");
                        }
                        if (this.db_manager.DynamicProperties.GetAllowTvmTransferTrc10() == 0)
                        {
                            throw new ContractValidateException(
                                "[ALLOW_TVM_TRANSFER_TRC10] proposal must be approved "
                                    + "before [ALLOW_TVM_CONSTANTINOPLE] can be proposed");
                        }
                        break;
                    }
                default:
                    break;
            }
        }
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
                ProposalCreateContract proposal_create_contract = this.contract.Unpack<ProposalCreateContract>();

                long id = Deposit == null ?
                    this.db_manager.DynamicProperties.GetLatestProposalNum() + 1 : Deposit.GetLatestProposalNum() + 1;

                ProposalCapsule proposal = new ProposalCapsule(proposal_create_contract.OwnerAddress, id);

                proposal.Parameters = new Dictionary<long, long>(proposal_create_contract.Parameters);

                long now = this.db_manager.GetHeadBlockTimestamp();
                long maintenance_interval = Deposit == null ?
                    this.db_manager.DynamicProperties.GetMaintenanceTimeInterval() : Deposit.GetMaintenanceTimeInterval();
                proposal.CreateTime = now;

                long current_maintenance_time = Deposit == null ?
                    this.db_manager.DynamicProperties.GetNextMaintenanceTime() : Deposit.GetNextMaintenanceTime();
                long now3 = now + (int)Args.Instance.Block.ProposalExpireTime;
                long round = (now3 - current_maintenance_time) / maintenance_interval;
                long expiration_time = current_maintenance_time + (round + 1) * maintenance_interval;
                proposal.ExpirationTime = expiration_time;

                if (Deposit == null)
                {
                    this.db_manager.Proposal.Put(proposal.CreateDatabaseKey(), proposal);
                    this.db_manager.DynamicProperties.PutLatestProposalNum(id);
                }
                else
                {
                    Deposit.PutProposalValue(proposal.CreateDatabaseKey(), proposal);
                    Deposit.PutDynamicPropertiesWithLatestProposalNum(id);
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
            return contract.Unpack<ProposalCreateContract>().OwnerAddress;
        }

        public override bool Validate()
        {
            if (this.contract == null)
            {
                throw new ContractValidateException("No contract!");
            }
            if (this.db_manager == null && (deposit == null || deposit.DBManager == null))
            {
                throw new ContractValidateException("No this.db_manager!");
            }
            if (this.contract.Is(ProposalCreateContract.Descriptor))
            {
                ProposalCreateContract contract = null;

                try
                {
                    contract = this.contract.Unpack<ProposalCreateContract>();
                }
                catch (InvalidProtocolBufferException e)
                {
                    throw new ContractValidateException(e.Message);
                }

                byte[] owner_address = contract.OwnerAddress.ToByteArray();
                if (!Wallet.IsValidAddress(owner_address))
                {
                    throw new ContractValidateException("Invalid address");
                }

                if (Deposit != null)
                {
                    if (deposit.GetAccount(owner_address) == null)
                    {
                        throw new ContractValidateException(
                            ActuatorParameter.ACCOUNT_EXCEPTION_STR + owner_address.ToHexString() + ActuatorParameter.NOT_EXIST_STR);
                    }
                }
                else if (!this.db_manager.Account.Contains(owner_address))
                {
                    throw new ContractValidateException(
                        ActuatorParameter.ACCOUNT_EXCEPTION_STR + owner_address.ToHexString() + ActuatorParameter.NOT_EXIST_STR);
                }

                if (Deposit != null)
                {
                    if (Deposit.GetWitness(owner_address) == null)
                    {
                        throw new ContractValidateException(
                            ActuatorParameter.WITNESS_EXCEPTION_STR + owner_address.ToHexString() + ActuatorParameter.NOT_EXIST_STR);
                    }
                }
                else if (!this.db_manager.Witness.Contains(owner_address))
                {
                    throw new ContractValidateException(
                        ActuatorParameter.WITNESS_EXCEPTION_STR + owner_address.ToHexString() + ActuatorParameter.NOT_EXIST_STR);
                }

                if (contract.Parameters.Count == 0)
                {
                    throw new ContractValidateException("This proposal has no parameter.");
                }

                foreach (KeyValuePair<long, long> entry in contract.Parameters)
                {
                    if (!ValidKey(entry.Key))
                        throw new ContractValidateException("Bad chain parameter id");

                    ValidateValue(entry);
                }
            }
            else
            {
                throw new ContractValidateException(
                    "contract type error,expected type [ProposalCreateContract],real type[" + contract.GetType().Name + "]");
            }

            return true;
        }
        #endregion
    }
}
