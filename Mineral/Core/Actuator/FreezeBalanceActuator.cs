using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Mineral.Core.Capsule;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Database;
using Mineral.Core.Exception;
using Protocol;
using static Protocol.Transaction.Types.Result.Types;

namespace Mineral.Core.Actuator
{
    public class FreezeBalanceActuator : AbstractActuator
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public FreezeBalanceActuator(Any contract, DatabaseManager db_manager) : base(contract, db_manager) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private void DelegateResource(byte[] owner_address, byte[] receiver_address, bool is_bandwidth, long balance, long expire_time)
        {
            byte[] key = DelegatedResourceCapsule.CreateDatabaseKey(owner_address, receiver_address);
            DelegatedResourceCapsule delegated_resource = this.db_manager.DelegatedResource.Get(key);
            if (delegated_resource != null)
            {
                if (is_bandwidth)
                {
                    delegated_resource.AddFrozenBalanceForBandwidth(balance, expire_time);
                }
                else
                {
                    delegated_resource.AddFrozenBalanceForEnergy(balance, expire_time);
                }
            }
            else
            {
                delegated_resource = new DelegatedResourceCapsule(
                    ByteString.CopyFrom(owner_address),
                    ByteString.CopyFrom(receiver_address));
                if (is_bandwidth)
                {
                    delegated_resource.SetFrozenBalanceForBandwidth(balance, expire_time);
                }
                else
                {
                    delegated_resource.SetFrozenBalanceForEnergy(balance, expire_time);
                }

            }
            this.db_manager.DelegatedResource.Put(key, delegated_resource);

            DelegatedResourceAccountIndexCapsule delegate_account_index = this.db_manager.DelegateResourceAccountIndex.Get(owner_address);
            if (delegate_account_index == null)
            {
                delegate_account_index = new DelegatedResourceAccountIndexCapsule(
                    ByteString.CopyFrom(owner_address));
            }
            List<ByteString> to_accounts = new List<ByteString>(delegate_account_index.ToAccounts);
            if (!to_accounts.Contains(ByteString.CopyFrom(receiver_address)))
            {
                delegate_account_index.AddToAccount(ByteString.CopyFrom(receiver_address));
            }
            this.db_manager.DelegateResourceAccountIndex.Put(owner_address, delegate_account_index);


            delegate_account_index = this.db_manager.DelegateResourceAccountIndex.Get(receiver_address);
            if (delegate_account_index == null)
            {
                delegate_account_index = new DelegatedResourceAccountIndexCapsule(
                    ByteString.CopyFrom(receiver_address));
            }
            List<ByteString> fromAccountsList = delegate_account_index.FromAccounts;
            if (!fromAccountsList.Contains(ByteString.CopyFrom(owner_address)))
            {
                delegate_account_index.AddFromAccount(ByteString.CopyFrom(owner_address));
            }
            this.db_manager.DelegateResourceAccountIndex.Put(receiver_address, delegate_account_index);

            AccountCapsule receiver = this.db_manager.Account.Get(receiver_address);
            if (is_bandwidth)
            {
                receiver.AddAcquiredDelegatedFrozenBalanceForBandwidth(balance);
            }
            else
            {
                receiver.AddAcquiredDelegatedFrozenBalanceForEnergy(balance);
            }

            this.db_manager.Account.Put(receiver.CreateDatabaseKey(), receiver);
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
            FreezeBalanceContract freeze_balance_contract = null;
            try
            {
                freeze_balance_contract = contract.Unpack<FreezeBalanceContract>();
            }
            catch (InvalidProtocolBufferException e)
            {
                Logger.Debug(e.Message);
                result.SetStatus(fee, code.Failed);
                throw new ContractExeException(e.Message);
            }

            AccountCapsule account = this.db_manager.Account.Get(freeze_balance_contract.OwnerAddress.ToByteArray());

            long now = this.db_manager.GetHeadBlockTimestamp();
            long duration = freeze_balance_contract.FrozenDuration * 86_400_000;

            long new_balance = account.Balance - freeze_balance_contract.FrozenBalance;

            long frozen_balance = freeze_balance_contract.FrozenBalance;
            long expire_time = now + duration;
            byte[] owner_address = freeze_balance_contract.OwnerAddress.ToByteArray();
            byte[] receiver_address = freeze_balance_contract.ReceiverAddress.ToByteArray();

            switch (freeze_balance_contract.Resource)
            {
                case ResourceCode.Bandwidth:
                    {
                        if (receiver_address != null && receiver_address.Length > 0 && this.db_manager.DynamicProperties.SupportDR())
                        {
                            DelegateResource(owner_address, receiver_address, true, frozen_balance, expire_time);
                            account.AddDelegatedFrozenBalanceForBandwidth(frozen_balance);
                        }
                        else
                        {
                            long newFrozenBalanceForBandwidth = frozen_balance + account.FrozenBalance;
                            account.SetFrozenForBandwidth(newFrozenBalanceForBandwidth, expire_time);
                        }
                        this.db_manager.DynamicProperties.AddTotalNetWeight(frozen_balance / 1000_000L);
                    }
                    break;
                case ResourceCode.Energy:
                    {
                        if (receiver_address != null
                            && receiver_address.Length > 0
                            && this.db_manager.DynamicProperties.SupportDR())
                        {
                            DelegateResource(owner_address, receiver_address, false,
                                frozen_balance, expire_time);
                            account.AddDelegatedFrozenBalanceForEnergy(frozen_balance);
                        }
                        else
                        {
                            long new_energy = frozen_balance + account.AccountResource.FrozenBalanceForEnergy.FrozenBalance;
                            account.SetFrozenForEnergy(new_energy, expire_time);
                        }
                        this.db_manager.DynamicProperties.AddTotalEnergyWeight(frozen_balance / 1000_000L);
                    }
                    break;
            }

            account.Balance = new_balance;
            this.db_manager.Account.Put(account.CreateDatabaseKey(), account);

            result.SetStatus(fee, code.Sucess);

            return true;
        }

