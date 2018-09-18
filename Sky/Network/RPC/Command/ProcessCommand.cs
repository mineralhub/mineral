using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using Sky.Core;
using Sky.Wallets;

namespace Sky.Network.RPC.Command
{
    public partial class ProcessCommand
    {
        public delegate JObject ProcessHandler(object obj, JArray parameters);

        public static JObject CreateErrorResponse(JToken id, int code, string message, string data = null)
        {
            JObject response = new JObject();
            response["jsonrpc"] = "2.0";
            response["id"] = id;
            response["error"] = new JObject();
            response["error"]["code"] = code;
            response["error"]["message"] = message;
            if (data != null)
                response["error"]["data"] = data;
            return response;
        }
    }
}
