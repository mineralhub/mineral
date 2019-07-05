using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Exception
{
    [Serializable]
    public class DupTransactionException : System.Exception
    {
        public DupTransactionException() { }
        public DupTransactionException(string message) : base(message) { }
        public DupTransactionException(string message, System.Exception inner) : base(message, inner) { }
        protected DupTransactionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
