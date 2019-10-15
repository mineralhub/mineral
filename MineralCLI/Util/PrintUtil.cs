using Google.Protobuf;
using Mineral;
using Mineral.Common.Utils;
using Mineral.Core;
using Mineral.Utils;
using Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using static Protocol.Account.Types;
using static Protocol.AssetIssueContract.Types;
using static Protocol.Transaction.Types;
using static Protocol.Transaction.Types.Contract.Types;

namespace MineralCLI.Util
{
    public static class PrintUtil
    {
        #region block

        public static string PrintBlockRowData(BlockHeader.Types.raw raw)
        {
            string result = "";

            result += "timestamp: ";
            result += raw.Timestamp.ToDateTime().ToLocalTime();
            result += "\n";

            result += "txTrieRoot: ";
            result += raw.TxTrieRoot.ToHexString();
            result += "\n";

            result += "parentHash: ";
            result += raw.ParentHash.ToHexString();
            result += "\n";

            result += "number: ";
            result += raw.Number;
            result += "\n";

            result += "witness_id: ";
            result += raw.WitnessId;
            result += "\n";

            result += "witness_address: ";
            result += Wallet.Encode58Check(raw.WitnessAddress.ToByteArray());
            result += "\n";

            result += "version: ";
            result += raw.Version;
            result += "\n";

            return result;
        }

        public static string PrintBlockHeader(BlockHeader block_header)
        {
            string result = "";
            result += "raw_data: ";
            result += "\n";
            result += "{";
            result += "\n";
            result += PrintBlockRowData(block_header.RawData);
            result += "}";
            result += "\n";

            result += "witness_signature: ";
            result += "\n";
            result += block_header.WitnessSignature.ToHexString();
            result += "\n";
            return result;
        }

        public static string PrintBlockExtention(BlockExtention block)
        {
            string result = "\n";
            if (block.Blockid != null)
            {
                result += "block_id: ";
                result += "\n";
                result += "{";
                result += "\n";
                result += block.Blockid.ToHexString();
                result += "\n";
                result += "}";
                result += "\n";
            }
            if (block.BlockHeader != null)
            {
                result += "block_header: ";
                result += "\n";
                result += "{";
                result += "\n";
                result += PrintBlockHeader(block.BlockHeader);
                result += "}";
                result += "\n";
            }
            if (block.Transactions.Count > 0)
            {
                result += PrintTransactionExtentionList(new List<TransactionExtention>(block.Transactions));
            }
            return result;
        }

        public static string PrintBlockListExtention(BlockListExtention blocks)
        {
            string result = "\n";
            int i = 0;
            foreach (BlockExtention block in blocks.Block)
            {
                result += "block " + i + " :::";
                result += "\n";
                result += "[";
                result += "\n";
                result += PrintBlockExtention(block);
                result += "]";
                result += "\n";
                result += "\n";
                i++;
            }
            return result;
        }
        #endregion

        #region Wallet
        public static string PrintAccount(Account account)
        {
            string result = "";
            result += "address: ";
            result += Wallet.Encode58Check(account.Address.ToByteArray());
            result += "\n";
            if (account.AccountId != null && !account.AccountId.IsEmpty)
            {
                result += "account_id: ";
                result += Encoding.UTF8.GetString(account.AccountId.ToByteArray());
                result += "\n";
            }
            if (account.AccountName != null && !account.AccountName.IsEmpty)
            {
                result += "account_name: ";
                result += Encoding.UTF8.GetString(account.AccountName.ToByteArray());
                result += "\n";
            }

            result += "type: ";
            result += account.Type.ToString();
            result += "\n";
            result += "balance: ";
            result += account.Balance;
            result += "\n";
            if (account.Frozen.Count > 0)
            {
                foreach (Frozen frozen in account.Frozen)
                {
                    result += "frozen";
                    result += "\n";
                    result += "{";
                    result += "\n";
                    result += "  frozen_balance: ";
                    result += frozen.FrozenBalance;
                    result += "\n";
                    result += "  expire_time: ";
                    result += frozen.ExpireTime.ToDateTime().ToLocalTime();
                    result += "\n";
                    result += "}";
                    result += "\n";
                }
            }
            result += "free_net_usage: ";
            result += account.FreeNetUsage;
            result += "\n";
            result += "net_usage: ";
            result += account.NetUsage;
            result += "\n";
            if (account.CreateTime != 0)
            {
                result += "create_time: ";
                result += account.CreateTime.ToDateTime().ToLocalTime();
                result += "\n";
            }
            if (account.Votes.Count > 0)
            {
                foreach (Vote vote in account.Votes)
                {
                    result += "votes";
                    result += "\n";
                    result += "{";
                    result += "\n";
                    result += "  vote_address: ";
                    result += Wallet.Encode58Check(vote.VoteAddress.ToByteArray());
                    result += "\n";
                    result += "  vote_count: ";
                    result += vote.VoteCount;
                    result += "\n";
                    result += "}";
                    result += "\n";
                }
            }
            if (account.Asset.Count > 0)
            {
                foreach (string name in account.Asset.Keys)
                {
                    result += "asset";
                    result += "\n";
                    result += "{";
                    result += "\n";
                    result += "  name: ";
                    result += name;
                    result += "\n";
                    result += "  balance: ";
                    result += account.Asset[name];
                    result += "\n";
                    result += "  latest_asset_operation_time: ";
                    result += account.LatestAssetOperationTime[name];
                    result += "\n";
                    result += "  free_asset_net_usage: ";
                    result += account.FreeAssetNetUsage[name];
                    result += "\n";
                    result += "}";
                    result += "\n";
                }
            }
            result += "asset issued id:";
            result += account.AssetIssuedID.ToStringUtf8();
            result += "\n";
            if (account.AssetV2.Count > 0)
            {
                foreach (string id in account.AssetV2.Keys)
                {
                    result += "assetV2";
                    result += "\n";
                    result += "{";
                    result += "\n";
                    result += "  id: ";
                    result += id;
                    result += "\n";
                    result += "  balance: ";
                    result += account.AssetV2[id];
                    result += "\n";
                    result += "  latest_asset_operation_timeV2: ";
                    result += account.LatestAssetOperationTimeV2[id];
                    result += "\n";
                    result += "  free_asset_net_usageV2: ";
                    result += account.FreeAssetNetUsageV2[id];
                    result += "\n";
                    result += "}";
                    result += "\n";
                }
            }
            if (account.FrozenSupply.Count > 0)
            {
                foreach (Frozen frozen in account.FrozenSupply)
                {
                    result += "frozen_supply";
                    result += "\n";
                    result += "{";
                    result += "\n";
                    result += "  amount: ";
                    result += frozen.FrozenBalance;
                    result += "\n";
                    result += "  expire_time: ";
                    result += frozen.ExpireTime.ToDateTime().ToLocalTime();
                    result += "\n";
                    result += "}";
                    result += "\n";
                }
            }
            result += "latest_opration_time: ";
            result += account.LatestOprationTime.ToDateTime().ToLocalTime();
            result += "\n";

            result += "latest_consume_time: ";
            result += account.LatestConsumeTime;
            result += "\n";

            result += "latest_consume_free_time: ";
            result += account.LatestConsumeFreeTime;
            result += "\n";

            result += "allowance: ";
            result += account.Allowance;
            result += "\n";

            result += "latest_withdraw_time: ";
            result += account.LatestWithdrawTime.ToDateTime().ToLocalTime();
            result += "\n";

            result += "is_witness: ";
            result += account.IsWitness;
            result += "\n";

            result += "AssetIssuedName: ";
            result += account.AssetIssuedName.ToStringUtf8();
            result += "\n";
            result += "AccountResource: {\n";
            result += PrintAccountResource(account.AccountResource);
            result += "\n";
            result += "AcquiredDelegatedFrozenBalanceForBandwidth: ";
            result += account.AcquiredDelegatedFrozenBalanceForBandwidth;
            result += "\n";
            result += "delegatedFrozenBalanceForBandwidth: ";
            result += account.DelegatedFrozenBalanceForBandwidth;
            result += "\n";
            if (account.AccountResource != null)
            {
                result += "AcquiredDelegatedFrozenBalanceForEnergy: ";
                result += account.AccountResource.AcquiredDelegatedFrozenBalanceForEnergy;
                result += "\n";
                result += "DelegatedFrozenBalanceForEnergy: ";
                result += account.AccountResource.DelegatedFrozenBalanceForEnergy;
                result += "}\n";
            }

            if (account.OwnerPermission != null)
            {
                result += "owner_permission: ";
                result += "\n";
                result += "{";
                result += "\n";
                result += PrintPermission(account.OwnerPermission);
                result += "\n";
                result += "}";
                result += "\n";
            }

            if (account.WitnessPermission != null)
            {
                result += "witness_permission: ";
                result += "\n";
                result += "{";
                result += "\n";
                result += PrintPermission(account.WitnessPermission);
                result += "\n";
                result += "}";
                result += "\n";
            }

            if (account.ActivePermission.Count > 0)
            {
                result += "active_permissions: ";
                result += PrintPermissionList(new List<Permission>(account.ActivePermission));
            }

            return result;
        }

