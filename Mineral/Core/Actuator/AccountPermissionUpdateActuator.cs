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
    public class AccountPermissionUpdateActuator : AbstractActuator
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public AccountPermissionUpdateActuator(Any contract, Manager db_manager) : base(contract, db_manager) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        private bool CheckPermission(Permission permission)
        {
            if (permission.Keys.Count > this.db_manager.DynamicProperties.GetTotalSignNum())
            {
                throw new ContractValidateException(
                    "number of keys in permission should not be greater than " + this.db_manager.DynamicProperties.GetTotalSignNum());
            }
            if (permission.Keys.Count == 0)
                throw new ContractValidateException("key's count should be greater than 0");

            if (permission.Type == Permission.Types.PermissionType.Witness && permission.Keys.Count != 1)
                throw new ContractValidateException("Witness permission's key count should be 1");

            if (permission.Threshold <= 0)
                throw new ContractValidateException("permission's threshold should be greater than 0");

            string name = permission.PermissionName;
            if (name != null && name.Length > 0 && name.Length > 32)
                throw new ContractValidateException("permission's name is too long");

            if (permission.ParentId != 0)
                throw new ContractValidateException("permission's parent should be owner");

            long weight_sum = 0;
            List<ByteString> addresses = permission.Keys.Select(x => x.Address)
                                                        .Distinct()
                                                        .ToList();

            if (addresses.Count != permission.Keys.Count)
            {
                throw new ContractValidateException(
                    "address should be distinct in permission " + permission.Type);
            }

            foreach (Key key in permission.Keys)
            {
                if (!Wallet.AddressValid(key.Address.ToByteArray()))
                {
                    throw new ContractValidateException("key is not a validate address");
                }
                if (key.Weight <= 0)
                {
                    throw new ContractValidateException("key's weight should be greater than 0");
                }
                try
                {
                    weight_sum = weight_sum + key.Weight;
                }
                catch (ArithmeticException e)
                {
                    throw new ContractValidateException(e.Message);
                }
            }
            if (weight_sum < permission.Threshold)
            {
                throw new ContractValidateException(
                    "sum of all key's weight should not be less than threshold in permission " + permission.Type);
            }

            ByteString operations = permission.Operations;
            if (permission.Type != Permission.Types.PermissionType.Active)
            {
                if (!operations.IsEmpty)
                {
                    throw new ContractValidateException(
                        permission.Type + " permission needn't operations");
                }
                return true;
            }

            if (operations.IsEmpty || operations.Length != 32)
                throw new ContractValidateException("operations size must 32");

            byte[] types1 = this.db_manager.DynamicProperties.GetAvailableContractType();
            for (int i = 0; i < 256; i++)
            {
                bool b = (operations.ElementAt(i / 8) & (1 << (i % 8))) != 0;
                bool t = ((types1[(i / 8)] & 0xff) & (1 << (i % 8))) != 0;
                if (b && !t)
                {
                    throw new ContractValidateException(i + " isn't a validate ContractType");
                }
            }
            return true;
        }
        #endregion


        #region External Method
        public override long CalcFee()
        {
            return this.db_manager.DynamicProperties.GetUpdateAccountPermissionFee();
        }

        public override bool Execute(TransactionResultCapsule result)
        {
            long fee = CalcFee();
            AccountPermissionUpdateContract apu_contract = null;

            try
            {
                apu_contract = contract.Unpack<AccountPermissionUpdateContract>();
                byte[] owner_address = apu_contract.OwnerAddress.ToByteArray();

                AccountCapsule account = this.db_manager.Account.Get(owner_address);
                account.UpdatePermissions(apu_contract.Owner, apu_contract.Witness, new List<Permission>(apu_contract.Actives));
                this.db_manager.Account.Put(owner_address, account);

                this.db_manager.AdjustBalance(owner_address, -fee);
                this.db_manager.AdjustBalance(this.db_manager.Account.GetBlackHole().CreateDatabaseKey(), fee);

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
            return contract.Unpack<AccountPermissionUpdateContract>().OwnerAddress;
        }

        public override bool Validate()
        {
            if (this.contract == null)
                throw new ContractValidateException("No contract!");

            if (this.db_manager == null)
                throw new ContractValidateException("No this.db_manager!");

            if (this.db_manager.DynamicProperties.GetAllowMultiSign() != 1)
            {
                throw new ContractValidateException(
                    "multi sign is not allowed, need to be opened by the committee");
            }
            if (!this.contract.Is(AccountPermissionUpdateContract.Descriptor))
            {
                AccountPermissionUpdateContract apu_contract = null;

                try
                {
                    apu_contract = contract.Unpack<AccountPermissionUpdateContract>();
                }
                catch (InvalidProtocolBufferException e)
                {
                    Logger.Debug(e.Message);
                    throw new ContractValidateException(e.Message);
                }

                byte[] owner_address = apu_contract.OwnerAddress.ToByteArray();
                if (!Wallet.AddressValid(owner_address))
                {
                    throw new ContractValidateException("invalidate owner_address");
                }
                AccountCapsule accountCapsule = this.db_manager.Account.Get(owner_address);
                if (accountCapsule == null)
                {
                    throw new ContractValidateException("owner_address account does not exist");
                }

                if (apu_contract.Owner == null)
                {
                    throw new ContractValidateException("owner permission is missed");
                }

                if (accountCapsule.IsWitness)
                {
                    if (apu_contract.Witness == null)
                    {
                        throw new ContractValidateException("witness permission is missed");
                    }
                }
                else
                {
                    if (apu_contract.Witness != null)
                    {
                        throw new ContractValidateException("account isn't witness can't set witness permission");
                    }
                }

                if (apu_contract.Actives.Count == 0)
                    throw new ContractValidateException("active permission is missed");

                if (apu_contract.Actives.Count > 8)
                    throw new ContractValidateException("active permission is too many");

                Permission owner = apu_contract.Owner;
                Permission witness = apu_contract.Witness;

                if (owner.Type != Permission.Types.PermissionType.Owner)
                    throw new ContractValidateException("owner permission type is error");

                if (!CheckPermission(owner))
                {
                    return false;
                }
                if (accountCapsule.IsWitness)
                {
                    if (witness.Type != Permission.Types.PermissionType.Witness)
                    {
                        throw new ContractValidateException("witness permission type is error");
                    }
                    if (!CheckPermission(witness))
                    {
                        return false;
                    }
                }
                foreach (Permission permission in apu_contract.Actives)
                {
                    if (permission.Type != Permission.Types.PermissionType.Active)
                    {
                        throw new ContractValidateException("active permission type is error");
                    }
                    if (!CheckPermission(permission))
                    {
                        return false;
                    }
                }
            }
            else
            {
                throw new ContractValidateException(
                    "contract type error,expected type [AccountPermissionUpdateContract],real type[" + contract.GetType().Name + "]");
            }

            return true;
        }
        #endregion
    }
}
