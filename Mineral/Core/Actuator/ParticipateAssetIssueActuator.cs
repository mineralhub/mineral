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
using static Protocol.Transaction.Types.Result.Types;

namespace Mineral.Core.Actuator
{
    public class ParticipateAssetIssueActuator : AbstractActuator
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public ParticipateAssetIssueActuator(Any contract, DatabaseManager db_manager) : base(contract, db_manager) { }
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
                ParticipateAssetIssueContract asset_issue_contract = this.contract.Unpack<ParticipateAssetIssueContract>();
                long cost = asset_issue_contract.Amount;

                //subtract from owner address
                byte[] owner_address = asset_issue_contract.OwnerAddress.ToByteArray();
                AccountCapsule owner_account = this.db_manager.Account.Get(owner_address);
                owner_account.Balance = owner_account.Balance - cost - fee;

                byte[] key = asset_issue_contract.AssetName.ToByteArray();

                AssetIssueCapsule asset_issue = this.db_manager.GetAssetIssueStoreFinal().Get(key);

                long exchange_amount = cost * asset_issue.Num;
                exchange_amount = (long)Math.Floor((double)(exchange_amount / asset_issue.TransactionNum));
                owner_account.AddAssetAmountV2(key, exchange_amount, this.db_manager);

                byte[] to_address = asset_issue_contract.ToAddress.ToByteArray();
                AccountCapsule to_account = this.db_manager.Account.Get(to_address);
                to_account.Balance = to_account.Balance + cost;
                if (!to_account.ReduceAssetAmountV2(key, exchange_amount, this.db_manager))
                {
                    throw new ContractExeException("reduceAssetAmount failed !");
                }

                //write to db
                this.db_manager.Account.Put(owner_address, owner_account);
                this.db_manager.Account.Put(to_address, to_account);
                result.SetStatus(fee, code.Sucess);
            }
            catch (InvalidProtocolBufferException e)
            {
                Logger.Debug(e.Message);
                result.SetStatus(fee, code.Failed);
                throw new ContractExeException(e.Message);
            }
            catch (ArithmeticException e)
            {
                Logger.Debug(e.Message);
                result.SetStatus(fee, code.Failed);
                throw new ContractExeException(e.Message);
            }

            return true;
        }

        public override ByteString GetOwnerAddress()
        {
            return this.contract.Unpack<ParticipateAssetIssueContract>().OwnerAddress;
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

            if (this.contract.Is(ParticipateAssetIssueContract.Descriptor))
            {
                ParticipateAssetIssueContract asset_issue_contract;
                try
                {
                    asset_issue_contract = this.contract.Unpack<ParticipateAssetIssueContract>();
                }
                catch (InvalidProtocolBufferException e)
                {
                    Logger.Debug(e.Message);
                    throw new ContractValidateException(e.Message);
                }

                byte[] owner_address = asset_issue_contract.OwnerAddress.ToByteArray();
                byte[] to_address = asset_issue_contract.ToAddress.ToByteArray();
                byte[] asset_name = asset_issue_contract.AssetName.ToByteArray();
                long amount = asset_issue_contract.Amount;

                if (!Wallet.AddressValid(owner_address))
                {
                    throw new ContractValidateException("Invalid ownerAddress");
                }

                if (!Wallet.AddressValid(to_address))
                {
                    throw new ContractValidateException("Invalid toAddress");
                }

                if (amount <= 0)
                {
                    throw new ContractValidateException("Amount must greater than 0!");
                }

                if (owner_address.SequenceEqual(to_address))
                {
                    throw new ContractValidateException("Cannot participate asset Issue yourself !");
                }

                AccountCapsule owner_account = this.db_manager.Account.Get(owner_address);
                if (owner_account == null)
                    throw new ContractValidateException("Account does not exist!");

                try
                {
                    //Whether the balance is enough
                    long fee = CalcFee();
                    if (owner_account.Balance < amount + fee)
                    {
                        throw new ContractValidateException("No enough balance !");
                    }

                    AssetIssueCapsule asset_issue = this.db_manager.GetAssetIssueStoreFinal().Get(asset_name);
                    if (asset_issue == null)
                    {
                        throw new ContractValidateException("No asset named " + Encoding.UTF8.GetString(asset_name));
                    }

                    if (!to_address.SequenceEqual(asset_issue.OwnerAddress.ToByteArray()))
                    {
                        throw new ContractValidateException(
                            "The asset is not issued by " + to_address.ToHexString());
                    }

                    long now = this.db_manager.DynamicProperties.GetLatestBlockHeaderTimestamp();
                    if (now >= asset_issue.EndTime || now < asset_issue.StartTime)
                        throw new ContractValidateException("No longer valid period!");

                    int tx_num = asset_issue.TransactionNum;
                    int num = asset_issue.Num;
                    long exchange_amount = amount * num;
                    exchange_amount = (long)Math.Floor((double)(exchange_amount / tx_num));

                    if (exchange_amount <= 0)
                        throw new ContractValidateException("Can not process the exchange!");

                    AccountCapsule to_account = this.db_manager.Account.Get(to_address);
                    if (to_account == null)
                    {
                        throw new ContractValidateException("To account does not exist!");
                    }

                    if (!to_account.AssetBalanceEnoughV2(asset_name, exchange_amount,
                        this.db_manager))
                    {
                        throw new ContractValidateException("Asset balance is not enough !");
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
                    "contract type error,expected type [ParticipateAssetIssueContract],real type[" + contract.GetType().Name + "]");
            }

            return true;
        }
        #endregion
    }
}
