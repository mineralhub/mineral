using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
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
    public class ExchangeTransactionActuator : AbstractActuator
    {
        #region Field
        private readonly byte[] COMPARE_CHARICTOR = Encoding.UTF8.GetBytes("_");
        #endregion


        #region Property
        #endregion


        #region Contructor
        public ExchangeTransactionActuator(Any contract, DataBaseManager db_manager) : base(contract, db_manager) { }
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
                ExchangeTransactionContract exchage_tx_contract = this.contract.Unpack<ExchangeTransactionContract>();
                AccountCapsule account = this.db_manager.Account.Get(exchage_tx_contract.OwnerAddress.ToByteArray());
                ExchangeCapsule exchange = this.db_manager.ExchangeFinal.Get(BitConverter.GetBytes(exchage_tx_contract.ExchangeId));

                byte[] first_token_id = exchange.FirstTokenId.ToByteArray();
                byte[] second_token_id = exchange.SecondTokenId.ToByteArray();
                byte[] token_id = exchage_tx_contract.TokenId.ToByteArray();
                long token_quantity = exchage_tx_contract.Quant;
                byte[] other_token_id = null;
                long other_token_quantity = exchange.Transaction(token_id, token_quantity);

                other_token_id = token_id.SequenceEqual(first_token_id) ? second_token_id : first_token_id;

                long new_balance = account.Balance - CalcFee();
                account.Balance = new_balance;

                if (token_id.SequenceEqual(COMPARE_CHARICTOR))
                {
                    account.Balance = new_balance - token_quantity;
                }
                else
                {
                    account.ReduceAssetAmountV2(token_id, token_quantity, this.db_manager);
                }

                if (other_token_id.SequenceEqual(COMPARE_CHARICTOR))
                {
                    account.Balance = new_balance + other_token_quantity;
                }
                else
                {
                    account.AddAssetAmountV2(other_token_id, other_token_quantity, this.db_manager);
                }

                this.db_manager.Account.Put(account.CreateDatabaseKey(), account);
                this.db_manager.PutExchangeCapsule(exchange);

                result.ExchangeReceivedAmount = other_token_quantity;
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
            return contract.Unpack<ExchangeTransactionContract>().OwnerAddress;
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
            if (!this.contract.Is(ExchangeTransactionContract.Descriptor))
            {
                ExchangeTransactionContract contract;
                try
                {
                    contract = this.contract.Unpack<ExchangeTransactionContract>();
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
                    throw new ContractValidateException("No enough balance for exchange transaction fee!");
                }

                ExchangeCapsule exchange;
                try
                {
                    exchange = this.db_manager.ExchangeFinal.Get(BitConverter.GetBytes(contract.ExchangeId));
                }
                catch (ItemNotFoundException ex)
                {
                    throw new ContractValidateException("Exchange[" + contract.ExchangeId + "] not exists");
                }

                byte[] first_token_id = exchange.FirstTokenId.ToByteArray();
                byte[] second_token_id = exchange.SecondTokenId.ToByteArray();
                long first_balance = exchange.FirstTokenBalance;
                long second_balance = exchange.SecondTokenBalance;

                byte[] token_id = contract.TokenId.ToByteArray();
                long token_quantity = contract.Quant;
                long token_expect = contract.Expected;

                if (this.db_manager.DynamicProperties.GetAllowSameTokenName() == 1)
                {
                    if (!token_id.SequenceEqual(COMPARE_CHARICTOR) && !TransactionUtil.IsNumber(token_id))
                    {
                        throw new ContractValidateException("token id is not a valid number");
                    }
                }
                if (!token_id.SequenceEqual(first_token_id) && !token_id.SequenceEqual(second_token_id))
                {
                    throw new ContractValidateException("token is not in exchange");
                }

                if (token_quantity <= 0)
                {
                    throw new ContractValidateException("token quant must greater than zero");
                }

                if (token_expect <= 0)
                {
                    throw new ContractValidateException("token expected must greater than zero");
                }

                if (first_balance == 0 || second_balance == 0)
                {
                    throw new ContractValidateException("Token balance in exchange is equal with 0,"
                        + "the exchange has been closed");
                }

                long balance_limit = this.db_manager.DynamicProperties.GetExchangeBalanceLimit();
                long token_balance = (token_id.SequenceEqual(first_token_id) ? first_balance : second_balance);
                token_balance += token_quantity;

                if (token_balance > balance_limit)
                {
                    throw new ContractValidateException("token balance must less than " + balance_limit);
                }

                if (token_id.SequenceEqual(COMPARE_CHARICTOR))
                {
                    if (account.Balance < (token_quantity + CalcFee()))
                    {
                        throw new ContractValidateException("balance is not enough");
                    }
                }
                else
                {
                    if (!account.AssetBalanceEnoughV2(token_id, token_quantity, this.db_manager))
                    {
                        throw new ContractValidateException("token balance is not enough");
                    }
                }

                long other_token_quantity = exchange.Transaction(token_id, token_quantity);
                if (other_token_quantity < token_expect)
                {
                    throw new ContractValidateException("token required must greater than expected");
                }
            }
            else
            {
                throw new ContractValidateException(
                    "contract type error,expected type [ExchangeTransactionContract],real type[" + contract.GetType().Name + "]");
            }

            return true;
        }
        #endregion
    }
}
