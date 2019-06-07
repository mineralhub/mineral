using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Exception
{
    [Serializable]
    public class ContractValidateException : System.Exception
    {
        public ContractValidateException() { }
        public ContractValidateException(string message) : base(message) { }
        public ContractValidateException(string message, System.Exception inner) : base(message, inner) { }
        protected ContractValidateException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
