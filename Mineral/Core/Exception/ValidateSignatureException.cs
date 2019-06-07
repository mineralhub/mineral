using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Exception
{
    [Serializable]
    public class ValidateSignatureException : System.Exception
    {
        public ValidateSignatureException() { }
        public ValidateSignatureException(string message) : base(message) { }
        public ValidateSignatureException(string message, System.Exception inner) : base(message, inner) { }
        protected ValidateSignatureException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
