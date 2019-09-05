using Google.Protobuf;
using Mineral;
using Mineral.Common.Utils;
using Mineral.Core;
using Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using static Protocol.AssetIssueContract.Types;
using static Protocol.Transaction.Types;
using static Protocol.Transaction.Types.Contract.Types;

namespace MineralCLI.Util
{
    public static class PrintUtil
    {
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
            result += new DateTime(raw.Timestamp);
            result += "\n";

            result += "fee_limit: ";
            result += raw.FeeLimit;
            result += "\n";

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
            result += new DateTime(asset_issue.StartTime);
            result += "\n";
            result += "end_time: ";
            result += new DateTime(asset_issue.EndTime);
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
    }
}
