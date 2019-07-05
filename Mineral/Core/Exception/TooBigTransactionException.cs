using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Exception
{
    [Serializable]
    public class TooBigTransactionException : System.Exception
    {
        public TooBigTransactionException() { }
        public TooBigTransactionException(string message) : base(message) { }
        public TooBigTransactionException(string message, System.Exception inner) : base(message, inner) { }
        protected TooBigTransactionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
