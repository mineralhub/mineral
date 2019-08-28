using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Mineral.Common.Net.RPC
{
    public static class RpcMessage
    {
        #region error code
        public static readonly int PARSE_ERROR = -32700;
        public static readonly int INVALID_REQUEST = -32600;
        public static readonly int METHOD_NOT_FOUND = -32601;
        public static readonly int INVALID_PARAMS = -32602;
        public static readonly int INTERNAL_ERROR = -32603;
        public static readonly int UNKNOWN_ERROR = -32000;
        #endregion

        public static JObject CreateErrorResult(JToken id, int code, string message)
        {
            JObject response = new JObject();
            response["error"] = new JObject();
            response["error"]["code"] = code;
            response["error"]["message"] = message;

            return response;
        }

        public static JObject CreateResponse(JToken id)
        {
            JObject response = new JObject();
            response["jsonrpc"] = "2.0";
            response["id"] = id;

            return response;
        }
    }
}
