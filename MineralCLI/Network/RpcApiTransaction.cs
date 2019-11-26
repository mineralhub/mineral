using Google.Protobuf;
using Mineral;
using Mineral.CommandLine;
using Mineral.Common.Net.RPC;
using Mineral.Common.Utils;
using Mineral.Core;
using Mineral.Core.Net.RpcHandler;
using Mineral.Cryptography;
using Mineral.Wallets.KeyStore;
using MineralCLI.Exception;
using MineralCLI.Util;
using Newtonsoft.Json.Linq;
using Protocol;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using static Protocol.Transaction.Types.Contract.Types;

namespace MineralCLI.Network
{
    using ResponseCode = TransactionSignWeight.Types.Result.Types.response_code;

    public partial class RpcApi
    {
        #region Field
        #endregion


        #region Property
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        public static Permission JsonToPermission(JObject json)
        {
            Permission permisstion = new Permission();
            if (json.ContainsKey("type"))
            {
                permisstion.Type = (Permission.Types.PermissionType)json["type"].ToObject<int>();
            }

            if (json.ContainsKey("permission_name"))
            {
                permisstion.PermissionName = json["permission_name"].ToString();
            }

            if (json.ContainsKey("threshold"))
            {
                permisstion.Threshold = json["threshold"].ToObject<long>();
            }

            if (json.ContainsKey("parent_id"))
            {
                permisstion.ParentId = json["parent_id"].ToObject<int>();
            }

            if (json.ContainsKey("operations"))
            {
                permisstion.Operations = ByteString.CopyFrom(json["operations"].ToString().HexToBytes());
            }

            if (json.ContainsKey("keys"))
            {
                List<Key> keys = new List<Key>();

                foreach (JToken token in json["keys"] as JArray)
                {
                    Key key = new Key();
                    key.Address = ByteString.CopyFrom(Wallet.Base58ToAddress(token["address"].ToString()));
                    key.Weight = token["weight"].ToObject<long>();
                    keys.Add(key);
                }

                permisstion.Keys.AddRange(keys);
            }

            return permisstion;
        }
        #endregion


        #region External Method
        public static RpcApiResult GetTotalTransaction(out NumberMessage message)
        {
            message = null;

            try
            {
                JObject receive = SendCommand(RpcCommand.Transaction.GetTotalTransaction, new JArray() { });
                if (receive.TryGetValue("error", out JToken value))
                {
                    return new RpcApiResult(false, value["code"].ToObject<int>(), value["message"].ToObject<string>());
                }

                message = NumberMessage.Parser.ParseFrom(receive["result"].ToObject<byte[]>());
            }
            catch (System.Exception e)
            {
                throw e;
            }

            return RpcApiResult.Success;
        }

        public static RpcApiResult GetTransactionApprovedList(Transaction transaction, out TransactionApprovedList transaction_list)
        {
            transaction_list = null;

            try
            {
                JObject receive = SendCommand(RpcCommand.Transaction.GetTransactionApprovedList, new JArray() { transaction.ToByteArray() });
                if (receive.TryGetValue("error", out JToken value))
                {
                    return new RpcApiResult(false, value["code"].ToObject<int>(), value["message"].ToObject<string>());
                }

                transaction_list = TransactionApprovedList.Parser.ParseFrom(receive["result"].ToObject<byte[]>());

            }
            catch (System.Exception e)
            {
                throw e;
            }

            return RpcApiResult.Success;
        }

        public static RpcApiResult GetTransactionById(string transaction_id, out TransactionExtention transaction)
        {
            transaction = null;

            try
            {
                JObject receive = SendCommand(RpcCommand.Transaction.GetTransactionById, new JArray() { transaction_id });
                if (receive.TryGetValue("error", out JToken value))
                {
                    return new RpcApiResult(false, value["code"].ToObject<int>(), value["message"].ToObject<string>());
                }

                transaction = TransactionExtention.Parser.ParseFrom(receive["result"].ToObject<byte[]>());
            }
            catch (System.Exception e)
            {
                throw e;
            }

            return RpcApiResult.Success;
        }

