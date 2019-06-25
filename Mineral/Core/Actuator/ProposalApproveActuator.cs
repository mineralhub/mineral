using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Mineral.Core.Capsule;
using Mineral.Core.Database;
using Mineral.Core.Exception;
using Protocol;
using static Protocol.Transaction.Types.Result.Types;

namespace Mineral.Core.Actuator
{
    public class ProposalApproveActuator : AbstractActuator
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public ProposalApproveActuator(Any contract, Manager db_manager) : base(contract, db_manager) { }
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
                ProposalApproveContract proposal_approve_contract = this.contract.Unpack<ProposalApproveContract>();
                ProposalCapsule proposal = Deposit == null ?
                    this.db_manager.Proposal.Get(BitConverter.GetBytes(proposal_approve_contract.ProposalId))
                    : Deposit.GetProposalCapsule(BitConverter.GetBytes(proposal_approve_contract.ProposalId));

                ByteString committee_address = proposal_approve_contract.OwnerAddress;
                if (proposal_approve_contract.IsAddApproval)
                    proposal.AddApproval(committee_address);
                else
                    proposal.RemoveApproval(committee_address);

                if (Deposit == null)
                    this.db_manager.Proposal.Put(proposal.CreateDatabaseKey(), proposal);
                else
                    deposit.PutProposalValue(proposal.CreateDatabaseKey(), proposal);

                result.SetStatus(fee, code.Sucess);
            }
            catch (ItemNotFoundException e)
            {
                Logger.Debug(e.Message);
                result.SetStatus(fee, code.Failed);
                throw new ContractExeException(e.Message);
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
            return contract.Unpack<ProposalApproveContract>().OwnerAddress;
        }

        public override bool Validate()
        {
            if (this.contract == null)
                throw new ContractValidateException("No contract!");

            if (this.db_manager == null && (Deposit == null || Deposit.DBManager == null))
                throw new ContractValidateException("No this.db_manager!");

            if (this.contract.Is(ProposalApproveContract.Descriptor))
            {
                ProposalApproveContract contract;
                try
                {
                    contract = this.contract.Unpack<ProposalApproveContract>();
                }
                catch (InvalidProtocolBufferException e)
                {
                    throw new ContractValidateException(e.Message);
                }

                byte[] owner_address = contract.OwnerAddress.ToByteArray();

                if (!Wallet.AddressValid(owner_address))
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
                    throw new ContractValidateException(ActuatorParameter.ACCOUNT_EXCEPTION_STR + owner_address.ToHexString()
                        + ActuatorParameter.NOT_EXIST_STR);
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

                long latest_proposal = Deposit == null ? 
                    this.db_manager.DynamicProperties.GetLatestProposalNum() : Deposit.GetLatestProposalNum();
                if (contract.ProposalId > latest_proposal)
                {
                    throw new ContractValidateException(ActuatorParameter.PROPOSAL_EXCEPTION_STR + contract.ProposalId
                        + ActuatorParameter.NOT_EXIST_STR);
                }

                long now = this.db_manager.GetHeadBlockTimestamp();
                ProposalCapsule proposal;
                try
                {
                    proposal = Deposit == null ?
                        this.db_manager.Proposal.Get(BitConverter.GetBytes(contract.ProposalId))
                        : Deposit.GetProposalCapsule(BitConverter.GetBytes(contract.ProposalId));
                }
                catch (ItemNotFoundException ex)
                {
                    throw new ContractValidateException(
                        ActuatorParameter.PROPOSAL_EXCEPTION_STR + contract.ProposalId + ActuatorParameter.NOT_EXIST_STR);
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

                if (!contract.IsAddApproval)
                {
                    if (!proposal.Approvals.Contains(contract.OwnerAddress))
                    {
                        throw new ContractValidateException(
                            ActuatorParameter.WITNESS_EXCEPTION_STR + owner_address.ToHexString() + "]has not approved proposal[" + contract
                                .ProposalId + "] before");
                    }
                }
                else
                {
                    if (proposal.Approvals.Contains(contract.OwnerAddress))
                    {
                        throw new ContractValidateException(
                            ActuatorParameter.WITNESS_EXCEPTION_STR + owner_address.ToHexString() + "]has approved proposal[" + contract
                                .ProposalId + "] before");
                    }
                }
            }
            else
            {
                throw new ContractValidateException(
                    "contract type error,expected type [ProposalApproveContract],real type[" + contract.GetType().Name + "]");
            }

            return true;
        }
        #endregion
    }
}
