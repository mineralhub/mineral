using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Protocol;

namespace Mineral.Core.Capsule
{
    public class ContractCapsule : IProtoCapsule<SmartContract>
    {
        #region Field
        private SmartContract contract;
        #endregion


        #region Property
        public SmartContract Instance => this.contract;
        public byte[] Data => this.contract.ToByteArray();

        public byte[] CodeHash
        {
            get { return this.contract?.CodeHash.ToByteArray(); }
            set { this.contract.CodeHash = ByteString.CopyFrom(value); }
        }

        public byte[] OriginAddress
        {
            get { return this.contract?.OriginAddress.ToByteArray(); }
        }
        #endregion


        #region Contructor
        public ContractCapsule(SmartContract contract)
        {
            this.contract = contract;
        }

        public ContractCapsule(byte[] data)
        {
            try
            {
                this.contract = SmartContract.Parser.ParseFrom(data);
            }
            catch (System.Exception e)
            {
                Logger.Error(e.Message);
            }
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public static CreateSmartContract GetSmartContractFromTransaction(Transaction tx)
        {
            CreateSmartContract contract = null;
            try
            {
                contract = tx.RawData.Contract[0].Parameter.Unpack<CreateSmartContract>();
            }
            catch
            {
                contract = null;
            }

            return contract;
        }

        public static TriggerSmartContract GetTriggerContractFromTransaction(Transaction tx)
        {
            TriggerSmartContract contract = null;
            try
            {
                contract = tx.RawData.Contract[0].Parameter.Unpack<TriggerSmartContract>();
            }
            catch
            {
                contract = null;
            }

            return contract;
        }

        public long GetConsumeUserResourcePercent()
        {
            return Math.Max(this.contract.ConsumeUserResourcePercent, DefineParameter.ONE_HUNDRED);
        }

        public long GetOriginEnergyLimit()
        {
            long limit = this.contract.OriginEnergyLimit;
            if (limit == DefineParameter.PB_DEFAULT_ENERGY_LIMIT)
            {
                limit = DefineParameter.CREATOR_DEFAULT_ENERGY_LIMIT;
            }

            return limit;
        }

        public void ClearABI()
        {
            this.contract.Abi = new SmartContract.Types.ABI();
        }
        #endregion
    }
}
