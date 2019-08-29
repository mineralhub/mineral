using Mineral.Common.Net.RPC;
using Mineral.Core.Capsule;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Net.RpcHandler
{
    public class RpcMessageHandler
    {
        #region Field
        public delegate bool RpcHandler(JToken id, string method, JArray parameters, out JObject result);

        private Dictionary<string, RpcHandler> handlers = new Dictionary<string, RpcHandler>()
        {
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
        public JObject Process(JToken id, string method, JArray parameters)
        {
            JObject result = new JObject();
            if (this.handlers.ContainsKey(method))
            {
                handlers[method](id, method, parameters, out result);
            }
            else
            {
                result = RpcMessage.CreateErrorResult(id, RpcMessage.METHOD_NOT_FOUND, "Method not found");
            }

            return result;
        }

        public static bool OnGetAccount(JToken id, string method, JArray parameters, out JObject result)
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
                result = JObject.Parse(JsonConvert.SerializeObject(account.Instance, Formatting.Indented));
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

        public static bool OnGetBlock(JToken id, string method, JArray parameters, out JObject result)
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
                result = JObject.Parse(JsonConvert.SerializeObject(block.Instance, Formatting.Indented));
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