        public static RpcApiResult GetTransactionInfoById(string transaction_id, out TransactionInfo transaction)
        {
            transaction = null;

            try
            {
                JObject receive = SendCommand(RpcCommand.Transaction.GetTransactionInfoById, new JArray() { transaction_id });
                if (receive.TryGetValue("error", out JToken value))
                {
                    return new RpcApiResult(false, value["code"].ToObject<int>(), value["message"].ToObject<string>());
                }

                transaction = TransactionInfo.Parser.ParseFrom(receive["result"].ToObject<byte[]>());

            }
            catch (System.Exception e)
            {
                throw e;
            }

            return RpcApiResult.Success;
        }

        public static RpcApiResult GetTransactionCountByBlockNum(long block_num, out int count)
        {
            count = -1;

            try
            {
                JObject receive = SendCommand(RpcCommand.Transaction.GetTransactionCountByBlockNum, new JArray() { block_num });
                if (receive.TryGetValue("error", out JToken value))
                {
                    return new RpcApiResult(false, value["code"].ToObject<int>(), value["message"].ToObject<string>());
                }

                count = receive["result"].ToObject<int>();

            }
            catch (System.Exception e)
            {
                throw e;
            }

            return RpcApiResult.Success;
        }

        public static RpcApiResult GetTransactionsFromThis(byte[] address, int offset, int limit, out TransactionListExtention transactions)
        {
            transactions = null;

            try
            {
                JObject receive = SendCommand(RpcCommand.Transaction.GetTransactionsFromThis, new JArray() { address, offset, limit });
                if (receive.TryGetValue("error", out JToken value))
                {
                    return new RpcApiResult(false, value["code"].ToObject<int>(), value["message"].ToObject<string>());
                }

                transactions = TransactionListExtention.Parser.ParseFrom(receive["result"].ToObject<byte[]>());

            }
            catch (System.Exception e)
            {
                throw e;
            }

            return RpcApiResult.Success;
        }

        public static RpcApiResult GetTransactionsToThis(byte[] address, int offset, int limit, out TransactionListExtention transactions)
        {
            transactions = null;

            try
            {
                JObject receive = SendCommand(RpcCommand.Transaction.GetTransactionsFromThis, new JArray() { address, offset, limit });
                if (receive.TryGetValue("error", out JToken value))
                {
                    return new RpcApiResult(false, value["code"].ToObject<int>(), value["message"].ToObject<string>());
                }

                transactions = TransactionListExtention.Parser.ParseFrom(receive["result"].ToObject<byte[]>());

            }
            catch (System.Exception e)
            {
                throw e;
            }

            return RpcApiResult.Success;
        }

        public static RpcApiResult CreateAccountContract(byte[] owner_address,
                                                         byte[] create_address,
                                                         out AccountCreateContract contract)
        {
            contract = new AccountCreateContract();
            contract.AccountAddress = ByteString.CopyFrom(create_address);
            contract.OwnerAddress = ByteString.CopyFrom(owner_address);

            return RpcApiResult.Success;
        }

        public static RpcApiResult CreateProposalContract(byte[] owner_address,
                                                          Dictionary<long, long> parameters,
                                                          out ProposalCreateContract contract)
        {
            contract = new ProposalCreateContract();
            contract.OwnerAddress = ByteString.CopyFrom(owner_address);
            foreach (var parameter in parameters)
            {
                contract.Parameters.Add(parameter.Key, parameter.Value);
            }

            return RpcApiResult.Success;
        }

        public static RpcApiResult CreateWitnessContract(byte[] owner_address,
                                                         byte[] url,
                                                         out WitnessCreateContract contract)
        {
            contract = new WitnessCreateContract();
            contract.OwnerAddress = ByteString.CopyFrom(owner_address);
            contract.Url = ByteString.CopyFrom(url);

            return RpcApiResult.Success;
        }