        public static string PrintAccountResource(AccountResource account_resource)
        {
            if (account_resource == null)
                return "";

            string result = "";
            result += "energy_usage: ";
            result += account_resource.EnergyUsage;
            result += "\n";

            if (account_resource.FrozenBalanceForEnergy != null)
            {
                result += "frozen_balance_for_energy: ";
                result += "{";
                result += "\n";
                result += "  amount: ";
                result += account_resource.FrozenBalanceForEnergy.FrozenBalance;
                result += "\n";
                result += "  expire_time: ";
                result += account_resource.FrozenBalanceForEnergy.ExpireTime.ToDateTime().ToLocalTime();
                result += "\n";
                result += "}";
                result += "\n";
            }

            result += "latest_consume_time_for_energy: ";
            result += account_resource.LatestConsumeTimeForEnergy;
            result += "\n";
            result += "storage_limit: ";
            result += account_resource.StorageLimit;
            result += "\n";
            result += "storage_usage: ";
            result += account_resource.StorageUsage;
            result += "\n";
            result += "latest_exchange_storage_time: ";
            result += account_resource.LatestExchangeStorageTime;
            result += "\n";
            return result;
        }

        public static string PrintWitness(Witness witness)
        {
            string result = "";
            result += "address: ";
            result += Wallet.AddressToBase58(witness.Address.ToByteArray());
            result += "\n";
            result += "voteCount: ";
            result += witness.VoteCount;
            result += "\n";
            result += "pubKey: ";
            result += witness.PubKey.ToByteArray().ToHexString();
            result += "\n";
            result += "url: ";
            result += witness.Url;
            result += "\n";
            result += "totalProduced: ";
            result += witness.TotalProduced;
            result += "\n";
            result += "totalMissed: ";
            result += witness.TotalMissed;
            result += "\n";
            result += "latestBlockNum: ";
            result += witness.LatestBlockNum;
            result += "\n";
            result += "latestSlotNum: ";
            result += witness.LatestSlotNum;
            result += "\n";
            result += "isJobs: ";
            result += witness.IsJobs;
            result += "\n";
            return result;
        }

        public static string PrintWitnessList(WitnessList witnesses)
        {
            string result = "\n";
            int i = 0;
            foreach (Witness witness in witnesses.Witnesses)
            {
                result += "witness " + i + " :::";
                result += "\n";
                result += "[";
                result += "\n";
                result += PrintWitness(witness);
                result += "]";
                result += "\n";
                result += "\n";
                i++;
            }
            return result;
        }
        #endregion

        #region Transaction
        public static string PrintTransaction(Transaction transaction)
        {
            string result = "";

            result += "hash: ";
            result += "\n";
            result += SHA256Hash.ToHash(transaction.ToByteArray()).ToHexString();
            result += "\n";
            result += "txid: ";
            result += "\n";
            result += SHA256Hash.ToHash(transaction.RawData.ToByteArray());
            result += "\n";

            if (transaction.RawData != null)
            {
                result += "raw_data: ";
                result += "\n";
                result += "{";
                result += "\n";
                result += PrintTransactionRaw(transaction.RawData);
                result += "}";
                result += "\n";
            }
            if (transaction.Signature.Count > 0)
            {
                result += "signature: ";
                result += "\n";
                result += "{";
                result += "\n";
                result += PrintSignature(new List<ByteString>(transaction.Signature));
                result += "}";
                result += "\n";
            }
            if (transaction.Ret.Count != 0)
            {
                result += "ret: ";
                result += "\n";
                result += "{";
                result += "\n";
                result += PrintRet(new List<Result>(transaction.Ret));
                result += "}";
                result += "\n";
            }
            return result;
        }

