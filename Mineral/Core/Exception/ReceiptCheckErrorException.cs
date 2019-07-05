using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Exception
{
    [Serializable]
    public class ReceiptCheckErrorException : System.Exception
    {
        public ReceiptCheckErrorException() { }
        public ReceiptCheckErrorException(string message) : base(message) { }
        public ReceiptCheckErrorException(string message, System.Exception inner) : base(message, inner) { }
        protected ReceiptCheckErrorException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
