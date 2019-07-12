using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Core.Capsule;
using Mineral.Utils;
using Protocol;
using static Protocol.Transaction.Types;

namespace Mineral.Core.Database
{
    public class ContractStore : MineralStoreWithRevoking<ContractCapsule, SmartContract>
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        public ContractStore(string db_name = "contract") : base(db_name) { }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override ContractCapsule Get(byte[] key)
        {
            return GetUnchecked(key);
        }

        public SmartContract.Types.ABI GetABI(byte[] contract_address)
        {
            byte[] value = this.revoking_db.GetUnchecked(contract_address);
            if (value.IsNotNullOrEmpty())
                return null;

            ContractCapsule contract = new ContractCapsule(value);

            return contract.Instance != null ? contract.Instance.Abi : null;
        }
        #endregion
    }
}