        public static string PrintTransaction(TransactionExtention extension)
        {
            string result = "";
            result += "txid: ";
            result += "\n";
            result += extension.Txid.ToByteArray();
            result += "\n";

            Transaction transaction = extension.Transaction;
            if (transaction.RawData != null)
            {
                result += "raw_data: ";
                result += "\n";
                result += "{";
                result += "\n";
                result += PrintTransactionRaw(transaction.RawData);
                result += "}";
                result += "\n";
            }
            if (transaction.Signature.Count > 0)
            {
                result += "signature: ";
                result += "\n";
                result += "{";
                result += "\n";
                result += PrintSignature(new List<ByteString>(transaction.Signature));
                result += "}";
                result += "\n";
            }
            if (transaction.Ret.Count != 0)
            {
                result += "ret: ";
                result += "\n";
                result += "{";
                result += "\n";
                result += PrintRet(new List<Result>(transaction.Ret));
                result += "}";
                result += "\n";
            }
            return result;
        }

        public static string PrintTransactionRaw(Transaction.Types.raw raw)
        {
            string result = "";

            if (raw.RefBlockBytes != null)
            {
                result += "ref_block_bytes: ";
                result += raw.RefBlockBytes.ToByteArray().ToHexString();
                result += "\n";
            }

            if (raw.RefBlockHash != null)
            {
                result += "ref_block_hash: ";
                result += raw.RefBlockHash.ToByteArray().ToHexString();
                result += "\n";
            }

            if (raw.Contract.Count > 0)
            {
                result += "contract: ";
                result += "\n";
                result += "{";
                result += "\n";
                result += PrintContractList(new List<Contract>(raw.Contract));
                result += "}";
                result += "\n";
            }

            result += "timestamp: ";
            result += raw.Timestamp.ToDateTime().ToLocalTime();
            result += "\n";

            result += "fee_limit: ";
            result += raw.FeeLimit;
            result += "\n";

            return result;
        }

        public static string PrintTransactionExtentionList(List<TransactionExtention> transactions)
        {
            string result = "\n";
            int i = 0;
            foreach (TransactionExtention transaction in transactions)
            {
                result += "transaction " + i + " :::";
                result += "\n";
                result += "[";
                result += "\n";
                result += PrintTransaction(transaction);
                result += "]";
                result += "\n";
                result += "\n";
                i++;
            }
            return result;
        }

        public static string PrintTransactionApprovedList(TransactionApprovedList transaction_list)
        {
            string result = "";
            result += "result:";
            result += "\n";
            result += "{";
            result += "\n";
            result += PrintResult(transaction_list.Result);
            result += "}";
            result += "\n";
            if (transaction_list.ApprovedList.Count > 0)
            {
                result += "approved_list:";
                result += "\n";
                result += "[";
                result += "\n";
                foreach (ByteString approved in transaction_list.ApprovedList)
                {
                    result += Wallet.AddressToBase58(approved.ToByteArray());
                    result += "\n";
                }
                result += "]";
                result += "\n";
            }
            result += "transaction:";
            result += "\n";
            result += "{";
            result += "\n";
            result += PrintTransaction(transaction_list.Transaction);
            result += "}";
            result += "\n";
            return result;
        }

        public static string PrintTransactionInfo(TransactionInfo transaction_info)
        {
            string result = "";
            result += "txid: ";
            result += "\n";
            result += transaction_info.Id.ToByteArray().ToHexString();
            result += "\n";
            result += "fee: ";
            result += "\n";
            result += transaction_info.Fee;
            result += "\n";
            result += "blockNumber: ";
            result += "\n";
            result += transaction_info.BlockNumber;
            result += "\n";
            result += "blockTimeStamp: ";
            result += "\n";
            result += transaction_info.BlockTimeStamp;
            result += "\n";
            result += "result: ";
            result += "\n";
            if (transaction_info.Result == TransactionInfo.Types.code.Sucess)
            {
                result += "SUCCESS";
            }
            else
            {
                result += "FAILED";
            }
            result += "\n";
            result += "resMessage: ";
            result += "\n";
            result += transaction_info.ResMessage.ToStringUtf8();
            result += "\n";
            result += "contractResult: ";
            result += "\n";
            result += transaction_info.ContractResult[0].ToByteArray().ToHexString();
            result += "\n";
            result += "contractAddress: ";
            result += "\n";
            result += Wallet.AddressToBase58(transaction_info.ContractAddress.ToByteArray());
            result += "\n";
            result += "logList: ";
            result += "\n";
            result += PrintLogList(new List<TransactionInfo.Types.Log>(transaction_info.Log));
            result += "\n";
            result += "receipt: ";
            result += "\n";
            result += PrintReceipt(transaction_info.Receipt);
            result += "\n";
            if (transaction_info.UnfreezeAmount != 0)
            {
                result += "UnfreezeAmount: ";
                result += transaction_info.UnfreezeAmount;
                result += "\n";
            }
            if (transaction_info.WithdrawAmount != 0)
            {
                result += "WithdrawAmount: ";
                result += transaction_info.WithdrawAmount;
                result += "\n";
            }
            if (transaction_info.ExchangeReceivedAmount != 0)
            {
                result += "ExchangeReceivedAmount: ";
                result += transaction_info.ExchangeReceivedAmount;
                result += "\n";
            }
            if (transaction_info.ExchangeInjectAnotherAmount != 0)
            {
                result += "ExchangeInjectAnotherAmount: ";
                result += transaction_info.ExchangeInjectAnotherAmount;
                result += "\n";
            }
            if (transaction_info.ExchangeWithdrawAnotherAmount != 0)
            {
                result += "ExchangeWithdrawAnotherAmount: ";
                result += transaction_info.ExchangeWithdrawAnotherAmount;
                result += "\n";
            }
            if (transaction_info.ExchangeId != 0)
            {
                result += "ExchangeId: ";
                result += transaction_info.ExchangeId;
                result += "\n";
            }
            result += "InternalTransactionList: ";
            result += "\n";
            result += PrintInternalTransactionList(new List<InternalTransaction>(transaction_info.InternalTransactions));
            result += "\n";
            return result;
        }

