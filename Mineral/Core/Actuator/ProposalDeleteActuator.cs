using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ProposalDeleteActuator : AbstractActuator
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public ProposalDeleteActuator(Any contract, DatabaseManager db_manager) : base(contract, db_manager) { }
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
                ProposalDeleteContract proposal_delete_contract = this.contract.Unpack<ProposalDeleteContract>();
                ProposalCapsule proposal = (Deposit == null) ?
                    this.db_manager.Proposal.Get(BitConverter.GetBytes(proposal_delete_contract.ProposalId))
                    : Deposit.GetProposalCapsule(BitConverter.GetBytes(proposal_delete_contract.ProposalId));

                proposal.State = Proposal.Types.State.Canceled;
                if (Deposit == null)
                {
                    this.db_manager.Proposal.Put(proposal.CreateDatabaseKey(), proposal);
                }
                else
                {
                    Deposit.PutProposalValue(proposal.CreateDatabaseKey(), proposal);
                }

                result.SetStatus(fee, code.Sucess);
            }
            catch (InvalidProtocolBufferException e)
            {
                Logger.Debug(e.Message);
                result.SetStatus(fee, code.Failed);
                throw new ContractExeException(e.Message);
            }
            catch (ItemNotFoundException e)
            {
                Logger.Debug(e.Message);
                result.SetStatus(fee, code.Failed);
                throw new ContractExeException(e.Message);
            }
            return true;
        }

        public override ByteString GetOwnerAddress()
        {
            return contract.Unpack<ProposalDeleteContract>().OwnerAddress;
        }

        public override bool Validate()
        {
            if (this.contract == null)
            {
                throw new ContractValidateException("No contract!");
            }
            if (this.db_manager == null && (Deposit == null || Deposit.DBManager == null))
            {
                throw new ContractValidateException("No this.db_manager!");
            }
            if (this.contract.Is(ProposalDeleteContract.Descriptor))
            {
                ProposalDeleteContract contract = null;

                try
                {
                    contract = this.contract.Unpack<ProposalDeleteContract>();
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
                    if (Deposit.GetAccount(owner_address) == null)
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

                long latest_proposal = Deposit == null ?
                    this.db_manager.DynamicProperties.GetLatestProposalNum() : Deposit.GetLatestProposalNum();

                if (contract.ProposalId > latest_proposal)
                {
                    throw new ContractValidateException(
                        ActuatorParameter.PROPOSAL_EXCEPTION_STR + contract.ProposalId + ActuatorParameter.NOT_EXIST_STR);
                }

                ProposalCapsule proposal = null;
                try
                {
                    proposal = Deposit == null ?
                        this.db_manager.Proposal.Get(BitConverter.GetBytes(contract.ProposalId))
                        : Deposit.GetProposalCapsule(BitConverter.GetBytes(contract.ProposalId));
                }
                catch (ItemNotFoundException e)
                {
                    throw new ContractValidateException(
                        ActuatorParameter.PROPOSAL_EXCEPTION_STR + contract.ProposalId + ActuatorParameter.NOT_EXIST_STR, e);
                }

                long now = this.db_manager.GetHeadBlockTimestamp();
                if (!proposal.Address.SequenceEqual(contract.OwnerAddress))
                {
                    throw new ContractValidateException(
                        ActuatorParameter.PROPOSAL_EXCEPTION_STR + contract.ProposalId + "] " + "is not proposed by " + owner_address.ToHexString());
                }
                if (now >= proposal.ExpirationTime)
                {
                    throw new ContractValidateException(
                        ActuatorParameter.PROPOSAL_EXCEPTION_STR + contract.ProposalId + "] expired");
                }
                if (proposal.State == Proposal.Types.State.Canceled)
                {
                    throw new ContractValidateException(
                        ActuatorParameter.PROPOSAL_EXCEPTION_STR + contract.ProposalId + "] canceled");
                }
            }
            else
            {
                throw new ContractValidateException(
                    "contract type error,expected type [ProposalDeleteContract],real type[" + contract.GetType().Name + "]");
            }

            return true;
        }
        #endregion
    }
}
