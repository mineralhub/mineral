using System;
using System.Collections.Generic;
using System.Text;

namespace MineralCLI.Network
{
    public class RpcApiResult
    {
        #region Field
        public static readonly RpcApiResult Success = new RpcApiResult(true, 0, "");

        private bool result = false;
        private int code = 0;
        private string message = "";
        #endregion


        #region Property
        public bool Result
        {
            get { return this.result; }
        }

        public int Code
        {
            get { return this.code; }
        }

        public string Message
        {
            get { return this.message; }
        }
        #endregion

        
        #region Contructor
        public RpcApiResult()
        {
            this.result = true;
            this.code = 0;
            this.message = "";
        }

        public RpcApiResult(bool result, int code, string message)
        {
            this.result = result;
            this.code = code;
            this.message = message;
        }
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
                return true;

            if (obj == null || this.GetType() != obj.GetType())
                return false;

            RpcApiResult ret = obj as RpcApiResult;

            return this.result == ret.result
                && this.code == ret.code;
        }
        #endregion
    }
}
