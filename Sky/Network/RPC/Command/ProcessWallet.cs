using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Sky.Network.RPC.Command
{
    public partial class ProcessCommand
    {
        public static JObject OnCreateAccount(JArray parameters)
        {
            JObject json = new JObject();
            return json;
        }

        public static JObject OnOpenAccount(JArray parameters)
        {
            JObject json = new JObject();
            return json;
        }

        public static JObject OnCloseAccount(JArray parameters)
        {
            JObject json = new JObject();
            return json;
        }

        public static JObject OnGetAccount(JArray parameters)
        {
            JObject json = new JObject();
            return json;
        }

        public static JObject OnGetAddress(JArray parameters)
        {
            JObject json = new JObject();
            return json;
        }

        public static JObject OnGetBalance(JArray parameters)
        {
            JObject json = new JObject();
            return json;
        }

        public static JObject OnSendTo(JArray parameters)
        {
            JObject json = new JObject();
            return json;
        }

        public static JObject OnFreezeBalance(JArray parameters)
        {
            JObject json = new JObject();
            return json;
        }

        public static JObject OnUnfreezeBalance(JArray parameters)
        {
            JObject json = new JObject();
            return json;
        }

        public static JObject OnVoteWitness(JArray parameters)
        {
            JObject json = new JObject();
            return json;
        }
    }
}
