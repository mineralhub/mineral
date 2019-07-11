using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Mineral.Core.Capsule;
using Mineral.Core.Database;
using Mineral.Core.Exception;
using Protocol;
using static Protocol.Account.Types;
using static Protocol.Transaction.Types.Result.Types;

namespace Mineral.Core.Actuator
{
    public class UnfreezeBalanceActuator : AbstractActuator
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public UnfreezeBalanceActuator(Any contract, DatabaseManager manager) : base(contract, manager) { }
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
            UnfreezeBalanceContract unfreeze_balance_contract;
            try
            {
                unfreeze_balance_contract = contract.Unpack<UnfreezeBalanceContract>();
            }
            catch (InvalidProtocolBufferException e)
            {
                Logger.Debug(e.Message);
                result.SetStatus(fee, code.Failed);
                throw new ContractExeException(e.Message);
            }
            byte[] owner_address = unfreeze_balance_contract.OwnerAddress.ToByteArray();

            AccountCapsule account = this.db_manager.Account.Get(owner_address);
            long old_balance = account.Balance;
            long unfreeze_balance = 0L;

            byte[] receiver_address = unfreeze_balance_contract.ReceiverAddress.ToByteArray();
            if (receiver_address != null && receiver_address.Length > 0 && this.db_manager.DynamicProperties.SupportDR())
            {
                byte[] key = DelegatedResourceCapsule.CreateDatabaseKey(unfreeze_balance_contract.OwnerAddress.ToByteArray(),
                                                                        unfreeze_balance_contract.ReceiverAddress.ToByteArray());

                DelegatedResourceCapsule delegated_resource = this.db_manager.DelegatedResource.Get(key);
                AccountCapsule receiver = this.db_manager.Account.Get(receiver_address);

                switch (unfreeze_balance_contract.Resource)
                {
                    case ResourceCode.Bandwidth:
                        {
                            unfreeze_balance = delegated_resource.FrozenBalanceForBandwidth;
                            delegated_resource.SetFrozenBalanceForBandwidth(0, 0);
                            receiver.AddAcquiredDelegatedFrozenBalanceForBandwidth(-unfreeze_balance);
                            account.AddDelegatedFrozenBalanceForBandwidth(-unfreeze_balance);
                        }
                        break;
                    case ResourceCode.Energy:
                        {
                            unfreeze_balance = delegated_resource.FrozenBalanceForEnergy;
                            delegated_resource.SetFrozenBalanceForEnergy(0, 0);
                            receiver.AddAcquiredDelegatedFrozenBalanceForEnergy(-unfreeze_balance);
                            account.AddDelegatedFrozenBalanceForEnergy(-unfreeze_balance);
                        }
                        break;
                    default:
                        break;
                }

                account.Balance = old_balance + unfreeze_balance;
                this.db_manager.Account.Put(receiver.CreateDatabaseKey(), receiver);

                if (delegated_resource.FrozenBalanceForBandwidth == 0 && delegated_resource.FrozenBalanceForEnergy == 0)
                {
                    this.db_manager.DelegatedResource.Delete(key);

                    DelegatedResourceAccountIndexCapsule delegate_account_index =
                        this.db_manager.DelegateResourceAccountIndex.Get(owner_address);

                    if (delegate_account_index != null)
                    {
                        List<ByteString> to_accounts = new List<ByteString>(delegate_account_index.ToAccounts);
                        to_accounts.Remove(ByteString.CopyFrom(receiver_address));

                        delegate_account_index.ToAccounts = to_accounts;
                        this.db_manager.DelegateResourceAccountIndex.Put(owner_address, delegate_account_index);
                    }

                    delegate_account_index = this.db_manager.DelegateResourceAccountIndex.Get(receiver_address);
                    if (delegate_account_index != null)
                    {
                        List<ByteString> from_accounts = new List<ByteString>(delegate_account_index.FromAccounts);
                        from_accounts.Remove(ByteString.CopyFrom(owner_address));

                        delegate_account_index.FromAccounts = from_accounts;
                        this.db_manager.DelegateResourceAccountIndex.Put(receiver_address, delegate_account_index);
                    }

                }
                else
                {
                    this.db_manager.DelegatedResource.Put(key, delegated_resource);
                }
            }
            else
            {
                switch (unfreeze_balance_contract.Resource)
                {
                    case ResourceCode.Bandwidth:
                        {
                            List<Frozen> frozens = new List<Frozen>();
                            frozens.AddRange(account.FrozenList);

                            long now = this.db_manager.GetHeadBlockTimestamp();
                            foreach (Frozen frozen in frozens)
                            {
                                if (frozen.ExpireTime <= now)
                                {
                                    unfreeze_balance += frozen.FrozenBalance;
                                    frozens.Remove(frozen);
                                }
                            }

                            account.Balance = old_balance + unfreeze_balance;
                            account.FrozenList.Clear();
                            account.FrozenList.AddRange(frozens);
                        }
                        break;
                    case ResourceCode.Energy:
                        {
                            unfreeze_balance = account.AccountResource.FrozenBalanceForEnergy.FrozenBalance;

                            account.AccountResource.FrozenBalanceForEnergy = new Frozen();
                            account.Balance = old_balance + unfreeze_balance;
                        }
                        break;
                    default:
                        break;
                }

            }