        public static RpcApiResult CreateTransaferContract(byte[] owner_address,
                                                           byte[] to_address,
                                                           long amount, out TransferContract contract)
        {
            contract = new TransferContract();
            contract.ToAddress = ByteString.CopyFrom(to_address);
            contract.OwnerAddress = ByteString.CopyFrom(owner_address);
            contract.Amount = amount;

            return RpcApiResult.Success;
        }

        public static RpcApiResult CreateTransferAssetContract(byte[] to_address,
                                                               byte[] from_address,
                                                               byte[] asset_name,
                                                               long amount,
                                                               out TransferAssetContract contract)
        {
            contract = new TransferAssetContract();
            contract.ToAddress = ByteString.CopyFrom(to_address);
            contract.AssetName = ByteString.CopyFrom(asset_name);
            contract.OwnerAddress = ByteString.CopyFrom(from_address);
            contract.Amount = amount;

            return RpcApiResult.Success;
        }

        public static RpcApiResult CreateFreezeBalanceContract(byte[] owner_address,
                                                               byte[] address,
                                                               long amount,
                                                               long duration,
                                                               int resource_code,
                                                               out FreezeBalanceContract contract)
        {
            contract = new FreezeBalanceContract();
            contract.OwnerAddress = ByteString.CopyFrom(owner_address);
            contract.FrozenBalance = amount;
            contract.FrozenDuration = duration;
            contract.Resource = (ResourceCode)resource_code;

            if (address != null)
            {
                contract.ReceiverAddress = ByteString.CopyFrom(address);
            }

            return RpcApiResult.Success;
        }

        public static RpcApiResult CreateUnfreezeBalanceContract(byte[] owner_address,
                                                               byte[] address,
                                                               int resource_code,
                                                               out UnfreezeBalanceContract contract)
        {
            contract = new UnfreezeBalanceContract();
            contract.OwnerAddress = ByteString.CopyFrom(owner_address);
            contract.Resource = (ResourceCode)resource_code;

            if (address != null)
            {
                contract.ReceiverAddress = ByteString.CopyFrom(address);
            }

            return RpcApiResult.Success;
        }

        public static RpcApiResult CreateUnfreezeAssetContract(byte[] owner_address,
                                                               out UnfreezeAssetContract contract)
        {
            contract = new UnfreezeAssetContract();
            contract.OwnerAddress = ByteString.CopyFrom(owner_address);

            return RpcApiResult.Success;
        }

        public static RpcApiResult CreateVoteWitnessContract(byte[] owner_address,
                                                     Dictionary<byte[], long> votes,
                                                     out VoteWitnessContract contract)
        {
            contract = new VoteWitnessContract();
            contract.OwnerAddress = ByteString.CopyFrom(owner_address);

            foreach (var vote in votes)
            {
                if (vote.Key == null)
                {
                    continue;
                }

                VoteWitnessContract.Types.Vote entry = new VoteWitnessContract.Types.Vote();
                entry.VoteAddress = ByteString.CopyFrom(vote.Key);
                entry.VoteCount = vote.Value;
                contract.Votes.Add(entry);
            }

            return RpcApiResult.Success;
        }

        public static RpcApiResult CreateWithdrawBalanceContract(byte[] owner_address,
                                                                 out WithdrawBalanceContract contract)
        {
            contract = new WithdrawBalanceContract();
            contract.OwnerAddress = ByteString.CopyFrom(owner_address);

            return RpcApiResult.Success;
        }

        public static RpcApiResult CreateApproveProposalContract(byte[] owner_address,
                                                                 long id,
                                                                 bool value,
                                                                 out ProposalApproveContract contract)
        {
            contract = new ProposalApproveContract();
            contract.OwnerAddress = ByteString.CopyFrom(owner_address);
            contract.ProposalId = id;
            contract.IsAddApproval = value;

            return RpcApiResult.Success;
        }

