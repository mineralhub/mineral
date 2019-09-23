using Google.Protobuf;
using Mineral.Common.Net.RPC;
using Mineral.Common.Utils;
using Mineral.Core.Capsule;
using Mineral.Core.Exception;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Net.RpcHandler
{
    public class RpcMessageHandler
    {
        #region Field
        public delegate bool RpcHandler(JToken id, string method, JArray parameters, out JToken result);

        private Dictionary<string, RpcHandler> handlers = new Dictionary<string, RpcHandler>()
        {
            { RpcCommandType.CreateAccount, new RpcHandler(OnCreateAccount) },
            { RpcCommandType.CreateProposal, new RpcHandler(OnCreateProposal) },
            { RpcCommandType.CreateWitness, new RpcHandler(OnCreateWitness) },
            { RpcCommandType.CreateTransaction, new RpcHandler(OnCreateTransaction) },
            { RpcCommandType.UpdateAccount, new RpcHandler(OnUpdateAccount) },
            { RpcCommandType.UpdateWitness, new RpcHandler(OnUpdateWitness) },
            { RpcCommandType.GetTransactionSignWeight, new RpcHandler(OnGetTransactionSignWeight) },
            { RpcCommandType.BroadcastTransaction, new RpcHandler(OnBroadcastTransaction) },

            { RpcCommandType.GetAccount, new RpcHandler(OnGetAccount) },
            { RpcCommandType.AssetIssueByAccount, new RpcHandler(OnAssetIssueByAccount) },
            { RpcCommandType.AssetIssueById, new RpcHandler(OnAssetIssueById) },
            { RpcCommandType.AssetIssueByName, new RpcHandler(OnAssetIssueByName) },
            { RpcCommandType.AssetIssueListByName, new RpcHandler(OnAssetIssueListByName) },
            { RpcCommandType.TransferAsset, new RpcHandler(OnTransferAsset) },

            { RpcCommandType.GetBlock, new RpcHandler(OnGetBlock) },
            { RpcCommandType.GetBlockByLatestNum, new RpcHandler(OnGetBlockByLatestNum) },
            { RpcCommandType.GetBlockById, new RpcHandler(OnGetBlockById) },
            { RpcCommandType.GetBlockByLimitNext, new RpcHandler(OnGetBlockByLimitNext) }
        };
        #endregion


        #region Property
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public bool Process(JToken id, string method, JArray parameters, out JToken result)
        {
            bool ret = false;
            if (this.handlers.ContainsKey(method))
            {
                ret = handlers[method](id, method, parameters, out result);
            }
            else
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.METHOD_NOT_FOUND, "Method not found");
                ret = false;
            }

            return ret;
        }

        public static bool OnCreateAccount(JToken id, string method, JArray parameters, out JToken result)
        {
            result = new JObject();

            if (parameters == null || parameters.Count != 1)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.INVALID_PARAMS, "Invalid parameters");
                return false;
            }

            try
            {
                AccountCreateContract contract = AccountCreateContract.Parser.ParseFrom(parameters[0].ToObject<byte[]>());
                TransactionExtention transaction_extention =
                    RpcApiService.CreateTransactionExtention(contract, Transaction.Types.Contract.Types.ContractType.AccountCreateContract);

                result = JToken.FromObject(transaction_extention.ToByteArray());
            }
            catch (System.Exception e)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.INVALID_PARAMS, e.Message);
                return false;
            }

            return true;
        }

        public static bool OnCreateProposal(JToken id, string method, JArray parameters, out JToken result)
        {
            result = new JObject();

            if (parameters == null || parameters.Count != 1)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.INVALID_PARAMS, "Invalid parameters");
                return false;
            }

            try
            {
                ProposalCreateContract contract = ProposalCreateContract.Parser.ParseFrom(parameters[0].ToObject<byte[]>());
                TransactionExtention transaction_extention =
                    RpcApiService.CreateTransactionExtention(contract, Transaction.Types.Contract.Types.ContractType.ProposalCreateContract);

                result = JToken.FromObject(transaction_extention.ToByteArray());
            }
            catch (System.Exception e)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.INVALID_PARAMS, e.Message);
                return false;
            }

            return true;
        }

        public static bool OnCreateWitness(JToken id, string method, JArray parameters, out JToken result)
        {
            result = new JObject();

            if (parameters == null || parameters.Count != 1)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.INVALID_PARAMS, "Invalid parameters");
                return false;
            }

            try
            {
                WitnessCreateContract contract = WitnessCreateContract.Parser.ParseFrom(parameters[0].ToObject<byte[]>());
                TransactionExtention transaction_extention =
                    RpcApiService.CreateTransactionExtention(contract, Transaction.Types.Contract.Types.ContractType.WitnessCreateContract);

                result = JToken.FromObject(transaction_extention.ToByteArray());
            }
            catch (System.Exception e)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.INVALID_PARAMS, e.Message);
                return false;
            }

            return true;
        }

        public static bool OnCreateTransaction(JToken id, string method, JArray parameters, out JToken result)
        {
            result = new JObject();

            if (parameters == null || parameters.Count != 1)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.INVALID_PARAMS, "Invalid parameters");
                return false;
            }

            try
            {
                TransferContract contract = TransferContract.Parser.ParseFrom(parameters[0].ToObject<byte[]>());
                TransactionExtention transaction_extention =
                    RpcApiService.CreateTransactionExtention(contract, Transaction.Types.Contract.Types.ContractType.TransferContract);

                result = JToken.FromObject(transaction_extention.ToByteArray());
            }
            catch (System.Exception e)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.INVALID_PARAMS, e.Message);
                return false;
            }

            return true;
        }

        public static bool OnUpdateAccount(JToken id, string method, JArray parameters, out JToken result)
        {
            result = new JObject();

            if (parameters == null || parameters.Count != 1)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.INVALID_PARAMS, "Invalid parameters");
                return false;
            }

            try
            {
                AccountUpdateContract contract = AccountUpdateContract.Parser.ParseFrom(parameters[0].ToObject<byte[]>());
                TransactionExtention transaction_extention =
                    RpcApiService.CreateTransactionExtention(contract, Transaction.Types.Contract.Types.ContractType.AccountUpdateContract);

                result = JToken.FromObject(transaction_extention.ToByteArray());
            }
            catch (System.Exception e)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.INVALID_PARAMS, e.Message);
                return false;
            }

            return true;
        }

        public static bool OnUpdateWitness(JToken id, string method, JArray parameters, out JToken result)
        {
            result = new JObject();

            if (parameters == null || parameters.Count != 1)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.INVALID_PARAMS, "Invalid parameters");
                return false;
            }

            try
            {
                WitnessUpdateContract contract = WitnessUpdateContract.Parser.ParseFrom(parameters[0].ToObject<byte[]>());
                TransactionExtention transaction_extention =
                    RpcApiService.CreateTransactionExtention(contract, Transaction.Types.Contract.Types.ContractType.WitnessUpdateContract);

                result = JToken.FromObject(transaction_extention.ToByteArray());
            }
            catch (System.Exception e)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.INVALID_PARAMS, e.Message);
                return false;
            }

            return true;
        }

        public static bool OnGetTransactionSignWeight(JToken id, string method, JArray parameters, out JToken result)
        {
            result = new JObject();

            if (parameters == null || parameters.Count != 1)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.INVALID_PARAMS, "Invalid parameters");
                return false;
            }

            Transaction transaction = Transaction.Parser.ParseFrom(parameters[0].ToObject<byte[]>());
            TransactionSignWeight weight = RpcApiService.GetTransactionSignWeight(transaction);

            result = JToken.FromObject(weight.ToByteArray());

            return true;
        }

        public static bool OnBroadcastTransaction(JToken id, string method, JArray parameters, out JToken result)
        {
            result = new JObject();

            if (parameters == null || parameters.Count != 1)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.INVALID_PARAMS, "Invalid parameters");
                return false;
            }

            Transaction transaction = Transaction.Parser.ParseFrom(parameters[0].ToObject<byte[]>());
            Return ret = RpcApiService.BroadcastTransaction(transaction);

            result = JToken.FromObject(ret.ToByteArray());

            return true;
        }

        public static bool OnGetAccount(JToken id, string method, JArray parameters, out JToken result)
        {
            result = new JObject();

            if (parameters == null || parameters.Count != 1)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.INVALID_PARAMS, "Invalid parameters");
                return false;
            }

            try
            {
                byte[] address = Wallet.Base58ToAddress(parameters[0].Value<string>());
                Account account = RpcApiService.GetAccount(address);

                result = (account != null) ? JToken.FromObject(account.ToByteArray()) : new JObject();
            }
            catch (InvalidCastException e)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.INVALID_PARAMS, e.Message);
                return false;
            }
            catch (FormatException e)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.INVALID_PARAMS, e.Message);
                return false;
            }
            catch (System.Exception e)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.UNKNOWN_ERROR, e.Message);
                return false;
            }

            return true;
        }

        public static bool OnAssetIssueByAccount(JToken id, string method, JArray parameters, out JToken result)
        {
            result = new JObject();

            if (parameters == null || parameters.Count != 1)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.INVALID_PARAMS, "Invalid parameters");
                return false;
            }

            try
            {
                byte[] address = Wallet.Base58ToAddress(parameters[0].ToString());
                AssetIssueList asset_issue_list = RpcApiService.GetAssetIssueList(address);

                result = JToken.FromObject(asset_issue_list);
            }
            catch (ArgumentException e)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.INVALID_PARAMS, e.Message);
                return false;
            }
            catch (System.Exception e)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.UNKNOWN_ERROR, e.Message);
                return false;
            }

            return true;
        }

        public static bool OnAssetIssueById(JToken id, string method, JArray parameters, out JToken result)
        {
            result = new JObject();

            if (parameters == null || parameters.Count != 1)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.INVALID_PARAMS, "Invalid parameters");
                return false;
            }

            try
            {
                byte[] asset_id = Encoding.UTF8.GetBytes(parameters[0].ToString());
                AssetIssueContract asset_issue_contract = RpcApiService.GetAssetIssueById(asset_id);

                result = JToken.FromObject(asset_issue_contract.ToByteArray());
            }
            catch (ArgumentException e)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.INVALID_PARAMS, e.Message);
                return false;
            }
            catch (System.Exception e)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.UNKNOWN_ERROR, e.Message);
                return false;
            }

            return true;
        }

        public static bool OnAssetIssueByName(JToken id, string method, JArray parameters, out JToken result)
        {
            result = new JObject();

            if (parameters == null || parameters.Count != 1)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.INVALID_PARAMS, "Invalid parameters");
                return false;
            }

            try
            {
                byte[] name = Encoding.UTF8.GetBytes(parameters[0].ToString());
                AssetIssueContract asset_issue_contract = RpcApiService.GetAssetIssueByName(name);

                result = JToken.FromObject(asset_issue_contract.ToByteArray());
            }
            catch (ArgumentException e)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.INVALID_PARAMS, e.Message);
                return false;
            }
            catch (System.Exception e)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.UNKNOWN_ERROR, e.Message);
                return false;
            }

            return true;
        }

        public static bool OnAssetIssueListByName(JToken id, string method, JArray parameters, out JToken result)
        {
            result = new JObject();

            if (parameters == null || parameters.Count != 1)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.INVALID_PARAMS, "Invalid parameters");
                return false;
            }

            try
            {
                byte[] name = Encoding.UTF8.GetBytes(parameters[0].ToString());
                AssetIssueList asset_issue_list = RpcApiService.GetAssetIssueListByName(name);

                result = JToken.FromObject(asset_issue_list.ToByteArray());
            }
            catch (NonUniqueObjectException e)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.INTERNAL_ERROR, e.Message);
                return false;
            }
            catch (ArgumentException e)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.INVALID_PARAMS, e.Message);
                return false;
            }
            catch (System.Exception e)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.UNKNOWN_ERROR, e.Message);
                return false;
            }

            return true;
        }

        public static bool OnTransferAsset(JToken id, string method, JArray parameters, out JToken result)
        {
            result = new JObject();

            if (parameters == null || parameters.Count != 1)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.INVALID_PARAMS, "Invalid parameters");
                return false;
            }

            try
            {
                TransferAssetContract contract = TransferAssetContract.Parser.ParseFrom(parameters.ToObject<byte[]>());
                TransactionExtention transaction =
                    RpcApiService.CreateTransactionExtention(contract,Transaction.Types.Contract.Types.ContractType.TransferAssetContract);

                result = JToken.FromObject(transaction.ToByteArray());
            }
            catch (System.Exception e)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.UNKNOWN_ERROR, e.Message);
                return false;
            }

            return true;
        }

        public static bool OnGetBlock(JToken id, string method, JArray parameters, out JToken result)
        {
            result = new JObject();

            if (parameters == null || parameters.Count != 1)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.INVALID_PARAMS, "Invalid parameters");
                return false;
            }

            try
            {
                BlockCapsule block = Manager.Instance.DBManager.GetBlockByNum(parameters[0].Value<long>());
                result = JToken.FromObject(RpcApiService.CreateBlockExtention(block).ToByteArray());
            }
            catch (InvalidCastException e)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.INVALID_PARAMS, e.Message);
                return false;
            }
            catch (System.Exception e)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.UNKNOWN_ERROR, e.Message);
                return false;
            }

            return true;
        }

        public static bool OnGetBlockByLatestNum(JToken id, string method, JArray parameters, out JToken result)
        {
            result = new JObject();

            if (parameters == null || parameters.Count != 0)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.INVALID_PARAMS, "Invalid parameters");
                return false;
            }

            try
            {
                List<BlockCapsule> blocks = Manager.Instance.DBManager.Block.GetBlockByLatestNum(1);
                if (blocks == null || blocks.Count == 0)
                {
                    result = RpcMessage.CreateErrorResult(id, RpcMessage.INTERNAL_ERROR, "Can not load latest block.");
                }

                BlockExtention block_extention = RpcApiService.CreateBlockExtention(blocks[0]);
                result = JToken.FromObject(block_extention.ToByteArray());
            }
            catch (InvalidCastException e)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.INVALID_PARAMS, e.Message);
                return false;
            }
            catch (System.Exception e)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.UNKNOWN_ERROR, e.Message);
                return false;
            }

            return true;
        }

        public static bool OnGetBlockById(JToken id, string method, JArray parameters, out JToken result)
        {
            result = new JObject();

            if (parameters == null || parameters.Count != 1)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.INVALID_PARAMS, "Invalid parameters");
                return false;
            }

            try
            {
                SHA256Hash hash = SHA256Hash.Wrap(parameters[0].ToString().HexToBytes());
                BlockCapsule block = Manager.Instance.DBManager.GetBlockById(hash);
                BlockExtention block_extention = RpcApiService.CreateBlockExtention(block);

                result = JToken.FromObject(block_extention.ToByteArray());
            }
            catch (System.Exception e)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.UNKNOWN_ERROR, e.Message);
                return false;
            }

            return true;
        }


        public static bool OnGetBlockByLimitNext(JToken id, string method, JArray parameters, out JToken result)
        {
            result = new JObject();

            if (parameters == null || parameters.Count != 1)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.INVALID_PARAMS, "Invalid parameters");
                return false;
            }

            try
            {
                BlockLimit limit = BlockLimit.Parser.ParseFrom(parameters[0].ToObject<byte[]>());
                BlockListExtention blocks = new BlockListExtention();
                Manager.Instance.DBManager.Block.GetLimitNumber(limit.StartNum, limit.EndNum - limit.StartNum).ForEach(block =>
                {
                    blocks.Block.Add(RpcApiService.CreateBlockExtention(block));
                });

                result = JToken.FromObject(blocks.ToByteArray());
            }
            catch (System.Exception e)
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.UNKNOWN_ERROR, e.Message);
                return false;
            }

            return true;
        }
        #endregion
    }
}
