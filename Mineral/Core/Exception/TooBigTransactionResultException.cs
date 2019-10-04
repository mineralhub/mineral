using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Exception
{
    [Serializable]
    public class TooBigTransactionResultException : System.Exception
    {
        public TooBigTransactionResultException() { }
        public TooBigTransactionResultException(string message) : base(message) { }
        public TooBigTransactionResultException(string message, System.Exception inner) : base(message, inner) { }
        protected TooBigTransactionResultException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}