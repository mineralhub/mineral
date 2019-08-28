using Mineral.Common.Application;
using Mineral.Common.Net.RPC;
using Mineral.Core.Config.Arguments;
using Mineral.Core.Net.RpcHandler;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Service
{
    public class RpcService : RpcServer, IService
    {
        #region Field
        private RpcMessageHandler handler = new RpcMessageHandler();
        #endregion


        #region Property
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        protected override JObject Process(JToken id, string method, JArray parameters)
        {
            return handler.Process(id, method, parameters);
        }
        #endregion


        #region External Method
        public void Init()
        {
        }

        public void Init(Args args)
        {
        }

        public void Start()
        {
            Start((int)Args.Instance.Node.RPC.Port);
        }

        public void Stop()
        {
            Dispose();
        }
        #endregion
    }
}
