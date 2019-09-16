using Google.Protobuf;
using Mineral.Common.Net.RPC;
using Mineral.Core.Capsule;
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
            { RpcCommandType.CreateTransaction, new RpcHandler(OnCreateTransaction) },
            { RpcCommandType.GetTransactionSignWeight, new RpcHandler(OnGetTransactionSignWeight) },
            { RpcCommandType.BroadcastTransaction, new RpcHandler(OnBroadcastTransaction) },

            { RpcCommandType.GetAccount, new RpcHandler(OnGetAccount) },

            { RpcCommandType.GetBlock, new RpcHandler(OnGetBlock) }
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
                TransactionCapsule transaction = 
                    RpcWalletApi.CreateTransactionCapsule(contract, Transaction.Types.Contract.Types.ContractType.TransferContract);

                TransactionExtention transaction_extention =
                    RpcWalletApi.CreateTransactionExtention(transaction);

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
            TransactionSignWeight weight = RpcWalletApi.GetTransactionSignWeight(transaction);

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
            Return ret = RpcWalletApi.BroadcastTransaction(transaction);

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
                AccountCapsule account = Wallet.GetAccount(address);

                result = (account != null) ? JToken.FromObject(account.Data) : new JObject();
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
                result = JToken.FromObject(block.Data);
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
        #endregion
    }
}