        public static string PrintInternalTransactionList(List<InternalTransaction> internal_transactions)
        {
            string result = "";
            foreach (var internal_transaction in internal_transactions)
            {
                result += "[\n";
                result += "  hash:\n";
                result += "  " + internal_transaction.Hash.ToByteArray().ToHexString();
                result += "  \n";
                result += "  caller_address:\n";
                result += "  " + internal_transaction.CallerAddress.ToByteArray().ToHexString();
                result += "  \n";
                result += "  transfer to_address:\n";
                result += "  " + internal_transaction.TransferToAddress.ToByteArray().ToHexString();
                result += "  \n";
                result += "  CallValueInfo:\n";
                string callValueInfo = "";

                foreach (var token in internal_transaction.CallValueInfo)
                {
                    callValueInfo += "  [\n";
                    callValueInfo += "    TokenName(Default trx):\n";
                    if (null == token.TokenId || token.TokenId.Length == 0)
                    {
                        callValueInfo += "    TRX(SUN)";
                    }
                    else
                    {
                        callValueInfo += "    " + token.TokenId;
                    }
                    callValueInfo += "    \n";
                    callValueInfo += "    callValue:\n";
                    callValueInfo += "    " + token.CallValue;
                    callValueInfo += "  \n";
                    callValueInfo += "  ]\n";
                    callValueInfo += "    \n";
                }
                result += callValueInfo;
                result += "  note:\n";
                result += "  " + Encoding.UTF8.GetString(internal_transaction.Note.ToByteArray());
                result += "  \n";
                result += "  rejected:\n";
                result += "  " + internal_transaction.Rejected;
                result += "  \n";
                result += "]\n";
            }
            return result;
        }

        public static string PrintTransactionSignWeight(TransactionSignWeight weight)
        {
            string result = "";

            result += "permission:";
            result += "\n";
            result += "{";
            result += "\n";
            result += PrintPermission(weight.Permission);
            result += "}";
            result += "\n";
            result += "current_weight: ";
            result += weight.CurrentWeight;
            result += "\n";
            result += "result:";
            result += "\n";
            result += "{";
            result += "\n";
            result += PrintResult(weight.Result);
            result += "}";
            result += "\n";
            if (weight.ApprovedList.Count > 0)
            {
                result += "approved_list:";
                result += "\n";
                result += "[";
                result += "\n";
                foreach (ByteString approved in weight.ApprovedList)
                {
                    result += Wallet.Encode58Check(approved.ToByteArray());
                    result += "\n";
                }
                result += "]";
                result += "\n";
            }
            result += "transaction:";
            result += "\n";
            result += "{";
            result += "\n";
            result += PrintTransaction(weight.Transaction);
            result += "}";
            result += "\n";

            return result;
        }

        public static string PrintResult(TransactionApprovedList.Types.Result transaction_result)
        {
            string result ="";
            result += "code: ";
            result += transaction_result.Code;
            result += "\n";
            if (transaction_result.Message.IsNotNullOrEmpty())
            {
                result += "message: ";
                result += transaction_result.Message;
                result += "\n";
            }
            return result;
        }

        public static string PrintLogList(List<TransactionInfo.Types.Log> logs)
        {
            string result = "";
            foreach (var log in logs)
            {
                result += "address:\n";
                result += log.Address.ToByteArray().ToHexString();
                result += "\n";
                result += "data:\n";
                result += log.Data.ToByteArray().ToHexString();
                result += "\n";
                result += "TopicsList\n";
                string topics = "";

                foreach (var topic in log.Topics)
                {
                    topics += topic.ToByteArray().ToHexString();
                    topics += "\n";
                }
                result += topics;
            }
            return result;
        }

        public static string PrintReceipt(ResourceReceipt receipt)
        {
            string result = "";
            result += "EnergyUsage: ";
            result += "\n";
            result += receipt.EnergyUsage;
            result += "\n";
            result += "EnergyFee(SUN): ";
            result += "\n";
            result += receipt.EnergyFee;
            result += "\n";
            result += "OriginEnergyUsage: ";
            result += "\n";
            result += receipt.OriginEnergyUsage;
            result += "\n";
            result += "EnergyUsageTotal: ";
            result += "\n";
            result += receipt.EnergyUsageTotal;
            result += "\n";
            result += "NetUsage: ";
            result += "\n";
            result += receipt.NetUsage;
            result += "\n";
            result += "NetFee: ";
            result += "\n";
            result += receipt.NetFee;
            result += "\n";
            return result;
        }

        public static string PrintResult(TransactionSignWeight.Types.Result weight_result)
        {
            string result = "";
            result += "code: ";
            result += weight_result.Code;
            result += "\n";
            if (weight_result.Message != null && weight_result.Message.Length > 0)
            {
                result += "message: ";
                result += weight_result.Message;
                result += "\n";
            }
            return result;
        }

        public static string PrintContractList(List<Contract> contracts)
        {
            string result = "";
            int i = 0;
            foreach (Contract contract in contracts)
            {
                result += "contract " + i + " :::";
                result += "\n";
                result += "[";
                result += "\n";
                result += PrintContract(contract);
                result += "]";
                result += "\n";
                result += "\n";
                i++;
            }
            return result;
        }

