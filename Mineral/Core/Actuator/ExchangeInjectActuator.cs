using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
    public class ExchangeInjectActuator : AbstractActuator
    {
        #region Field
        private readonly byte[] COMPARE_CHARICTOR = Encoding.UTF8.GetBytes("_");
        #endregion


        #region Property
        #endregion


        #region Contructor
        public ExchangeInjectActuator(Any contract, DatabaseManager db_manager) : base(contract, db_manager) { }
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
                ExchangeInjectContract ei_contract = this.contract.Unpack<ExchangeInjectContract>();
                AccountCapsule account = this.db_manager.Account.Get(ei_contract.OwnerAddress.ToByteArray());
                ExchangeCapsule exchange = this.db_manager.ExchangeFinal.Get(BitConverter.GetBytes(ei_contract.ExchangeId));

                byte[] first_token_id = exchange.FirstTokenId.ToByteArray();
                byte[] second_token_id = exchange.SecondTokenId.ToByteArray();
                long first_token_balance = exchange.FirstTokenBalance;
                long second_token_balance = exchange.SecondTokenBalance;
                byte[] token_id = ei_contract.TokenId.ToByteArray();
                long token_quantity = ei_contract.Quant;

                byte[] other_token_id = null;
                long other_token_quantity = 0;

                if (token_id.SequenceEqual(first_token_id))
                {
                    other_token_id = second_token_id;
                    other_token_quantity = (long)Math.Floor((double)(second_token_balance * token_quantity) / first_token_balance);
                    exchange.SetBalance(first_token_balance + token_quantity, second_token_balance + other_token_quantity);
                }
                else
                {
                    other_token_id = first_token_id;
                    other_token_quantity = (long)Math.Floor((double)(first_token_balance * token_quantity) / second_token_balance);
                    exchange.SetBalance(first_token_balance + other_token_quantity, second_token_balance + token_quantity);
                }

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
                    account.Balance = new_balance - other_token_quantity;
                }
                else
                {
                    account.ReduceAssetAmountV2(other_token_id, other_token_quantity, this.db_manager);
                }
                this.db_manager.Account.Put(account.CreateDatabaseKey(), account);
                this.db_manager.PutExchangeCapsule(exchange);

                result.ExchangeInjectAnotherAmount = other_token_quantity;
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
            return contract.Unpack<ExchangeInjectContract>().OwnerAddress;
        }

        public override bool Validate()
        {
            if (this.contract == null)
                throw new ContractValidateException("No contract!");

            if (this.db_manager == null)
                throw new ContractValidateException("No this.db_manager!");

            if (!this.contract.Is(ExchangeInjectContract.Descriptor))
            {
                ExchangeInjectContract contract;
                try
                {
                    contract = this.contract.Unpack<ExchangeInjectContract>();
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

                if (!this.db_manager.Account.Contains(owner_address))
                {
                    throw new ContractValidateException("account[" + owner_address.ToHexString() + "] not exists");
                }

                AccountCapsule account = this.db_manager.Account.Get(owner_address);

                if (account.Balance < CalcFee())
                {
                    throw new ContractValidateException("No enough balance for exchange inject fee!");
                }

                ExchangeCapsule exchange = null;

                try
                {
                    exchange = this.db_manager.ExchangeFinal.Get(BitConverter.GetBytes(contract.ExchangeId));

                }
                catch (ItemNotFoundException e)
                {
                    throw new ContractValidateException("Exchange[" + contract.ExchangeId + "] not exists", e);
                }

                if (!account.Address.Equals(exchange.CreatorAddress))
                {
                    throw new ContractValidateException("account[" + owner_address.ToHexString() + "] is not creator");
                }

                byte[] first_token_id = exchange.FirstTokenId.ToByteArray();
                byte[] second_token_id = exchange.SecondTokenId.ToByteArray();
                long first_token_balance = exchange.FirstTokenBalance;
                long second_token_balance = exchange.SecondTokenBalance;

                byte[] token_id = contract.TokenId.ToByteArray();
                long token_quantity = contract.Quant;

                byte[] other_token_id = null;
                long other_token_quantity = 0;

                if (this.db_manager.DynamicProperties.GetAllowSameTokenName() == 1)
                {
                    if (!token_id.SequenceEqual(COMPARE_CHARICTOR) && !TransactionUtil.IsNumber(token_id))
                    {
                        throw new ContractValidateException("token id is not a valid number");
                    }
                }

                if (!token_id.SequenceEqual(first_token_id) && !token_id.SequenceEqual(second_token_id))
                {
                    throw new ContractValidateException("token id is not in exchange");
                }

                if (first_token_balance == 0 || second_token_balance == 0)
                {
                    throw new ContractValidateException("Token balance in exchange is equal with 0,"
                        + "the exchange has been closed");
                }

                if (token_quantity <= 0)
                {
                    throw new ContractValidateException("injected token quant must greater than zero");
                }

                BigInteger first_balance = new BigInteger(first_token_balance);
                BigInteger second_balance = new BigInteger(second_token_balance);
                BigInteger quantity = new BigInteger(token_quantity);
                long new_token_balance = 0;
                long new_other_token_balance = 0;
                if (token_id.SequenceEqual(first_token_id))
                {
                    other_token_id = second_token_id;
                    other_token_quantity = (long)BigInteger.Multiply(second_balance, token_quantity);
                    other_token_quantity = (long)BigInteger.Divide(other_token_quantity, first_balance);
                    new_token_balance = first_token_balance + token_quantity;
                    new_other_token_balance = second_token_balance + other_token_quantity;
                }
                else
                {
                    other_token_id = first_token_id;
                    other_token_quantity = (long)BigInteger.Multiply(first_balance, token_quantity);
                    other_token_quantity = (long)BigInteger.Divide(other_token_quantity, second_balance);
                    new_token_balance = second_token_balance + token_quantity;
                    new_other_token_balance = first_token_balance + other_token_quantity;
                }

                if (other_token_quantity <= 0)
                {
                    throw new ContractValidateException("the calculated token quant  must be greater than 0");
                }

                long balance_limit = this.db_manager.DynamicProperties.GetExchangeBalanceLimit();
                if (new_token_balance > balance_limit || new_other_token_balance > balance_limit)
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

                if (other_token_id.SequenceEqual(COMPARE_CHARICTOR))
                {
                    if (account.Balance < (other_token_quantity + CalcFee()))
                    {
                        throw new ContractValidateException("balance is not enough");
                    }
                }
                else
                {
                    if (!account.AssetBalanceEnoughV2(other_token_id, other_token_quantity, this.db_manager))
                    {
                        throw new ContractValidateException("other token balance is not enough");
                    }
                }
            }
            else
            {
                throw new ContractValidateException(
                    "contract type error,expected type [ExchangeInjectContract],real type[" + this.contract.GetType().Name + "]");
            }

            return true;
        }
        #endregion
    }
}
