using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Exception
{
    [Serializable]
    public class SignatureFormatException : System.Exception
    {
        public SignatureFormatException() { }
        public SignatureFormatException(string message) : base(message) { }
        public SignatureFormatException(string message, System.Exception inner) : base(message, inner) { }
        protected SignatureFormatException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
