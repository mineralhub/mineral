using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Exception
{
    [Serializable]
    public class TransactionExpirationException : System.Exception
    {
        public TransactionExpirationException() { }
        public TransactionExpirationException(string message) : base(message) { }
        public TransactionExpirationException(string message, System.Exception inner) : base(message, inner) { }
        protected TransactionExpirationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
