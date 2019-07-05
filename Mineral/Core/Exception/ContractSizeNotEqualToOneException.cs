using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Exception
{
    [Serializable]
    public class ContractSizeNotEqualToOneException : System.Exception
    {
        public ContractSizeNotEqualToOneException() { }
        public ContractSizeNotEqualToOneException(string message) : base(message) { }
        public ContractSizeNotEqualToOneException(string message, System.Exception inner) : base(message, inner) { }
        protected ContractSizeNotEqualToOneException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
