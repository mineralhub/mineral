using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.LogsFilter.Trigger;
using Mineral.Common.Storage;
using Mineral.Common.Utils;
using Mineral.Core;
using Mineral.Core.Capsule;
using Mineral.Core.Service;
using Mineral.Utils;
using static Protocol.SmartContract.Types;

namespace Mineral.Common.Runtime.VM
{
    public class LogInfoTriggerParser
    {
        #region Field
        private long block_num = 0;
        private long block_timestamp = 0;
        private string transaction_id = "";
        private string origin_address = "";
        #endregion


        #region Property
        #endregion


        #region Contructor
        public LogInfoTriggerParser(long block_num, long block_timestamp, byte[] tx_id, byte[] origin_address)
        {
            this.block_num = block_num;
            this.block_timestamp = block_timestamp;
            this.transaction_id = tx_id.IsNotNullOrEmpty() ? tx_id.ToHexString() : ""; 
            this.origin_address = origin_address.IsNotNullOrEmpty() ? Wallet.AddressToBase58(origin_address) : "";

        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public List<ContractTrigger> ParseLogInfos(List<LogInfo> log_infos, IDeposit deposit)
        {
            List<ContractTrigger> triggers = new List<ContractTrigger>();
            if (log_infos == null || log_infos.Count <= 0)
            {
                return triggers;
            }

            Dictionary<string, string> signs = new Dictionary<string, string>();
            Dictionary<string, string> abis = new Dictionary<string, string>();

            foreach (LogInfo info in log_infos)
            {

                byte[] contract_address = Wallet.ToAddAddressPrefix(info.Address);
                string contract_address_str = contract_address.IsNotNullOrEmpty() ?
                    Wallet.AddressToBase58(contract_address) : "";


                if (signs.TryGetValue(contract_address_str, out _) == false)
                    continue;

                ContractCapsule contract = deposit.GetContract(contract_address);
                if (contract == null)
                {
                    signs.Add(contract_address_str, origin_address);
                    abis.Add(contract_address_str, "");
                    continue;
                }

                ABI abi = contract.Instance.Abi;
                string creator_address = Wallet.AddressToBase58(Wallet.ToAddAddressPrefix(contract.Instance.OriginAddress.ToByteArray()));
                signs.Add(contract_address_str, creator_address);

                if (abi != null && abi.Entrys.Count > 0)
                {
                    abis.Add(contract_address_str, JsonFormat.PrintToString(abi, false));
                }
                else
                {
                    abis.Add(contract_address_str, "");
                }
            }

            int index = 1;
            foreach (LogInfo info in log_infos)
            {
                byte[] contract_address = Wallet.ToAddAddressPrefix(info.Address);
                string contract_address_str = contract_address.IsNotNullOrEmpty() ? Wallet.AddressToBase58(contract_address) : "";

                string abi_value = abis[contract_address_str];
                ContractTrigger trigger = new ContractTrigger();
                string creator_address = signs[contract_address_str];

                trigger.UniqueId = this.transaction_id + "_" + index;
                trigger.TransactionId = this.transaction_id;
                trigger.ContractAddress = contract_address_str;
                trigger.OriginAddress = this.origin_address;
                trigger.CallerAddress = "";
                trigger.CreatorAddress = creator_address.IsNotNullOrEmpty() ? creator_address : "";
                trigger.BlockNumber = this.block_num;
                trigger.Timestamp = this.block_timestamp;
                trigger.LogInfo = info;
                trigger.AbiString = abi_value;

                triggers.Add(trigger);
                index++;
        }

        return triggers;
  }

    public static string GetEntrySignature(ABI.Types.Entry entry)
    {
        string signature = entry.Name + "(";
        StringBuilder builder = new StringBuilder();

        foreach (ABI.Types.Entry.Types.Param param in entry.Inputs)
        {
            if (builder.Length > 0)
            {
                builder.Append(",");
            }
            builder.Append(param.Type);
        }
        signature += builder.ToString() + ")";

        return signature;
    }
    #endregion
}
}
