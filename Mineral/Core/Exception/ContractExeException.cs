using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Exception
{
    [Serializable]
    public class ContractExeException : System.Exception
    {
        public ContractExeException() { }
        public ContractExeException(string message) : base(message) { }
        public ContractExeException(string message, System.Exception inner) : base(message, inner) { }
        protected ContractExeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