        public static string PrintContract(Contract contract)
        {
            string result = "";
            try
            {
                result += "contract_type: ";
                result += contract.Type.ToString();
                result += "\n";

                switch (contract.Type)
                {
                    case ContractType.AccountCreateContract:
                        {
                            AccountCreateContract account_create_contract = contract.Parameter.Unpack<AccountCreateContract>();
                            result += "type: ";
                            result += account_create_contract.Type.ToString();
                            result += "\n";
                            if (account_create_contract.AccountAddress != null
                                && !account_create_contract.AccountAddress.IsEmpty)
                            {
                                result += "account_address: ";
                                result += Wallet.Encode58Check(account_create_contract.AccountAddress.ToByteArray());
                                result += "\n";
                            }
                            result += "owner_address: ";
                            result += Wallet.Encode58Check(account_create_contract.OwnerAddress.ToByteArray());
                            result += "\n";

                        }
                        break;
                    case ContractType.AccountUpdateContract:
                        {
                            AccountUpdateContract account_update_contract = contract.Parameter.Unpack<AccountUpdateContract>();
                            if (account_update_contract.AccountName != null
                                && !account_update_contract.AccountName.IsEmpty)
                            {
                                result += "account_name: ";
                                result += account_update_contract.AccountName.ToByteArray().ToHexString();
                                result += "\n";
                            }
                            result += "owner_address: ";
                            result += Wallet.Encode58Check(account_update_contract.OwnerAddress.ToByteArray());
                            result += "\n";
                        }
                        break;
                    case ContractType.TransferContract:
                        {
                            TransferContract transfer_contract = contract.Parameter.Unpack<TransferContract>();
                            result += "owner_address: ";
                            result += Wallet.Encode58Check(transfer_contract.OwnerAddress.ToByteArray());
                            result += "\n";
                            result += "to_address: ";
                            result += Wallet.Encode58Check(transfer_contract.ToAddress.ToByteArray());
                            result += "\n";
                            result += "amount: ";
                            result += transfer_contract.Amount;
                            result += "\n";
                        }
                        break;
                    case ContractType.TransferAssetContract:
                        {
                            TransferAssetContract trasfer_asset_contract = contract.Parameter.Unpack<TransferAssetContract>();
                            result += "asset_name: ";
                            result += trasfer_asset_contract.AssetName.ToByteArray().ToHexString();
                            result += "\n";
                            result += "owner_address: ";
                            result += Wallet.Encode58Check(trasfer_asset_contract.OwnerAddress.ToByteArray());
                            result += "\n";
                            result += "to_address: ";
                            result += Wallet.Encode58Check(trasfer_asset_contract.ToAddress.ToByteArray());
                            result += "\n";
                            result += "amount: ";
                            result += trasfer_asset_contract.Amount;
                            result += "\n";
                        }
                        break;
                    case ContractType.VoteAssetContract:
                        {
                            VoteAssetContract voteAssetContract = contract.Parameter.Unpack<VoteAssetContract>();
                        }
                        break;
                    case ContractType.VoteWitnessContract:
                        {
                            VoteWitnessContract vote_witness_contract = contract.Parameter.Unpack<VoteWitnessContract>();
                            result += "owner_address: ";
                            result += Wallet.Encode58Check(vote_witness_contract.OwnerAddress.ToByteArray());
                            result += "\n";
                            result += "votes: ";
                            result += "\n";
                            result += "{";
                            result += "\n";
                            foreach (VoteWitnessContract.Types.Vote vote in vote_witness_contract.Votes)
                            {
                                result += "[";
                                result += "\n";
                                result += "vote_address: ";
                                result += Wallet.Encode58Check(vote.VoteAddress.ToByteArray());
                                result += "\n";
                                result += "vote_count: ";
                                result += vote.VoteCount;
                                result += "\n";
                                result += "]";
                                result += "\n";
                            }
                            result += "}";
                            result += "\n";
                        }
                        break;
                    case ContractType.WitnessCreateContract:
                        {
                            WitnessCreateContract witness_create_contract = contract.Parameter.Unpack<WitnessCreateContract>();
                            result += "owner_address: ";
                            result += Wallet.Encode58Check(witness_create_contract.OwnerAddress.ToByteArray());
                            result += "\n";
                            result += "url: ";
                            result += Encoding.UTF8.GetString(witness_create_contract.Url.ToByteArray());
                            result += "\n";
                        }
                        break;
                    case ContractType.WitnessUpdateContract:
                        {
                            WitnessUpdateContract witness_update_contract = contract.Parameter.Unpack<WitnessUpdateContract>();
                            result += "owner_address: ";
                            result += Wallet.Encode58Check(witness_update_contract.OwnerAddress.ToByteArray());
                            result += "\n";
                            result += "url: ";
                            result += Encoding.UTF8.GetString(witness_update_contract.UpdateUrl.ToByteArray());
                            result += "\n";
                        }
                        break;
                    case ContractType.AssetIssueContract:
                        {
                            AssetIssueContract asset_issue_contract = contract.Parameter.Unpack<AssetIssueContract>();
                            result += PrintAssetIssue(asset_issue_contract);
                        }
                        break;
                    case ContractType.UpdateAssetContract:
                        {
                            UpdateAssetContract update_asset_contract = contract.Parameter.Unpack<UpdateAssetContract>();
                            result += "owner_address: ";
                            result += Wallet.Encode58Check(update_asset_contract.OwnerAddress.ToByteArray());
                            result += "\n";
                            result += "description: ";
                            result += Encoding.UTF8.GetString(update_asset_contract.Description.ToByteArray());
                            result += "\n";
                            result += "url: ";
                            result += Encoding.UTF8.GetString(update_asset_contract.Url.ToByteArray());
                            result += "\n";
                            result += "free asset net limit: ";
                            result += update_asset_contract.NewLimit;
                            result += "\n";
                            result += "public free asset net limit: ";
                            result += update_asset_contract.NewPublicLimit;
                            result += "\n";
                        }
                        break;
                    case ContractType.ParticipateAssetIssueContract:
                        {
                            ParticipateAssetIssueContract participate_asset_issue_contract =
                                contract.Parameter.Unpack<ParticipateAssetIssueContract>();
                            result += "asset_name: ";
                            result += Encoding.UTF8.GetString(participate_asset_issue_contract.AssetName.ToByteArray());
                            result += "\n";
                            result += "owner_address: ";
                            result += Wallet.Encode58Check(participate_asset_issue_contract.OwnerAddress.ToByteArray());
                            result += "\n";
                            result += "to_address: ";
                            result += Wallet.Encode58Check(participate_asset_issue_contract.ToAddress.ToByteArray());
                            result += "\n";
                            result += "amount: ";
                            result += participate_asset_issue_contract.Amount;
                            result += "\n";
                        }
                        break;
                    case ContractType.FreezeBalanceContract:
                        {
                            FreezeBalanceContract freeze_balance_contract = contract.Parameter.Unpack<FreezeBalanceContract>();
                            result += "owner_address: ";
                            result += Wallet.Encode58Check(freeze_balance_contract.OwnerAddress.ToByteArray());
                            result += "\n";
                            result += "receive_address: ";
                            result += Wallet.Encode58Check(freeze_balance_contract.ReceiverAddress.ToByteArray());
                            result += "\n";
                            result += "frozen_balance: ";
                            result += freeze_balance_contract.FrozenBalance;
                            result += "\n";
                            result += "frozen_duration: ";
                            result += freeze_balance_contract.FrozenDuration;
                            result += "\n";
                        }
                        break;
                    case ContractType.UnfreezeBalanceContract:
                        {
                            UnfreezeBalanceContract unfreeze_balance_contract = contract.Parameter.Unpack<UnfreezeBalanceContract>();
                            result += "owner_address: ";
                            result += Wallet.Encode58Check(unfreeze_balance_contract.OwnerAddress.ToByteArray());
                            result += "\n";
                            result += "receive_address: ";
                            result += Wallet.Encode58Check(unfreeze_balance_contract.ReceiverAddress.ToByteArray());
                            result += "\n";
                        }
                        break;
                    case ContractType.UnfreezeAssetContract:
                        {
                            UnfreezeAssetContract unfreeze_asset_contract = contract.Parameter
                                .Unpack<UnfreezeAssetContract>();
                            result += "owner_address: ";
                            result += Wallet.Encode58Check(unfreeze_asset_contract.OwnerAddress.ToByteArray());
                            result += "\n";
                        }
                        break;
                    case ContractType.WithdrawBalanceContract:
                        {
                            WithdrawBalanceContract withdraw_balance_contract = contract.Parameter.Unpack<WithdrawBalanceContract>();
                            result += "owner_address: ";
                            result += Wallet.Encode58Check(withdraw_balance_contract.OwnerAddress.ToByteArray());
                            result += "\n";
                        }
                        break;
                    case ContractType.SetAccountIdContract:
                        {
                            SetAccountIdContract set_accountid_contract = contract.Parameter.Unpack<SetAccountIdContract>();
                            result += "owner_address: ";
                            result += Wallet.Encode58Check(set_accountid_contract.OwnerAddress.ToByteArray());
                            result += "\n";
                            result += "account_id: ";
                            result += Encoding.UTF8.GetString(set_accountid_contract.AccountId.ToByteArray());
                            result += "\n";
                        }
                        break;
                    case ContractType.CreateSmartContract:
                        {
                            CreateSmartContract create_smart_contract = contract.Parameter.Unpack<CreateSmartContract>();
                            SmartContract new_contract = create_smart_contract.NewContract;
                            result += "owner_address: ";
                            result += Wallet.Encode58Check(create_smart_contract.OwnerAddress.ToByteArray());
                            result += "\n";
                            result += "ABI: ";
                            result += new_contract.Abi.ToString();
                            result += "\n";
                            result += "byte_code: ";
                            result += new_contract.Bytecode.ToByteArray().ToHexString();
                            result += "\n";
                            result += "call_value: ";
                            result += new_contract.CallValue;
                            result += "\n";
                            result += "contract_address:";
                            result += Wallet.Encode58Check(new_contract.ContractAddress.ToByteArray());
                            result += "\n";
                        }
                        break;
                    case ContractType.TriggerSmartContract:
                        {
                            TriggerSmartContract trigger_smart_contract = contract.Parameter
                                .Unpack<TriggerSmartContract>();
                            result += "owner_address: ";
                            result += Wallet.Encode58Check(trigger_smart_contract.OwnerAddress.ToByteArray());
                            result += "\n";
                            result += "contract_address: ";
                            result += Wallet.Encode58Check(trigger_smart_contract.ContractAddress.ToByteArray());
                            result += "\n";
                            result += "call_value:";
                            result += trigger_smart_contract.CallValue;
                            result += "\n";
                            result += "data:";
                            result += trigger_smart_contract.Data.ToByteArray().ToHexString();
                            result += "\n";
                        }
                        break;
                    case ContractType.ProposalCreateContract:
                        {
                            ProposalCreateContract proposal_create_contract = contract.Parameter.Unpack<ProposalCreateContract>();
                            result += "owner_address: ";
                            result += Wallet.Encode58Check(proposal_create_contract.OwnerAddress.ToByteArray());
                            result += "\n";
                            result += "parametersMap: ";
                            result += proposal_create_contract.Parameters;
                            result += "\n";
                        }
                        break;
                    case ContractType.ProposalApproveContract:
                        {
                            ProposalApproveContract proposal_approve_contract = contract.Parameter
                                .Unpack<ProposalApproveContract>();
                            result += "owner_address: ";
                            result += Wallet.Encode58Check(proposal_approve_contract.OwnerAddress.ToByteArray());
                            result += "\n";
                            result += "proposal id: ";
                            result += proposal_approve_contract.ProposalId;
                            result += "\n";
                            result += "IsAddApproval: ";
                            result += proposal_approve_contract.IsAddApproval;
                            result += "\n";
                        }
                        break;
                    case ContractType.ProposalDeleteContract:
                        {
                            ProposalDeleteContract proposal_delete_contract = contract.Parameter.Unpack<ProposalDeleteContract>();
                            result += "owner_address: ";
                            result += Wallet.Encode58Check(proposal_delete_contract.OwnerAddress.ToByteArray());
                        }
                        break;
                    case ContractType.ExchangeCreateContract:
                        {
                            ExchangeCreateContract exchange_create_contract = contract.Parameter.Unpack<ExchangeCreateContract>();
                            result += "owner_address: ";
                            result += Wallet.Encode58Check(exchange_create_contract.OwnerAddress.ToByteArray());
                            result += "\n";
                            result += "firstTokenId: ";
                            result += exchange_create_contract.FirstTokenId.ToStringUtf8();
                            result += "\n";
                            result += "firstTokenBalance: ";
                            result += exchange_create_contract.FirstTokenBalance;
                            result += "\n";
                            result += "secondTokenId: ";
                            result += exchange_create_contract.SecondTokenId.ToStringUtf8();
                            result += "\n";
                            result += "secondTokenBalance: ";
                            result += exchange_create_contract.SecondTokenBalance;
                            result += "\n";
                        }
                        break;
                    case ContractType.ExchangeInjectContract:
                        {
                            ExchangeInjectContract exchange_inject_contract = contract.Parameter.Unpack<ExchangeInjectContract>();
                            result += "owner_address: ";
                            result += Wallet.Encode58Check(exchange_inject_contract.OwnerAddress.ToByteArray());
                            result += "\n";
                            result += "TokenId: ";
                            result += exchange_inject_contract.TokenId.ToStringUtf8();
                            result += "\n";
                            result += "quant: ";
                            result += exchange_inject_contract.Quant;
                            result += "\n";
                        }
                        break;
                    case ContractType.ExchangeWithdrawContract:
                        {
                            ExchangeWithdrawContract exchange_withdraw_contract = contract.Parameter
                                .Unpack<ExchangeWithdrawContract>();
                            result += "owner_address: ";
                            result += Wallet.Encode58Check(exchange_withdraw_contract.OwnerAddress.ToByteArray());
                            result += "\n";
                            result += "TokenId: ";
                            result += exchange_withdraw_contract.TokenId.ToStringUtf8();
                            result += "\n";
                            result += "quant: ";
                            result += exchange_withdraw_contract.Quant;
                            result += "\n";
                        }
                        break;
                    case ContractType.ExchangeTransactionContract:
                        {
                            ExchangeTransactionContract exchange_transaction_contract = 
                                contract.Parameter.Unpack<ExchangeTransactionContract>();

                            result += "owner_address: ";
                            result += Wallet.Encode58Check(exchange_transaction_contract.OwnerAddress.ToByteArray());
                            result += "\n";
                            result += "TokenId: ";
                            result += exchange_transaction_contract.TokenId.ToStringUtf8();
                            result += "\n";
                            result += "quant: ";
                            result += exchange_transaction_contract.Quant;
                            result += "\n";
                        }
                        break;
                    case ContractType.AccountPermissionUpdateContract:
                        {
                            AccountPermissionUpdateContract account_permission_update_contract = 
                                contract.Parameter.Unpack<AccountPermissionUpdateContract>();

                            result += "owner_address: ";
                            result += Wallet.Encode58Check(account_permission_update_contract.OwnerAddress.ToByteArray());
                            result += "\n";
                            if (account_permission_update_contract.Owner != null)
                            {
                                result += "owner_permission: ";
                                result += "\n";
                                result += "{";
                                result += "\n";
                                result += PrintPermission(account_permission_update_contract.Owner);
                                result += "\n";
                                result += "}";
                                result += "\n";
                            }

                            if (account_permission_update_contract.Witness != null)
                            {
                                result += "witness_permission: ";
                                result += "\n";
                                result += "{";
                                result += "\n";
                                result += PrintPermission(account_permission_update_contract.Witness);
                                result += "\n";
                                result += "}";
                                result += "\n";
                            }

                            if (account_permission_update_contract.Actives.Count > 0)
                            {
                                result += "active_permissions: ";
                                result += PrintPermissionList(new List<Permission>(account_permission_update_contract.Actives));
                                result += "\n";
                            }
                        }
                        break;
                    case ContractType.UpdateSettingContract:
                        {
                            UpdateSettingContract update_setting_contract = contract.Parameter.Unpack<UpdateSettingContract>();
                            result += "owner_address: ";
                            result += Wallet.Encode58Check(update_setting_contract.OwnerAddress.ToByteArray());
                            result += "\n";
                            result += "contract_address: ";
                            result += Wallet.Encode58Check(update_setting_contract.ContractAddress.ToByteArray());
                            result += "\n";
                            result += "consume_user_resource_percent: ";
                            result += update_setting_contract.ConsumeUserResourcePercent;
                            result += "\n";
                        }
                        break;
                    case ContractType.UpdateEnergyLimitContract:
                        {
                            UpdateEnergyLimitContract update_energy_limit_contract =
                                contract.Parameter.Unpack<UpdateEnergyLimitContract>();
                            result += "owner_address: ";
                            result += Wallet.Encode58Check(update_energy_limit_contract.OwnerAddress.ToByteArray());
                            result += "\n";
                            result += "contract_address: ";
                            result += Wallet.Encode58Check(update_energy_limit_contract.ContractAddress.ToByteArray());
                            result += "\n";
                            result += "origin_energy_limit: ";
                            result += update_energy_limit_contract.OriginEnergyLimit;
                            result += "\n";
                        }
                        break;
                    case ContractType.ClearAbicontract:
                        {
                            ClearABIContract clear_abi_contract = contract.Parameter.Unpack<ClearABIContract>();
                            result += "owner_address: ";
                            result += Wallet.Encode58Check(clear_abi_contract.OwnerAddress.ToByteArray());
                            result += "\n";
                            result += "contract_address: ";
                            result += Wallet.Encode58Check(clear_abi_contract.ContractAddress.ToByteArray());
                            result += "\n";
                        }
                        break;
                    default:
                        return "";
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return "";
            }

            return result;
        }

        public static string PrintPermission(Permission permission)
        {
            string result = "";
            result += "permission_type: ";
            result += permission.Type;
            result += "\n";
            result += "permission_id: ";
            result += permission.Id;
            result += "\n";
            result += "permission_name: ";
            result += permission.PermissionName;
            result += "\n";
            result += "threshold: ";
            result += permission.Threshold;
            result += "\n";
            result += "parent_id: ";
            result += permission.ParentId;
            result += "\n";
            result += "operations: ";
            result += permission.Operations.ToByteArray();
            result += "\n";
            if (permission.Keys.Count > 0)
            {
                result += "keys:";
                result += "\n";
                result += "[";
                result += "\n";
                foreach (Key key in permission.Keys)
                {
                    result += PrintKey(key);
                }
                result += "]";
                result += "\n";
            }
            return result;
        }

        public static string PrintKey(Key key)
        {
            string result = "";
            result += "address: ";
            result += Wallet.Encode58Check(key.Address.ToByteArray());
            result += "\n";
            result += "weight: ";
            result += key.Weight;
            result += "\n";

            return result;
        }

        public static string PrintPermissionList(List<Permission> permissions)
        {
            string result = "\n";
            result += "[";
            result += "\n";
            int i = 0;
            foreach (Permission permission in permissions)
            {
                result += "permission " + i + " :::";
                result += "\n";
                result += "{";
                result += "\n";
                result += PrintPermission(permission);
                result += "\n";
                result += "}";
                result += "\n";
                i++;
            }
            result += "]";

            return result;
        }

        public static string PrintSignature(List<ByteString> signatures)
        {
            string result = "";
            int i = 0;
            foreach (ByteString signature in signatures)
            {
                result += "signature " + i + " :";
                result += signature.ToByteArray().ToHexString();
                result += "\n";
                i++;
            }

            return result;
        }

        public static string PrintRet(List<Result> results)
        {
            string result = "";
            int i = 0;
            foreach (Result ret in results)
            {
                result += "result: ";
                result += i;
                result += " ::: ";
                result += "\n";
                result += "[";
                result += "\n";
                result += "code ::: ";
                result += ret.Ret;
                result += "\n";
                result += "fee ::: ";
                result += ret.Fee;
                result += "\n";
                result += "ContractRet ::: ";
                result += ret.ContractRet;
                result += "\n";
                result += "]";
                result += "\n";
                i++;
            }
            return result;
        }

        public static string PrintProposal(Proposal proposal)
        {
            string result = "";
            result += "id: ";
            result += proposal.ProposalId;
            result += "\n";
            result += "state: ";
            result += proposal.State;
            result += "\n";
            result += "createTime: ";
            result += proposal.CreateTime.ToDateTime().ToLocalTime();
            result += "\n";
            result += "expirationTime: ";
            result += proposal.ExpirationTime.ToDateTime().ToLocalTime();
            result += "\n";
            result += "parametersMap: ";
            foreach (var parameter in proposal.Parameters)
            {
                result += parameter.Key;
                result += "  ";
                result += parameter.Value;
                result += "\n";
            }
            result += "\n";
            result += "approvalsList: [ \n";
            foreach (ByteString address in proposal.Approvals)
            {
                result += Wallet.AddressToBase58(address.ToByteArray());
                result += "\n";
            }
            result += "]";
            return result;
        }

        public static string PrintProposalsList(ProposalList proposals)
        {
            string result = "\n";
            int i = 0;
            foreach (Proposal proposal in proposals.Proposals)
            {
                result += "proposal " + i + " :::";
                result += "\n";
                result += "[";
                result += "\n";
                result += PrintProposal(proposal);
                result += "]";
                result += "\n";
                result += "\n";
                i++;
            }
            return result;
        }

        #endregion

        #region AssetIssue
        public static string PrintAssetIssueList(AssetIssueList asset_issue_list)
        {
            string result = "";
            int i = 0;
            foreach (AssetIssueContract asset_issue in asset_issue_list.AssetIssue)
            {
                result += "assetIssue " + i + " :::";
                result += "\n";
                result += "[";
                result += "\n";
                result += PrintAssetIssue(asset_issue);
                result += "]";
                result += "\n";
                result += "\n";
                i++;
            }

            return result;
        }

        public static string PrintAssetIssue(AssetIssueContract asset_issue)
        {
            string result = "";
            result += "id: ";
            result += asset_issue.Id;
            result += "\n";
            result += "owner_address: ";
            result += Wallet.Encode58Check(asset_issue.OwnerAddress.ToByteArray());
            result += "\n";
            result += "name: ";
            result += Encoding.UTF8.GetString(asset_issue.Name.ToByteArray());
            result += "\n";
            result += "order: ";
            result += asset_issue.Order;
            result += "\n";
            result += "total_supply: ";
            result += asset_issue.TotalSupply;
            result += "\n";
            result += "trx_num: ";
            result += asset_issue.TrxNum;
            result += "\n";
            result += "num: ";
            result += asset_issue.Num;
            result += "\n";
            result += "precision ";
            result += asset_issue.Precision;
            result += "\n";
            result += "start_time: ";
            result += asset_issue.StartTime.ToDateTime().ToLocalTime();
            result += "\n";
            result += "end_time: ";
            result += asset_issue.EndTime.ToDateTime().ToLocalTime();
            result += "\n";
            result += "vote_score: ";
            result += asset_issue.VoteScore;
            result += "\n";
            result += "description: ";
            result += Encoding.UTF8.GetString(asset_issue.Description.ToByteArray());
            result += "\n";
            result += "url: ";
            result += Encoding.UTF8.GetString(asset_issue.Url.ToByteArray());
            result += "\n";
            result += "free asset net limit: ";
            result += asset_issue.FreeAssetNetLimit;
            result += "\n";
            result += "public free asset net limit: ";
            result += asset_issue.PublicFreeAssetNetLimit;
            result += "\n";
            result += "public free asset net usage: ";
            result += asset_issue.PublicFreeAssetNetUsage;
            result += "\n";
            result += "public latest free net time: ";
            result += asset_issue.PublicLatestFreeNetTime;
            result += "\n";

            if (asset_issue.FrozenSupply.Count > 0)
            {
                foreach (FrozenSupply frozenSupply in asset_issue.FrozenSupply)
                {
                    result += "frozen_supply";
                    result += "\n";
                    result += "{";
                    result += "\n";
                    result += "  amount: ";
                    result += frozenSupply.FrozenAmount;
                    result += "\n";
                    result += "  frozen_days: ";
                    result += frozenSupply.FrozenDays;
                    result += "\n";
                    result += "}";
                    result += "\n";
                }
            }

            if (asset_issue.Id.Equals(""))
            {
                result += "\n";
                result += "Note: In 3.2, you can use AssetIssueById or AssetIssueListByName, because 3.2 allows same token name.";
                result += "\n";
            }
            return result;
        }

        public static string PrintExchange(Exchange exchange)
        {
            string result = "";
            result += "id: ";
            result += exchange.ExchangeId;
            result += "\n";
            result += "creator: ";
            result += Wallet.AddressToBase58(exchange.CreatorAddress.ToByteArray());
            result += "\n";
            result += "createTime: ";
            result += exchange.CreateTime.ToDateTime().ToLocalTime();
            result += "\n";
            result += "firstTokenId: ";
            result += exchange.FirstTokenId.ToStringUtf8();
            result += "\n";
            result += "firstTokenBalance: ";
            result += exchange.FirstTokenBalance;
            result += "\n";
            result += "secondTokenId: ";
            result += exchange.SecondTokenId.ToStringUtf8();
            result += "\n";
            result += "secondTokenBalance: ";
            result += exchange.SecondTokenBalance;
            result += "\n";
            return result;
        }


        public static string PrintExchangeList(ExchangeList exchanges)
        {
            string result = "\n";
            int i = 0;
            foreach (Exchange exchange in exchanges.Exchanges)
            {
                result += "exchange " + i + " :::";
                result += "\n";
                result += "[";
                result += "\n";
                result += PrintExchange(exchange);
                result += "]";
                result += "\n";
                result += "\n";
                i++;
            }
            return result;
        }
        #endregion

        #region Node
        public static string PrintNodeList(NodeList nodes)
        {
            string result = "";
            foreach (var node in nodes.Nodes)
            {
                result += "IP::";
                result += node.Address.Host.ToStringUtf8();
                result += "\n";
                result += "Port::";
                result += node.Address.Port;
            }
            return result;
        }
        #endregion
    }
}