        public static RpcApiResult CreateUpdateAcountContract(byte[] owner_address,
                                                              byte[] name,
                                                              out AccountUpdateContract contract)
        {
            contract = new AccountUpdateContract();
            contract.OwnerAddress = ByteString.CopyFrom(owner_address);
            contract.AccountName = ByteString.CopyFrom(name);

            return RpcApiResult.Success;
        }

        public static RpcApiResult CreateUpdateWitnessContract(byte[] owner_address,
                                                               byte[] url,
                                                               out WitnessUpdateContract contract)
        {
            contract = new WitnessUpdateContract();
            contract.OwnerAddress = ByteString.CopyFrom(owner_address);
            contract.UpdateUrl = ByteString.CopyFrom(url);

            return RpcApiResult.Success;
        }

        public static RpcApiResult CreateUpdateAssetContract(byte[] owner_address,
                                                             byte[] description,
                                                             byte[] url,
                                                             long limit,
                                                             long public_limit,
                                                             out UpdateAssetContract contract)
        {
            contract = new UpdateAssetContract();
            contract.OwnerAddress = ByteString.CopyFrom(owner_address);
            contract.Description = ByteString.CopyFrom(description);
            contract.Url = ByteString.CopyFrom(url);
            contract.NewLimit = limit;
            contract.NewPublicLimit = public_limit;

            return RpcApiResult.Success;
        }

        public static RpcApiResult CreateUpdateEnergyLimitContract(byte[] owner_address,
                                                                   byte[] contract_address,
                                                                   long energy_limit,
                                                                   out UpdateEnergyLimitContract contract)
        {
            contract = new UpdateEnergyLimitContract();
            contract.OwnerAddress = ByteString.CopyFrom(owner_address);
            contract.ContractAddress = ByteString.CopyFrom(contract_address);
            contract.OriginEnergyLimit = energy_limit;

            return RpcApiResult.Success;
        }

        public static RpcApiResult CreateAccountPermissionUpdateContract(byte[] owner_address,
                                                                         string permission_json,
                                                                         out AccountPermissionUpdateContract contract)
        {
            try
            {
                contract = new AccountPermissionUpdateContract();
                JObject json = JObject.Parse(permission_json);

                if (json.TryGetValue("owner_permission", out JToken output))
                {
                    contract.Owner = JsonToPermission(output as JObject);
                }

                if (json.TryGetValue("witness_permission", out output))
                {
                    contract.Witness = JsonToPermission(output as JObject);
                }

                if (json.TryGetValue("active_permission", out output))
                {
                    List<Permission> permissions = new List<Permission>();
                    foreach (JToken permission in output as JArray)
                    {
                        contract.Actives.Add(JsonToPermission(permission as JObject));
                    }
                }

                contract.OwnerAddress = ByteString.CopyFrom(owner_address);
            }
            catch (System.Exception e)
            {
                throw e;
            }

            return RpcApiResult.Success;
        }

        public static RpcApiResult CreateUpdateSettingContract(byte[] owner_address,
                                                               byte[] contract_address,
                                                               long resource_percent,
                                                               out UpdateSettingContract contract)
        {
            contract = new UpdateSettingContract();
            contract.OwnerAddress = ByteString.CopyFrom(owner_address);
            contract.ContractAddress = ByteString.CopyFrom(contract_address);
            contract.ConsumeUserResourcePercent = resource_percent;

            return RpcApiResult.Success;
        }

        public static RpcApiResult CreateProposalDeleteContract(byte[] owner_address,
                                                                long id,
                                                                out ProposalDeleteContract contract)
        {
            contract = new ProposalDeleteContract();
            contract.OwnerAddress = ByteString.CopyFrom(owner_address);
            contract.ProposalId = id;

            return RpcApiResult.Success;
        }