            switch (unfreeze_balance_contract.Resource)
            {
                case ResourceCode.Bandwidth:
                    {
                        this.db_manager.DynamicProperties.AddTotalNetWeight(-unfreeze_balance / 1000_000L);
                    }
                    break;
                case ResourceCode.Energy:
                    {
                        this.db_manager.DynamicProperties.AddTotalEnergyWeight(-unfreeze_balance / 1000_000L);
                    }
                    break;
                default:
                    break;
            }

            VotesCapsule votes = null;
            if (!this.db_manager.Votes.Contains(owner_address))
            {
                votes = new VotesCapsule(unfreeze_balance_contract.OwnerAddress, account.GetVotesList());
            }
            else
            {
                votes = this.db_manager.Votes.Get(owner_address);
            }

            account.ClearVotes();
            votes.ClearNewVotes();

            this.db_manager.Account.Put(owner_address, account);
            this.db_manager.Votes.Put(owner_address, votes);

            result.UnfreezeAmount = unfreeze_balance;
            result.SetStatus(fee, code.Sucess);

            return true;
        }

        public override ByteString GetOwnerAddress()
        {
            return contract.Unpack<UnfreezeBalanceContract>().OwnerAddress;
        }

        public override bool Validate()
        {
            if (this.contract == null)
            {
                throw new ContractValidateException("No contract!");
            }
            if (this.db_manager == null)
            {
                throw new ContractValidateException("No this.db_manager!");
            }
            if (this.contract.Is(UnfreezeBalanceContract.Descriptor))
            {
                UnfreezeBalanceContract unfreeze_balance_contract = null;
                try
                {
                    unfreeze_balance_contract = this.contract.Unpack<UnfreezeBalanceContract>();
                }
                catch (InvalidProtocolBufferException e)
                {
                    Logger.Debug(e.Message);
                    throw new ContractValidateException(e.Message);
                }

                byte[] owner_address = unfreeze_balance_contract.OwnerAddress.ToByteArray();
                if (!Wallet.AddressValid(owner_address))
                {
                    throw new ContractValidateException("Invalid address");
                }

                AccountCapsule account = this.db_manager.Account.Get(owner_address);
                if (account == null)
                {
                    throw new ContractValidateException(
                        "Account[" + owner_address.ToHexString() + "] not exists");
                }
                long now = this.db_manager.GetHeadBlockTimestamp();
                byte[] receiver_address = unfreeze_balance_contract.ReceiverAddress.ToByteArray();
                if (receiver_address != null && receiver_address.Length > 0 && this.db_manager.DynamicProperties.SupportDR())
                {
                    if (receiver_address.SequenceEqual(owner_address))
                    {
                        throw new ContractValidateException("receiver_address must not be the same as owner_address");
                    }

                    if (!Wallet.AddressValid(receiver_address))
                    {
                        throw new ContractValidateException("Invalid receiver_address");
                    }

                    AccountCapsule receiver = this.db_manager.Account.Get(receiver_address);
                    if (receiver == null)
                    {
                        throw new ContractValidateException("Account[" + receiver_address.ToHexString() + "] not exists");
                    }

                    byte[] key = DelegatedResourceCapsule.CreateDatabaseKey(
                                                                unfreeze_balance_contract.OwnerAddress.ToByteArray(),
                                                                unfreeze_balance_contract.ReceiverAddress.ToByteArray());

                    DelegatedResourceCapsule delegated_resource = this.db_manager.DelegatedResource.Get(key);
                    if (delegated_resource == null)
                    {
                        throw new ContractValidateException("delegated Resource not exists");
                    }

                    switch (unfreeze_balance_contract.Resource)
                    {
                        case ResourceCode.Bandwidth:
                            {
                                if (delegated_resource.FrozenBalanceForBandwidth <= 0)
                                    throw new ContractValidateException("no delegatedFrozenBalance(BANDWIDTH)");

                                if (receiver.AcquiredDelegatedFrozenBalanceForBandwidth < delegated_resource.FrozenBalanceForBandwidth)
                                {
                                    throw new ContractValidateException(
                                        "AcquiredDelegatedFrozenBalanceForBandwidth["
                                        + receiver.AcquiredDelegatedFrozenBalanceForBandwidth
                                        + "] < delegatedBandwidth["
                                        + delegated_resource.FrozenBalanceForBandwidth
                                        + "],this should never happen");
                                }

                                if (delegated_resource.ExpireTimeForBandwidth > now)
                                    throw new ContractValidateException("It's not time to unfreeze.");
                            }
                            break;
                        case ResourceCode.Energy:
                            {
                                if (delegated_resource.FrozenBalanceForEnergy <= 0)
                                {
                                    throw new ContractValidateException("no delegateFrozenBalance(Energy)");
                                }
                                if (receiver.AcquiredDelegatedFrozenBalanceForEnergy < delegated_resource.FrozenBalanceForEnergy)
                                {
                                    throw new ContractValidateException(
                                        "AcquiredDelegatedFrozenBalanceForEnergy["
                                        + receiver.AcquiredDelegatedFrozenBalanceForEnergy
                                        + "] < delegatedEnergy["
                                        + delegated_resource.FrozenBalanceForEnergy
                                        + "],this should never happen");
                                }
                                if (delegated_resource.GetExpireTimeForEnergy(this.db_manager) > now)
                                {
                                    throw new ContractValidateException("It's not time to unfreeze.");
                                }
                            }
                            break;
                        default:
                            {
                                throw new ContractValidateException("ResourceCode error.valid ResourceCode[BANDWIDTH、Energy]");
                            }
                    }
                }
                else
                {
                    switch (unfreeze_balance_contract.Resource)
                    {
                        case ResourceCode.Bandwidth:
                            {
                                if (account.FrozenCount <= 0)
                                    throw new ContractValidateException("no frozenBalance(BANDWIDTH)");

                                long unfreeze_count = account.FrozenList.Where(frozen => frozen.ExpireTime <= now).Count();
                                if (unfreeze_count <= 0)
                                    throw new ContractValidateException("It's not time to unfreeze(BANDWIDTH).");
                            }
                            break;
                        case ResourceCode.Energy:
                            {
                                Frozen frozen = account.AccountResource.FrozenBalanceForEnergy;
                                if (frozen.FrozenBalance <= 0)
                                    throw new ContractValidateException("no frozenBalance(Energy)");

                                if (frozen.ExpireTime > now)
                                    throw new ContractValidateException("It's not time to unfreeze(Energy).");
                            }
                            break;
                        default:
                            {
                                throw new ContractValidateException("ResourceCode error.valid ResourceCode[BANDWIDTH、Energy]");
                            }
                    }

                }
            }
            else
            {
                throw new ContractValidateException(
                    "contract type error,expected type [UnfreezeBalanceContract],real type[" + contract.GetType().Name + "]");
            }

            return true;
        }
        #endregion
    }
}
