using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Sky.Network.RPC.Command
{
    public partial class ProcessCommand
    {
        public static JObject OnCreateAccount(object obj, JArray parameters)
        {
            JObject json = new JObject();
            return json;
        }

        public static JObject OnOpenAccount(object obj, JArray parameters)
        {
            JObject json = new JObject();
            return json;
        }

        public static JObject OnCloseAccount(object obj, JArray parameters)
        {
            JObject json = new JObject();
            return json;
        }

        public static JObject OnGetAccount(object obj, JArray parameters)
        {
            JObject json = new JObject();
            return json;
        }

        public static JObject OnGetAddress(object obj, JArray parameters)
        {
            JObject json = new JObject();
            return json;
        }

        public static JObject OnGetBalance(object obj, JArray parameters)
        {
            JObject json = new JObject();
            return json;
        }

        public static JObject OnSendTo(object obj, JArray parameters)
        {
            JObject json = new JObject();
            return json;
        }

        public static JObject OnFreezeBalance(object obj, JArray parameters)
        {
            JObject json = new JObject();
            return json;
        }

        public static JObject OnUnfreezeBalance(object obj, JArray parameters)
        {
            JObject json = new JObject();
            return json;
        }

        public static JObject OnVoteWitness(object obj, JArray parameters)
        {
            JObject json = new JObject();
            return json;
        }
    }
}