        public static RpcApiResult CreateTransaction(IMessage contract,
                                                     ContractType contract_type,
                                                     out TransactionExtention transaction_extention)
        {
            try
            {
                transaction_extention = null;

                JObject receive = SendCommand(RpcCommand.Transaction.CreateTransaction,
                                              new JArray()
                                              {
                                                  contract_type,
                                                  contract.ToByteArray()
                                              });

                if (receive.TryGetValue("error", out JToken value))
                {
                    return new RpcApiResult(false, value["code"].ToObject<int>(), value["message"].ToObject<string>());
                }

                transaction_extention = TransactionExtention.Parser.ParseFrom(receive["result"].ToObject<byte[]>());

                Return ret = transaction_extention.Result;
                if (!ret.Result)
                {
                    OutputTransactionErrorMessage((int)ret.Code, ret.Message.ToStringUtf8());
                    return new RpcApiResult(false, RpcMessage.TRANSACTION_ERROR, "");
                }
            }
            catch (System.Exception e)
            {
                throw e;
            }

            return RpcApiResult.Success;
        }

        public static RpcApiResult SignatureTransaction(ref Transaction transaction)
        {
            if (transaction.RawData.Timestamp == 0)
            {
                transaction.RawData.Timestamp = Helper.CurrentTimeMillis();
            }

            ProtocolUtil.SetExpirationTime(ref transaction);
            Console.WriteLine("Your transaction details are as follows, please confirm.");
            Console.WriteLine("transaction hex string is " + transaction.ToByteArray().ToHexString());
            Console.WriteLine(PrintUtil.PrintTransaction(transaction));
            Console.WriteLine(
                "Please confirm and input your permission id, if input y or Y means default 0, other non-numeric characters will cancell transaction.");
            ProtocolUtil.SetPermissionId(ref transaction);

            try
            {
                while (true)
                {
                    Console.WriteLine("Please choose keystore for signature.");
                    KeyStore key_store = SelectKeyStore();

                    string password = CommandLineUtil.ReadPasswordString("Please input password");
                    if (KeyStoreService.DecryptKeyStore(password, key_store, out byte[] privatekey))
                    {
                        ECKey key = ECKey.FromPrivateKey(privatekey);
                        ECDSASignature signature = key.Sign(SHA256Hash.ToHash(transaction.RawData.ToByteArray()));

                        transaction.Signature.Add(ByteString.CopyFrom(signature.ToByteArray()));
                        Console.WriteLine("current transaction hex string is " + transaction.ToByteArray().ToHexString());
                    }
                    else
                    {
                        return new RpcApiResult(false, RpcMessage.INVALID_PASSWORD, "Invalid keystore password");
                    }

                    JObject receive = SendCommand(RpcCommand.Transaction.GetTransactionSignWeight, new JArray() { transaction.ToByteArray() });

                    TransactionSignWeight weight = TransactionSignWeight.Parser.ParseFrom(receive["result"].ToObject<byte[]>());
                    if (weight.Result.Code == ResponseCode.EnoughPermission)
                    {
                        break;
                    }
                    else if (weight.Result.Code == ResponseCode.NotEnoughPermission)
                    {
                        Console.WriteLine("Current signWeight is:");
                        Console.WriteLine(PrintUtil.PrintTransactionSignWeight(weight));
                        Console.WriteLine("Please confirm if continue add signature enter y or Y, else any other");

                        if (!CommandLineUtil.Confirm())
                        {
                            throw new CancelException("User cancelled");
                        }
                        continue;
                    }

                    throw new CancelException(weight.Result.Message);
                }
            }
            catch (System.Exception e)
            {
                throw e;
            }

            return RpcApiResult.Success;
        }

