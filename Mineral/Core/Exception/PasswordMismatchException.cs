using System;
using System.Collections.Generic;
using System.Text;

namespace Mineral.Core.Exception
{
    [System.Serializable]
    public class PasswordMismatchException : System.Exception
    {
        public PasswordMismatchException() { }
        public PasswordMismatchException(string message) : base(message) { }
        public PasswordMismatchException(string message, System.Exception inner) : base(message, inner) { }
        protected PasswordMismatchException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
