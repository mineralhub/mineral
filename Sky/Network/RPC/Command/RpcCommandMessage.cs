using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Sky.Network.RPC.Command
{
    public partial class RpcCommand
    {
        public delegate JObject ProcessHandler(object obj, RpcCommand.ParamType type, JArray parameters);

        //public delegate string InvalidParameterMessage();

        //protected static Dictionary<object, InvalidParameterMessage> InvalidParameterHandlers = new Dictionary<object, InvalidParameterMessage>()
        //{
        //    { RpcCommand.Block.GetBlock, InvalidParameter_GetBlock },
        //};

        //public static string InvalidParameter_GetBlock()
        //{
        //    return "";
        //}

        //public static string GetInvalidParameterMessage(object obj)
        //{
        //    return InvalidParameterHandlers.ContainsKey(obj) ?
        //        InvalidParameterHandlers[obj]() : "";
        //}

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
