using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Mineral.Network.RPC.Command
{
    public partial class RpcCommand
    {
        public delegate JObject ProcessHandler(object obj, JArray parameters);

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

        public static JObject CreateErrorResult(JToken id, int code, string message, string data = null)
        {
            JObject response = new JObject();
            response["error"] = new JObject();
            if (id != null)
                response["error"]["id"] = id;
            response["error"]["code"] = code;
            response["error"]["message"] = message;
            if (data != null)
                response["error"]["data"] = data;
            return response;
        }
    }
}
