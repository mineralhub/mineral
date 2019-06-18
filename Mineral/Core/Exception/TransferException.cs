using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Exception
{
    [Serializable]
    public class TransferException : System.Exception
    {
        public TransferException() { }
        public TransferException(string message) : base(message) { }
        public TransferException(string message, System.Exception inner) : base(message, inner) { }
        protected TransferException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
