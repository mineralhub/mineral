using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Mineral.Core.Capsule;
using Mineral.Core.Capsule.Util;
using Mineral.Core.Database;
using Mineral.Core.Exception;
using Protocol;
using static Protocol.Transaction.Types.Result.Types;

namespace Mineral.Core.Actuator
{
    public class ExchangeCreateActuator : AbstractActuator
    {
        #region Field
        private readonly byte[] COMPARE_CHARICTOR = Encoding.UTF8.GetBytes("_");
        #endregion


        #region Property
        #endregion


        #region Contructor
        public ExchangeCreateActuator(Any contract, DatabaseManager db_manager) : base(contract, db_manager) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override long CalcFee()
        {
            return this.db_manager.DynamicProperties.GetExchangeCreateFee();
        }

        public override bool Execute(TransactionResultCapsule result)
        {
            long fee = CalcFee();
            try
            {
                ExchangeCreateContract exchange_create_contract = this.contract.Unpack<ExchangeCreateContract>();
                AccountCapsule account = this.db_manager.Account.Get(exchange_create_contract.OwnerAddress.ToByteArray());

                byte[] first_token_id = exchange_create_contract.FirstTokenId.ToByteArray();
                byte[] secodn_token_id = exchange_create_contract.SecondTokenId.ToByteArray();
                long first_token_balance = exchange_create_contract.FirstTokenBalance;
                long second_token_balance = exchange_create_contract.SecondTokenBalance;
                long new_balance = account.Balance - fee;

                account.Balance = new_balance;

                if (first_token_id.SequenceEqual(COMPARE_CHARICTOR))
                    account.Balance = new_balance - first_token_balance;
                else
                    account.ReduceAssetAmountV2(first_token_id, first_token_balance, this.db_manager);

                if (secodn_token_id.SequenceEqual(COMPARE_CHARICTOR))
                    account.Balance = new_balance - second_token_balance;
                else
                    account.ReduceAssetAmountV2(secodn_token_id, second_token_balance, this.db_manager);

                long id = this.db_manager.DynamicProperties.GetLatestExchangeNum() + 1;
                long now = this.db_manager.GetHeadBlockTimestamp();
                if (this.db_manager.DynamicProperties.GetAllowSameTokenName() == 0)
                {
                    ExchangeCapsule exchange = new ExchangeCapsule(
                                                            exchange_create_contract.OwnerAddress,
                                                            id,
                                                            now,
                                                            first_token_id,
                                                            secodn_token_id);

                    exchange.SetBalance(first_token_balance, second_token_balance);
                    this.db_manager.Exchange.Put(exchange.CreateDatabaseKey(), exchange);

                    if (!first_token_id.SequenceEqual(COMPARE_CHARICTOR))
                    {
                        string first_id = this.db_manager.AssetIssue.Get(first_token_id).Id;
                        first_token_id = Encoding.UTF8.GetBytes(first_id);
                    }
                    if (!secodn_token_id.SequenceEqual(COMPARE_CHARICTOR))
                    {
                        string second_id = this.db_manager.AssetIssue.Get(secodn_token_id).Id;
                        secodn_token_id = Encoding.UTF8.GetBytes(second_id);
                    }
                }

                ExchangeCapsule exchange_v2 = new ExchangeCapsule(
                                                        exchange_create_contract.OwnerAddress,
                                                        id,
                                                        now,
                                                        first_token_id,
                                                        secodn_token_id);

                exchange_v2.SetBalance(first_token_balance, second_token_balance);

                this.db_manager.ExchangeV2.Put(exchange_v2.CreateDatabaseKey(), exchange_v2);
                this.db_manager.Account.Put(account.CreateDatabaseKey(), account);
                this.db_manager.DynamicProperties.PutLatestExchangeNum(id);
                this.db_manager.AdjustBalance(this.db_manager.Account.GetBlackHole().CreateDatabaseKey(), fee);

                result.ExchangeId = id;
                result.SetStatus(fee, code.Sucess);
            }
            catch (BalanceInsufficientException e)
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
            return contract.Unpack<ExchangeCreateContract>().OwnerAddress;
        }

        public override bool Validate()
        {
            if (this.contract == null)
                throw new ContractValidateException("No contract!");

            if (this.db_manager == null)
                throw new ContractValidateException("No this.db_manager!");

            if (!this.contract.Is(ExchangeCreateContract.Descriptor))
            {
                ExchangeCreateContract contract = null;

                try
                {
                    contract = this.contract.Unpack<ExchangeCreateContract>();
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

                if (!this.db_manager.Account.Contains(owner_address))
                {
                    throw new ContractValidateException("account[" + owner_address.ToHexString() + "] not exists");
                }

                AccountCapsule account = this.db_manager.Account.Get(owner_address);

                if (account.Balance < CalcFee())
                {
                    throw new ContractValidateException("No enough balance for exchange create fee!");
                }

                byte[] first_token_id = contract.FirstTokenId.ToByteArray();
                byte[] secodn_token_id = contract.SecondTokenId.ToByteArray();
                long first_token_balance = contract.FirstTokenBalance;
                long second_token_balance = contract.SecondTokenBalance;

                if (this.db_manager.DynamicProperties.GetAllowSameTokenName() == 1)
                {
                    if (!first_token_id.SequenceEqual(COMPARE_CHARICTOR) && !TransactionUtil.IsNumber(first_token_id))
                    {
                        throw new ContractValidateException("first token id is not a valid number");
                    }
                    if (!secodn_token_id.SequenceEqual(COMPARE_CHARICTOR) && !TransactionUtil.IsNumber(secodn_token_id))
                    {
                        throw new ContractValidateException("second token id is not a valid number");
                    }
                }

                if (first_token_id.SequenceEqual(secodn_token_id))
                {
                    throw new ContractValidateException("cannot exchange same tokens");
                }

                if (first_token_balance <= 0 || second_token_balance <= 0)
                {
                    throw new ContractValidateException("token balance must greater than zero");
                }

                long balance_limit = this.db_manager.DynamicProperties.GetExchangeBalanceLimit();
                if (first_token_balance > balance_limit || second_token_balance > balance_limit)
                {
                    throw new ContractValidateException("token balance must less than " + balance_limit);
                }

                if (first_token_id.SequenceEqual(COMPARE_CHARICTOR))
                {
                    if (account.Balance < (first_token_balance + CalcFee()))
                    {
                        throw new ContractValidateException("balance is not enough");
                    }
                }
                else
                {
                    if (!account.AssetBalanceEnoughV2(first_token_id, first_token_balance, this.db_manager))
                    {
                        throw new ContractValidateException("first token balance is not enough");
                    }
                }

                if (secodn_token_id.SequenceEqual(COMPARE_CHARICTOR))
                {
                    if (account.Balance < (second_token_balance + CalcFee()))
                    {
                        throw new ContractValidateException("balance is not enough");
                    }
                }
                else
                {
                    if (!account.AssetBalanceEnoughV2(secodn_token_id, second_token_balance, this.db_manager))
                    {
                        throw new ContractValidateException("second token balance is not enough");
                    }
                }
            }
            else
            {
                throw new ContractValidateException(
                    "contract type error,expected type [ExchangeCreateContract],real type[" + this.contract.GetType().Name + "]");
            }

            return true;
        }
        #endregion
    }
}
