using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Mineral.Common.Storage;
using Mineral.Core.Capsule;
using Mineral.Core.Config;
using Mineral.Core.Database;
using Mineral.Core.Exception;
using Protocol;
using static Protocol.Transaction.Types.Result.Types;

namespace Mineral.Core.Actuator
{
    public class VoteWitnessActuator : AbstractActuator
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public VoteWitnessActuator(Any contract, DataBaseManager db_manager) : base(contract, db_manager) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private void CountVoteAccount(VoteWitnessContract contract, IDeposit deposit)
        {
            byte[] owner_address = contract.OwnerAddress.ToByteArray();

            VotesCapsule votes = null;
            AccountCapsule account = (Deposit == null) ? db_manager.Account.Get(owner_address) : Deposit.GetAccount(owner_address);
            if (Deposit != null)
            {
                if (Deposit.GetVotesCapsule(owner_address) == null)
                {
                    votes = new VotesCapsule(contract.OwnerAddress, account.GetVotesList());
                }
                else
                {
                    votes = Deposit.GetVotesCapsule(owner_address);
                }
            }
            else if (!db_manager.Votes.Contains(owner_address))
            {
                votes = new VotesCapsule(contract.OwnerAddress, account.GetVotesList());
            }
            else
            {
                votes = db_manager.Votes.Get(owner_address);
            }

            account.ClearVotes();
            votes.ClearNewVotes();

            foreach (Protocol.VoteWitnessContract.Types.Vote vote in contract.Votes)
            {
                Logger.Debug(
                    string.Format(
                        "CountVoteAccount, address[{0}]",
                        vote.VoteAddress.ToByteArray().ToHexString()));

                votes.AddNewVotes(vote.VoteAddress, vote.VoteCount);
                account.AddVotes(vote.VoteAddress, vote.VoteCount);
            }

            if (Deposit == null)
            {
                db_manager.Account.Put(account.CreateDatabaseKey(), account);
                db_manager.Votes.Put(owner_address, votes);
            }
            else
            {
                deposit.PutAccountValue(account.CreateDatabaseKey(), account);
                deposit.PutVoteValue(owner_address, votes);
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
                VoteWitnessContract contract = this.contract.Unpack<VoteWitnessContract>();
                CountVoteAccount(contract, Deposit);
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
            return contract.Unpack<VoteWitnessContract>().OwnerAddress;
    }

        public override bool Validate()
        {
            if (this.contract == null)
                throw new ContractValidateException("No contract!");

            if (this.db_manager == null && (this.deposit == null || this.deposit.DBManager == null))
                throw new ContractValidateException("No dbManager!");

            if (this.contract.Is(VoteWitnessContract.Descriptor))
            {
                VoteWitnessContract witness_contract = null;
                try
                {
                    witness_contract = this.contract.Unpack<VoteWitnessContract>();
                }
                catch (InvalidProtocolBufferException e)
                {
                    Logger.Debug(e.Message);
                    throw new ContractValidateException(e.Message);
                }

                if (!Wallet.AddressValid(witness_contract.OwnerAddress.ToByteArray()))
                {
                    throw new ContractValidateException("Invalid address");
                }

                byte[] owner_address = witness_contract.OwnerAddress.ToByteArray();
                string owner_address_str = owner_address.ToHexString();
                if (witness_contract.Votes.Count == 0)
                {
                    throw new ContractValidateException("VoteNumber must more than 0");
                }

                int max_vote = Parameter.ChainParameters.MAX_VOTE_NUMBER;
                if (witness_contract.Votes.Count > max_vote)
                {
                    throw new ContractValidateException("VoteNumber more than maxVoteNumber " + max_vote);
                }

                try
                {
                    long sum = 0;
                    foreach (VoteWitnessContract.Types.Vote vote in witness_contract.Votes)
                    {
                        byte[] witness_candidate = vote.VoteAddress.ToByteArray();
                        if (!Wallet.AddressValid(witness_candidate))
                            throw new ContractValidateException("Invalid vote address!");

                        if (vote.VoteCount <= 0)
                            throw new ContractValidateException("vote count must be greater than 0");

                        string witness_address_str = vote.VoteAddress.ToHexString();
                        if (Deposit != null)
                        {
                            if (Deposit.GetAccount(witness_candidate) == null)
                            {
                                throw new ContractValidateException(
                                    ActuatorParameter.ACCOUNT_EXCEPTION_STR + witness_address_str + ActuatorParameter.NOT_EXIST_STR);
                            }
                        }
                        else if (!db_manager.Account.Contains(witness_candidate))
                        {
                            throw new ContractValidateException(
                                ActuatorParameter.ACCOUNT_EXCEPTION_STR + witness_address_str + ActuatorParameter.NOT_EXIST_STR);
                        }
                        if (Deposit != null)
                        {
                            if (Deposit.GetWitness(witness_candidate) == null)
                            {
                                throw new ContractValidateException(
                                    ActuatorParameter.WITNESS_EXCEPTION_STR + witness_address_str + ActuatorParameter.NOT_EXIST_STR);
                            }
                        }
                        else if (!db_manager.Witness.Contains(witness_candidate))
                        {
                            throw new ContractValidateException(
                                ActuatorParameter.WITNESS_EXCEPTION_STR + witness_address_str + ActuatorParameter.NOT_EXIST_STR);
                        }
                        sum += vote.VoteCount;
                    }

                    AccountCapsule account = Deposit == null ? db_manager.Account.Get(owner_address) : Deposit.GetAccount(owner_address);
                    if (account == null)
                    {
                        throw new ContractValidateException(
                            ActuatorParameter.ACCOUNT_EXCEPTION_STR + owner_address_str + ActuatorParameter.NOT_EXIST_STR);
                    }

                    long power = account.GetMineralPower();

                    sum += 1000000L;
                    if (sum > power)
                    {
                        throw new ContractValidateException(
                            "The total number of votes[" + sum + "] is greater than the tronPower[" + power + "]");
                    }
                }
                catch (ArithmeticException e)
                {
                    Logger.Debug(e.Message);
                    throw new ContractValidateException(e.Message);
                }
            }
            else
            {
                throw new ContractValidateException(
                    "contract type error,expected type [VoteWitnessContract],real type[" + contract.GetType().Name + "]");
            }

            return true;
        }
        #endregion
    }
}
