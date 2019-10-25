using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Exception
{
    [Serializable]
    public class BalanceInsufficientException : System.Exception
    {
        public BalanceInsufficientException() { }
        public BalanceInsufficientException(string message) : base(message) { }
        public BalanceInsufficientException(string message, System.Exception inner) : base(message, inner) { }
        protected BalanceInsufficientException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