        public override ByteString GetOwnerAddress()
        {
            return contract.Unpack<FreezeBalanceContract>().OwnerAddress;
        }

        public override bool Validate()
        {
            if (this.contract == null)
                throw new ContractValidateException("No contract!");

            if (this.db_manager == null)
                throw new ContractValidateException("No this.db_manager!");

            if (this.contract.Is(FreezeBalanceContract.Descriptor))
            {
                FreezeBalanceContract freeze_balance_contract;

                try
                {
                    freeze_balance_contract = this.contract.Unpack<FreezeBalanceContract>();
                }
                catch (InvalidProtocolBufferException e)
                {
                    Logger.Debug(e.Message);
                    throw new ContractValidateException(e.Message);
                }
                byte[] owner_address = freeze_balance_contract.OwnerAddress.ToByteArray();
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

                long frozen_balance = freeze_balance_contract.FrozenBalance;
                if (frozen_balance <= 0)
                {
                    throw new ContractValidateException("frozenBalance must be positive");
                }
                if (frozen_balance < 1_000_000L)
                {
                    throw new ContractValidateException("frozenBalance must be more than 1TRX");
                }

                if (!(account.FrozenCount == 0 || account.FrozenCount == 1))
                {
                    throw new ContractValidateException("frozenCount must be 0 or 1");
                }

                if (frozen_balance > account.Balance)
                {
                    throw new ContractValidateException("frozenBalance must be less than accountBalance");
                }

                long frozen_duration = freeze_balance_contract.FrozenDuration;
                long min_frozen_time = this.db_manager.DynamicProperties.GetMinFrozenTime();
                long max_frozen_time = this.db_manager.DynamicProperties.GetMaxFrozenTime();

                bool need_check = Args.Instance.Block.CheckFrozenTime == 1;
                if (need_check && !(frozen_duration >= min_frozen_time
                    && frozen_duration <= max_frozen_time))
                {
                    throw new ContractValidateException(
                        "frozenDuration must be less than " + max_frozen_time + " days "
                            + "and more than " + min_frozen_time + " days");
                }

                if (freeze_balance_contract.Resource != ResourceCode.Bandwidth
                    && freeze_balance_contract.Resource != ResourceCode.Energy)
                {
                    throw new ContractValidateException(
                        "ResourceCode error,valid ResourceCode[BANDWIDTH、ENERGY]");
                }

                byte[] receiver_address = freeze_balance_contract.ReceiverAddress.ToByteArray();
                if (receiver_address != null && receiver_address.Length > 0 && this.db_manager.DynamicProperties.SupportDR())
                {
                    if (receiver_address.SequenceEqual(owner_address))
                    {
                        throw new ContractValidateException(
                            "receiverAddress must not be the same as owner_address");
                    }

                    if (!Wallet.AddressValid(receiver_address))
                    {
                        throw new ContractValidateException("Invalid receiverAddress");
                    }

                    AccountCapsule receiver = this.db_manager.Account.Get(receiver_address);
                    if (receiver == null)
                    {
                        throw new ContractValidateException(
                            "Account[" + receiver_address.ToHexString() + "] not exists");
                    }
                }
            }
            else
            {
                throw new ContractValidateException(
                    "contract type error,expected type [FreezeBalanceContract],real type[" + contract.GetType().Name + "]");
            }

            return true;
        }
        #endregion
    }
}
