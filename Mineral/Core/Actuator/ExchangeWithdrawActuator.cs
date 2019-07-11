using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Deveel.Math;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Mineral.Core.Capsule;
using Mineral.Core.Capsule.Util;
using Mineral.Core.Database;
using Mineral.Core.Exception;
using Mineral.Utils;
using Org.BouncyCastle.Utilities;
using Protocol;
using static Protocol.Transaction.Types.Result.Types;

namespace Mineral.Core.Actuator
{
    using BigInteger = System.Numerics.BigInteger;

    public class ExchangeWithdrawActuator : AbstractActuator
    {
        #region Field
        private readonly byte[] COMPARE_CHARICTOR = Encoding.UTF8.GetBytes("_");
        #endregion


        #region Property
        #endregion


        #region Contructor
        public ExchangeWithdrawActuator(Any contract, DatabaseManager db_manager) : base(contract, db_manager) { }
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
                ExchangeWithdrawContract ew_contract = this.contract.Unpack<ExchangeWithdrawContract>();
                AccountCapsule account = this.db_manager.Account.Get(ew_contract.OwnerAddress.ToByteArray());
                ExchangeCapsule exchange = this.db_manager.ExchangeFinal.Get(BitConverter.GetBytes(ew_contract.ExchangeId));

                byte[] first_token_id = exchange.FirstTokenId.ToByteArray();
                byte[] second_token_id = exchange.SecondTokenId.ToByteArray();
                long first_token_balance = exchange.FirstTokenBalance;
                long second_token_balance = exchange.SecondTokenBalance;
                byte[] token_id = ew_contract.TokenId.ToByteArray();
                long token_quantity = ew_contract.Quant;

                byte[] other_token_id = null;
                long other_token_quantity = 0;

                BigInteger first_balance = new BigInteger(first_token_balance);
                BigInteger second_balance = new BigInteger(second_token_balance);
                BigInteger quantity = new BigInteger(token_quantity);
                if (token_id.SequenceEqual(first_token_id))
                {
                    other_token_id = second_token_id;
                    other_token_quantity = (long)BigInteger.Multiply(second_balance, quantity);
                    other_token_quantity = (long)BigInteger.Divide(other_token_quantity, first_balance);
                    exchange.SetBalance(first_token_balance - token_quantity, second_token_balance - other_token_quantity);
                }
                else
                {
                    other_token_id = first_token_id;
                    other_token_quantity = (long)BigInteger.Multiply(first_balance, quantity);
                    other_token_quantity = (long)BigInteger.Divide(other_token_quantity, second_balance);
                    exchange.SetBalance(first_token_balance - other_token_quantity, second_token_balance - token_quantity);
                }

                long new_balance = account.Balance - CalcFee();

                if (token_id.SequenceEqual(COMPARE_CHARICTOR))
                {
                    account.Balance = new_balance + token_quantity;
                }
                else
                {
                    account.AddAssetAmountV2(token_id, token_quantity, this.db_manager);
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

                result.ExchangeWithdrawAnotherAmount = other_token_quantity;
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
            return contract.Unpack<ExchangeWithdrawContract>().OwnerAddress;
        }

        public override bool Validate()
        {
            if (this.contract == null)
                throw new ContractValidateException("No contract!");

            if (this.db_manager == null)
                throw new ContractValidateException("No this.db_manager!");

            if (this.contract.Is(ExchangeWithdrawContract.Descriptor))
            {
                ExchangeWithdrawContract contract = null;
                try
                {
                    contract = this.contract.Unpack<ExchangeWithdrawContract>();
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
                    throw new ContractValidateException("No enough balance for exchange withdraw fee!");
                }

                ExchangeCapsule exchange = null;
                try
                {
                    exchange = this.db_manager.ExchangeFinal.Get(BitConverter.GetBytes(contract.ExchangeId));
                }
                catch (ItemNotFoundException e)
                {
                    throw new ContractValidateException("Exchange[" + contract.ExchangeId + "] not exists");
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
                    throw new ContractValidateException("token is not in exchange");
                }

                if (token_quantity <= 0)
                {
                    throw new ContractValidateException("withdraw token quant must greater than zero");
                }

                if (first_token_balance == 0 || second_token_balance == 0)
                {
                    throw new ContractValidateException("Token balance in exchange is equal with 0,"
                        + "the exchange has been closed");
                }


                BigDecimal first_balance = new BigDecimal(first_token_balance);
                BigDecimal second_balance = new BigDecimal(second_token_balance);
                BigDecimal bigTokenQuant = new BigDecimal(token_quantity);
                if (token_id.SequenceEqual(first_token_id))
                {
                    other_token_quantity = second_balance.Multiply(bigTokenQuant)
                                                         .DivideToIntegralValue(first_balance).ToInt64();

                    if (first_token_balance < token_quantity || second_token_balance < other_token_quantity)
                    {
                        throw new ContractValidateException("exchange balance is not enough");
                    }

                    if (other_token_quantity <= 0)
                    {
                        throw new ContractValidateException("withdraw another token quant must greater than zero");
                    }

                    double remainder = second_balance.Multiply(bigTokenQuant)
                                                     .Divide(first_balance, 4, RoundingMode.HalfUp).ToDouble();
                    remainder -= other_token_quantity;

                    if (remainder / other_token_quantity > 0.0001)
                    {
                        throw new ContractValidateException("Not precise enough");
                    }

                }
                else
                {
                    other_token_quantity = first_balance.Multiply(bigTokenQuant)
                                                        .DivideToIntegralValue(second_balance).ToInt64();

                    if (second_token_balance < token_quantity || first_token_balance < other_token_quantity)
                    {
                        throw new ContractValidateException("exchange balance is not enough");
                    }

                    if (other_token_quantity <= 0)
                    {
                        throw new ContractValidateException("withdraw another token quant must greater than zero");
                    }

                    double remainder = first_balance.Multiply(bigTokenQuant)
                                                    .Divide(second_balance, 4, RoundingMode.HalfUp).ToDouble();
                    remainder -= other_token_quantity;

                    if (remainder / other_token_quantity > 0.0001)
                    {
                        throw new ContractValidateException("Not precise enough");
                    }
                }
            }
            else
            {
                throw new ContractValidateException(
                    "contract type error,expected type [ExchangeWithdrawContract],real type[" + this.contract.GetType().Name + "]");
            }

            return true;
        }
        #endregion
    }
}
