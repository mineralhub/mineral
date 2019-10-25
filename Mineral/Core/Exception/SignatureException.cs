using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Exception
{
    [Serializable]
    public class SignatureException : System.Exception
    {
        public SignatureException() { }
        public SignatureException(string message) : base(message) { }
        public SignatureException(string message, System.Exception inner) : base(message, inner) { }
        protected SignatureException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