        public static RpcApiResult ProcessTransactionExtention(TransactionExtention transaction_extention)
        {
            if (transaction_extention == null)
            {
                return new RpcApiResult(false, RpcMessage.TRANSACTION_ERROR, "Transaction is null");
            }

            Return ret = transaction_extention.Result;
            if (!ret.Result)
            {
                OutputTransactionErrorMessage((int)ret.Code, ret.Message.ToStringUtf8());
                return new RpcApiResult(false, RpcMessage.TRANSACTION_ERROR, "");
            }

            Transaction transaction = transaction_extention.Transaction;
            if (transaction == null || transaction.RawData.Contract.Count == 0)
            {
                return new RpcApiResult(false, RpcMessage.TRANSACTION_ERROR, "Transaction is null");
            }

            Console.WriteLine("Receive txid = " + transaction_extention.Txid.ToByteArray().ToHexString());
            Console.WriteLine("Transaction hex string is " + PrintUtil.PrintTransaction(transaction));
            Console.WriteLine(PrintUtil.PrintTransaction(transaction_extention));

            RpcApiResult result = SignatureTransaction(ref transaction);
            if (!result.Result)
            {
                return result;
            }

            return BroadcastTransaction(transaction);
        }

        public static RpcApiResult BroadcastTransaction(Transaction transaction)
        {
            try
            {
                JObject receive = SendCommand(RpcCommand.Transaction.BroadcastTransaction, new JArray() { transaction.ToByteArray() });
                Return ret = Return.Parser.ParseFrom(receive["result"].ToObject<byte[]>());

                int retry = 10;
                while (ret.Result == false && ret.Code == Return.Types.response_code.ServerBusy && retry > 0)
                {
                    retry--;
                    receive = SendCommand(RpcCommand.Transaction.BroadcastTransaction, new JArray() { transaction.ToByteArray() });
                    ret = Return.Parser.ParseFrom(receive["result"].ToObject<byte[]>());
                    Console.WriteLine("Retry broadcast : " + (11 - retry));

                    Thread.Sleep(1000);
                }

                if (!ret.Result)
                {
                    Console.WriteLine("Code : " + ret.Code);
                    Console.WriteLine("Message : " + ret.Message.ToStringUtf8());
                    return new RpcApiResult(false, RpcMessage.INVALID_REQUEST, ret.Message.ToStringUtf8());
                }
            }
            catch (System.Exception e)
            {
                throw e;
            }

            return RpcApiResult.Success;
        }

        public static RpcApiResult ListProposal(out ProposalList proposals)
        {
            proposals = null;

            JObject receive = SendCommand(RpcCommand.Transaction.ListProposal, new JArray() { });
            if (receive.TryGetValue("error", out JToken value))
            {
                return new RpcApiResult(false, value["code"].ToObject<int>(), value["message"].ToObject<string>());
            }

            proposals = ProposalList.Parser.ParseFrom(receive["result"].ToObject<byte[]>());

            return RpcApiResult.Success;
        }

        public static RpcApiResult ListProposalPaginated(int offset,
                                                         int limit,
                                                         out ProposalList proposals)
        {
            proposals = null;

            PaginatedMessage message = new PaginatedMessage();
            message.Offset = offset;
            message.Limit = limit;

            JObject receive = SendCommand(RpcCommand.Transaction.ListProposalPaginated, new JArray() { message.ToByteArray() });
            if (receive.TryGetValue("error", out JToken value))
            {
                return new RpcApiResult(false, value["code"].ToObject<int>(), value["message"].ToObject<string>());
            }

            proposals = ProposalList.Parser.ParseFrom(receive["result"].ToObject<byte[]>());

            return RpcApiResult.Success;
        }

        public static RpcApiResult GetParameters(out ChainParameters parameters)
        {
            parameters = null;

            JObject receive = SendCommand(RpcCommand.Transaction.GetParameters, new JArray() { });
            if (receive.TryGetValue("error", out JToken value))
            {
                return new RpcApiResult(false, value["code"].ToObject<int>(), value["message"].ToObject<string>());
            }

            parameters = ChainParameters.Parser.ParseFrom(receive["result"].ToObject<byte[]>());

            return RpcApiResult.Success;
        }
        #endregion
    }
}
